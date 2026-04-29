using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.BackgroundServices;
using AISEP.BLL.Services.Deals;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.Storage;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.DAL.Repositories.ConnectionRequests;
using AISEP.DAL.Repositories.Deals;
using AISEP.DAL.Repositories.Investors;
using AISEP.DAL.Repositories.Projects;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Options;
using Moq;
using Sieve.Models;
using Sieve.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AISEP.BLL.Tests.Deals
{
    public class DealServiceGroupedTests
    {
        [Fact]
        public async Task UT301_CreateDealForInvestorAsync_ShouldSetPendingStatus_AndNotifyStartup()
        {
            var seedDeal = BuildDeal();
            var (service, unitOfWork, dealRepo, investorRepo, projectRepo, connectionRepo, notification, mapper, storage, queue) = CreateSut(seedDeal);

            var evidenceFile = BuildFormFile("deal-301.pdf");
            var request = new CreateDealDto { ProjectId = seedDeal.ProjectId, EvidenceFile = evidenceFile };

            dealRepo
                .Setup(x => x.AddAsync(It.IsAny<Deal>()))
                .Callback<Deal>(d => d.DealId = 701)
                .Returns(Task.CompletedTask);

            dealRepo.Setup(x => x.GetByIdWithDetailsAsync(701)).ReturnsAsync(BuildDeal(dealId: 701));
            investorRepo.Setup(x => x.GetByIdAsync(seedDeal.InvestorId)).ReturnsAsync(seedDeal.Investor);
            projectRepo.Setup(x => x.GetByIdAsync(seedDeal.ProjectId)).ReturnsAsync(seedDeal.Project);
            dealRepo.Setup(x => x.HasBlockingDealAsync(seedDeal.InvestorId, seedDeal.ProjectId)).ReturnsAsync(false);
            storage.Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), "deal-evidences")).ReturnsAsync("https://storage.test/evidence.pdf");

            var result = await service.CreateDealForInvestorAsync(seedDeal.InvestorId, request);

            Assert.Equal(701, result.DealId);
            notification.Verify(
                x => x.SendNotificationAsync(
                    seedDeal.Project.Startup.UserId,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    NotificationType.Deal,
                    701,
                    "Deal"),
                Times.Once);

            unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UT302_CreateDealForStartupAsync_ShouldThrow_WhenProjectNotOwned()
        {
            var seedDeal = BuildDeal();
            var (service, _, _, _, projectRepo, _, _, _, _, _) = CreateSut(seedDeal);

            projectRepo.Setup(x => x.GetByIdAsync(seedDeal.ProjectId)).ReturnsAsync(new Project
            {
                ProjectId = seedDeal.ProjectId,
                StartupId = seedDeal.Project.StartupId + 1,
                Startup = seedDeal.Project.Startup
            });

            var request = new CreateDealDto { ProjectId = seedDeal.ProjectId, EvidenceFile = BuildFormFile("deal-302.pdf") };

            var ex = await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                service.CreateDealForStartupAsync(seedDeal.Project.StartupId, request));

            Assert.Contains("permission", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UT303_VerifyDealForStartupAsync_ShouldMoveToPendingStaffApproval_WhenConfirmed()
        {
            var deal = BuildDeal(status: DealStatus.PendingCounterpartyConfirmation, initiatorRole: UserRole.Investor);
            deal.StartupConfirmed = false;

            var (service, unitOfWork, dealRepo, _, _, _, notification, mapper, _, _) = CreateSut(deal);
            dealRepo.Setup(x => x.GetByIdWithDetailsAsync(deal.DealId)).ReturnsAsync(deal);

            var result = await service.VerifyDealForStartupAsync(deal.Project.StartupId, deal.DealId,
                new VerifyDealRequestDto { IsConfirmed = true });

            Assert.Equal(DealStatus.PendingStaffApproval, deal.Status);
            Assert.True(deal.StartupConfirmed);
            Assert.Equal(DealStatus.PendingStaffApproval.ToString(), result.Status);
            unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
            notification.Verify(
                x => x.SendNotificationAsync(
                    deal.Investor.UserId,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    NotificationType.Deal,
                    deal.DealId,
                    "Deal"),
                Times.Once);
        }

        [Fact]
        public async Task UT304_VerifyDealForInvestorAsync_ShouldCancel_WhenRejected()
        {
            var deal = BuildDeal(status: DealStatus.PendingCounterpartyConfirmation, initiatorRole: UserRole.Startup);
            deal.InvestorConfirmed = false;

            var (service, unitOfWork, dealRepo, _, _, _, notification, mapper, _, _) = CreateSut(deal);
            dealRepo.Setup(x => x.GetByIdWithDetailsAsync(deal.DealId)).ReturnsAsync(deal);

            var result = await service.VerifyDealForInvestorAsync(deal.InvestorId, deal.DealId,
                new VerifyDealRequestDto { IsConfirmed = false, Reason = "Not valid" });

            Assert.Equal(DealStatus.Canceled, deal.Status);
            Assert.Equal(DealStatus.Canceled.ToString(), result.Status);
            unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
            notification.Verify(
                x => x.SendNotificationAsync(
                    deal.Project.Startup.UserId,
                    It.IsAny<string>(),
                    It.Is<string>(m => m.Contains("Not valid")),
                    NotificationType.Deal,
                    deal.DealId,
                    "Deal"),
                Times.Once);
        }

        [Fact]
        public async Task UT305_StaffReviewDealAsync_ShouldQueue_WhenApproved()
        {
            var deal = BuildDeal(status: DealStatus.PendingStaffApproval, initiatorRole: UserRole.Investor);
            deal.DocumentUrl = "https://storage.test/evidence.pdf";

            var (service, unitOfWork, dealRepo, _, _, _, _, _, _, queue) = CreateSut(deal);
            dealRepo.Setup(x => x.GetByIdWithDetailsAsync(deal.DealId)).ReturnsAsync(deal);

            var result = await service.StaffReviewDealAsync(deal.DealId, new StaffReviewDealRequestDto
            {
                IsApproved = true
            });

            Assert.Equal(DealStatus.ProcessingBlockchain, deal.Status);
            Assert.Equal(DealStatus.ProcessingBlockchain.ToString(), result.Status);
            queue.Verify(x => x.QueueAsync(It.Is<DocumentOwnerAssignmentWorkItem>(w => w.DealId == deal.DealId), It.IsAny<CancellationToken>()), Times.Once);
            unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UT306_StaffReviewDealAsync_ShouldRequireReupload_WhenRejected()
        {
            var deal = BuildDeal(status: DealStatus.PendingStaffApproval, initiatorRole: UserRole.Investor);

            var (service, unitOfWork, dealRepo, _, _, _, notification, _, _, _) = CreateSut(deal);
            dealRepo.Setup(x => x.GetByIdWithDetailsAsync(deal.DealId)).ReturnsAsync(deal);

            var result = await service.StaffReviewDealAsync(deal.DealId, new StaffReviewDealRequestDto
            {
                IsApproved = false,
                Reason = "Blurred file"
            });

            Assert.Equal(DealStatus.RequireReupload, deal.Status);
            Assert.Equal(DealStatus.RequireReupload.ToString(), result.Status);
            unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
            notification.Verify(
                x => x.SendNotificationAsync(
                    deal.Investor.UserId,
                    It.IsAny<string>(),
                    It.Is<string>(m => m.Contains("Blurred file")),
                    NotificationType.Deal,
                    deal.DealId,
                    "Deal"),
                Times.Once);
        }

        [Fact]
        public async Task UT307_ReuploadDealEvidenceForInvestorAsync_ShouldResetToPendingConfirmation()
        {
            var deal = BuildDeal(status: DealStatus.RequireReupload, initiatorRole: UserRole.Investor);

            var (service, unitOfWork, dealRepo, _, _, _, notification, _, storage, _) = CreateSut(deal);
            dealRepo.Setup(x => x.GetByIdWithDetailsAsync(deal.DealId)).ReturnsAsync(deal);
            storage.Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), "deal-evidences")).ReturnsAsync("https://storage.test/reupload.pdf");

            var result = await service.ReuploadDealEvidenceForInvestorAsync(deal.InvestorId, deal.DealId,
                new ReuploadDealEvidenceDto { EvidenceFile = BuildFormFile("deal-307.pdf") });

            Assert.Equal(DealStatus.PendingCounterpartyConfirmation, deal.Status);
            Assert.Equal("https://storage.test/reupload.pdf", deal.DocumentUrl);
            Assert.True(deal.InvestorConfirmed);
            Assert.False(deal.StartupConfirmed);
            Assert.Equal(DealStatus.PendingCounterpartyConfirmation.ToString(), result.Status);
            unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
            notification.Verify(
                x => x.SendNotificationAsync(
                    deal.Project.Startup.UserId,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    NotificationType.Deal,
                    deal.DealId,
                    "Deal"),
                Times.Once);
        }

        private static (DealService Service,
            Mock<IUnitOfWork> UnitOfWork,
            Mock<IDealRepository> DealRepository,
            Mock<IInvestorRepository> InvestorRepository,
            Mock<IProjectRepository> ProjectRepository,
            Mock<IConnectionRequestRepository> ConnectionRepository,
            Mock<INotificationService> NotificationService,
            Mock<IMapper> Mapper,
            Mock<IStorageService> StorageService,
            Mock<IBlockchainOwnershipAssignmentQueue> Queue) CreateSut(Deal? seedDeal = null, List<Deal>? queryDeals = null)
        {
            seedDeal ??= BuildDeal();
            queryDeals ??= new List<Deal> { seedDeal };

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var dealRepositoryMock = new Mock<IDealRepository>();
            var investorRepositoryMock = new Mock<IInvestorRepository>();
            var projectRepositoryMock = new Mock<IProjectRepository>();
            var connectionRepositoryMock = new Mock<IConnectionRequestRepository>();
            var notificationServiceMock = new Mock<INotificationService>();
            var mapperMock = new Mock<IMapper>();
            var storageServiceMock = new Mock<IStorageService>();
            var queueMock = new Mock<IBlockchainOwnershipAssignmentQueue>();

            unitOfWorkMock.SetupGet(x => x.Deals).Returns(dealRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Investors).Returns(investorRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.Projects).Returns(projectRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.ConnectionRequests).Returns(connectionRepositoryMock.Object);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            dealRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(seedDeal);
            dealRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(seedDeal);
            dealRepositoryMock.Setup(x => x.HasBlockingDealAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);
            dealRepositoryMock
                .Setup(x => x.GetQuery())
                .Returns(() => new TestAsyncEnumerable<Deal>(queryDeals.AsQueryable()));
            dealRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Deal>())).Returns(Task.CompletedTask);

            investorRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(seedDeal.Investor);
            projectRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(seedDeal.Project);
            connectionRepositoryMock.Setup(x => x.GetByStartupQuery(It.IsAny<int>()))
                .Returns(() => new TestAsyncEnumerable<ConnectionRequest>(BuildConnections(seedDeal).AsQueryable()));

            storageServiceMock
                .Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync("https://storage.test/evidence.pdf");

            queueMock
                .Setup(x => x.QueueAsync(It.IsAny<DocumentOwnerAssignmentWorkItem>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            mapperMock
                .Setup(x => x.Map<Deal>(It.IsAny<CreateDealDto>()))
                .Returns<CreateDealDto>(dto => new Deal
                {
                    ProjectId = dto.ProjectId
                });

            mapperMock
                .Setup(x => x.Map<DealDto>(It.IsAny<Deal>()))
                .Returns<Deal>(d => new DealDto
                {
                    DealId = d.DealId,
                    InvestorId = d.InvestorId,
                    InvestorName = d.Investor.OrganizationName ?? string.Empty,
                    ProjectId = d.ProjectId,
                    ProjectName = d.Project.ProjectName,
                    StartupName = d.Project.Startup.CompanyName ?? string.Empty,
                    Status = d.Status.ToString(),
                    DealDate = d.DealDate,
                    DocumentUrl = d.DocumentUrl,
                    InitiatorRole = d.InitiatorRole.ToString(),
                    StartupConfirmed = d.StartupConfirmed,
                    InvestorConfirmed = d.InvestorConfirmed,
                    IsCompleted = d.IsCompleted,
                    CompletionDate = d.CompletionDate
                });

            var sieveProcessor = new ApplicationSieveProcessor(Options.Create(new SieveOptions()));

            var service = new DealService(
                unitOfWorkMock.Object,
                notificationServiceMock.Object,
                mapperMock.Object,
                queueMock.Object,
                storageServiceMock.Object,
                sieveProcessor);

            return (
                service,
                unitOfWorkMock,
                dealRepositoryMock,
                investorRepositoryMock,
                projectRepositoryMock,
                connectionRepositoryMock,
                notificationServiceMock,
                mapperMock,
                storageServiceMock,
                queueMock);
        }

        private static IEnumerable<ConnectionRequest> BuildConnections(Deal seedDeal)
        {
            return new List<ConnectionRequest>
            {
                new()
                {
                    ConnectionRequestId = 1,
                    InvestorId = seedDeal.InvestorId,
                    Investor = seedDeal.Investor,
                    ProjectId = seedDeal.ProjectId,
                    Project = seedDeal.Project,
                    Status = ConnectionRequestStatus.Accepted,
                    ResponseDate = DateTime.UtcNow
                }
            };
        }

        private static Deal BuildDeal(
            int dealId = 100,
            int investorId = 200,
            int startupId = 300,
            DealStatus status = DealStatus.PendingCounterpartyConfirmation,
            UserRole initiatorRole = UserRole.Investor)
        {
            var startupUser = new User
            {
                Id = 3100,
                FullName = "Startup Rep",
                UserName = "startup.rep",
                Email = "startup@test.local"
            };

            var investorUser = new User
            {
                Id = 2100,
                FullName = "Investor Owner",
                UserName = "investor.owner",
                Email = "investor@test.local"
            };

            var startup = new Startup
            {
                StartupId = startupId,
                UserId = startupUser.Id,
                User = startupUser,
                CompanyName = "Startup Co",
                Email = "startup@company.test"
            };

            var project = new Project
            {
                ProjectId = 300,
                StartupId = startupId,
                Startup = startup,
                ProjectName = "Growth Project"
            };

            var investor = new Investor
            {
                InvestorId = investorId,
                UserId = investorUser.Id,
                User = investorUser,
                OrganizationName = "Investor Org",
                WalletAddress = "0xA1SEP123456789"
            };

            var deal = new Deal
            {
                DealId = dealId,
                InvestorId = investorId,
                ProjectId = project.ProjectId,
                Investor = investor,
                Project = project,
                DocumentUrl = "https://storage.test/evidence.pdf",
                InitiatorRole = initiatorRole,
                Status = status,
                DealDate = DateTime.UtcNow,
                IsCompleted = status == DealStatus.Completed
            };

            if (initiatorRole == UserRole.Investor)
            {
                deal.InvestorConfirmed = true;
                deal.StartupConfirmed = false;
            }
            else
            {
                deal.InvestorConfirmed = false;
                deal.StartupConfirmed = true;
            }

            return deal;
        }

        private static IFormFile BuildFormFile(string fileName)
        {
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            return new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };
        }

        private sealed class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            public TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new TestAsyncEnumerable<TEntity>(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new TestAsyncEnumerable<TElement>(expression);
            }

            public object? Execute(Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            {
                return new TestAsyncEnumerable<TResult>(expression);
            }

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                return Execute<TResult>(expression);
            }
        }

        private sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable)
                : base(enumerable)
            {
            }

            public TestAsyncEnumerable(Expression expression)
                : base(expression)
            {
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        private sealed class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public T Current => _inner.Current;

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return ValueTask.CompletedTask;
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return ValueTask.FromResult(_inner.MoveNext());
            }
        }
    }
}
