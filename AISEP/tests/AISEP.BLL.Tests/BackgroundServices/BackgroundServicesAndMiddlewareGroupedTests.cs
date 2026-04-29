using System.Reflection;
using System.Text;
using System.Text.Json;
using AISEP.API.Middleware;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.BackgroundServices;
using AISEP.BLL.Services.Blockchain;
using AISEP.BLL.Services.Bookings;
using AISEP.BLL.Services.ConsultingReports;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.ProjectAdvisorAssignments;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.DAL.Repositories.Deals;
using AISEP.DAL.Repositories.Subscriptions;
using AISEP.DAL.Repositories.Users;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AISEP.BLL.Tests.BackgroundServices;

public class BackgroundServicesAndMiddlewareGroupedTests
{
    [Fact]
    public async Task UT232_GlobalExceptionMiddleware_ShouldMapValidationException_To400BadRequest()
    {
        var validationException = new ValidationException(new[]
        {
            new ValidationFailure("File", "File is required")
        });

        var (context, payload) = await InvokeMiddlewareAndReadPayloadAsync(validationException);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(StatusCodes.Status400BadRequest, payload!.StatusCode);
        Assert.Equal("Validation failed", payload.Message);
    }

    [Fact]
    public async Task UT233_GlobalExceptionMiddleware_ShouldMapKeyNotFoundException_To404NotFound()
    {
        var (context, payload) = await InvokeMiddlewareAndReadPayloadAsync(new KeyNotFoundException("missing"));

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(StatusCodes.Status404NotFound, payload!.StatusCode);
        Assert.Equal("Not found", payload.Message);
    }

    [Fact]
    public async Task UT234_GlobalExceptionMiddleware_ShouldMapForbiddenAccessException_To403Forbidden()
    {
        var (context, payload) = await InvokeMiddlewareAndReadPayloadAsync(new ForbiddenAccessException("forbidden"));

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(StatusCodes.Status403Forbidden, payload!.StatusCode);
        Assert.Equal("Forbidden", payload.Message);
    }

    [Fact]
    public async Task UT235_GlobalExceptionMiddleware_ShouldMapInvalidOperationException_To409Conflict()
    {
        var (context, payload) = await InvokeMiddlewareAndReadPayloadAsync(new InvalidOperationException("conflict"));

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(StatusCodes.Status409Conflict, payload!.StatusCode);
        Assert.Equal("Conflict", payload.Message);
    }

    [Fact]
    public async Task UT236_GlobalExceptionMiddleware_ShouldMapHttpRequestException_To502BadGateway()
    {
        var (context, payload) = await InvokeMiddlewareAndReadPayloadAsync(new HttpRequestException("upstream"));

        Assert.Equal(StatusCodes.Status502BadGateway, context.Response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(StatusCodes.Status502BadGateway, payload!.StatusCode);
        Assert.Equal("Upstream service error", payload.Message);
    }

    [Fact]
    public async Task UT237_GlobalExceptionMiddleware_ShouldMapUnknownException_To500InternalServerError()
    {
        var (context, payload) = await InvokeMiddlewareAndReadPayloadAsync(new Exception("unexpected"));

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(StatusCodes.Status500InternalServerError, payload!.StatusCode);
        Assert.Equal("Internal Server Error", payload.Message);
        Assert.Contains("An unexpected error occurred.", payload.Errors!);
    }

    [Fact]
    public async Task UT238_GlobalExceptionMiddleware_ShouldSkipWritingResponse_WhenResponseHasStarted()
    {
        var startedBody = new MemoryStream(Encoding.UTF8.GetBytes("already-started"));
        startedBody.Seek(0, SeekOrigin.Begin);

        var responseFeature = new Mock<IHttpResponseFeature>();
        responseFeature.SetupAllProperties();
        responseFeature.SetupGet(x => x.HasStarted).Returns(true);
        responseFeature.Object.Headers = new HeaderDictionary();
        responseFeature.Object.StatusCode = StatusCodes.Status202Accepted;

        var context = new DefaultHttpContext();
        context.Features.Set<IHttpResponseFeature>(responseFeature.Object);
        context.Request.Method = "GET";
        context.Request.Path = "/api/test";
        context.Response.Body = startedBody;

        var logger = new Mock<ILogger<GlobalExceptionMiddleware>>();
        RequestDelegate next = _ => throw new Exception("boom");
        var middleware = new GlobalExceptionMiddleware(next, logger.Object);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        Assert.Equal(StatusCodes.Status202Accepted, context.Response.StatusCode);
        Assert.Equal("already-started", body);
    }

    [Fact]
    public async Task UT239_BlockchainOwnershipAssignmentBackgroundService_ShouldCompleteDeal_WhenBlockchainSucceeds()
    {
        var deal = BuildDealForBlockchain(701, "https://storage.test/evidence.pdf", "0xwallet");
        var workItem = new DocumentOwnerAssignmentWorkItem(deal.DealId);

        var cts = new CancellationTokenSource();
        var queue = new Mock<IBlockchainOwnershipAssignmentQueue>();
        queue.Setup(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .Returns<CancellationToken>(_ =>
            {
                cts.Cancel();
                return new ValueTask<DocumentOwnerAssignmentWorkItem>(workItem);
            });

        var dealRepository = new Mock<IDealRepository>();
        dealRepository.Setup(x => x.GetByIdWithDetailsAsync(deal.DealId)).ReturnsAsync(deal);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.Deals).Returns(dealRepository.Object);
        unitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var blockchainService = new Mock<IBlockchainService>();
        blockchainService
            .Setup(x => x.ComputeFileHashFromUrlAsync(deal.DocumentUrl!))
            .ReturnsAsync("0xhash");
        blockchainService
            .Setup(x => x.AssignDocumentOwnerAsync("0xhash", "0xwallet"))
            .ReturnsAsync("0xtx123");

        var notificationService = new Mock<INotificationService>();
        notificationService
            .Setup(x => x.SendNotificationAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<int?>(),
                It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        var provider = new ServiceCollection()
            .AddSingleton(unitOfWork.Object)
            .AddSingleton(blockchainService.Object)
            .AddSingleton(notificationService.Object)
            .BuildServiceProvider();

        var scopeFactory = CreateScopeFactory(provider);
        var logger = new Mock<ILogger<BlockchainOwnershipAssignmentBackgroundService>>();

        var service = new TestableBlockchainOwnershipAssignmentBackgroundService(
            queue.Object,
            scopeFactory,
            logger.Object);

        await service.RunUntilCanceledAsync(cts.Token);

        blockchainService.Verify(
            x => x.AssignDocumentOwnerAsync("0xhash", "0xwallet"),
            Times.Once);

        Assert.Equal(DealStatus.Completed, deal.Status);
        Assert.True(deal.IsCompleted);
        Assert.NotNull(deal.CompletionDate);

        notificationService.Verify(
            x => x.SendNotificationAsync(
                deal.Investor.UserId,
                It.IsAny<string>(),
                It.Is<string>(m => m.Contains("0xtx123")),
                NotificationType.Deal,
                deal.DealId,
                "Deal"),
            Times.Once);
    }

    [Fact]
    public async Task UT240_BlockchainOwnershipAssignmentBackgroundService_ShouldContinue_WhenFirstDealInvalid()
    {
        var failedDeal = BuildDealForBlockchain(1, null, "0xwallet-fail");
        var successfulDeal = BuildDealForBlockchain(2, "https://storage.test/evidence.pdf", "0xwallet-ok");

        var failedWorkItem = new DocumentOwnerAssignmentWorkItem(failedDeal.DealId);
        var successfulWorkItem = new DocumentOwnerAssignmentWorkItem(successfulDeal.DealId);

        var cts = new CancellationTokenSource();
        var dequeueCount = 0;

        var queue = new Mock<IBlockchainOwnershipAssignmentQueue>();
        queue.Setup(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .Returns<CancellationToken>(_ =>
            {
                dequeueCount++;
                return dequeueCount switch
                {
                    1 => new ValueTask<DocumentOwnerAssignmentWorkItem>(failedWorkItem),
                    _ => new ValueTask<DocumentOwnerAssignmentWorkItem>(successfulWorkItem)
                };
            });

        var dealRepository = new Mock<IDealRepository>();
        dealRepository.Setup(x => x.GetByIdWithDetailsAsync(failedDeal.DealId)).ReturnsAsync(failedDeal);
        dealRepository.Setup(x => x.GetByIdWithDetailsAsync(successfulDeal.DealId)).ReturnsAsync(successfulDeal);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.Deals).Returns(dealRepository.Object);
        unitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var blockchainService = new Mock<IBlockchainService>();
        blockchainService
            .Setup(x => x.ComputeFileHashFromUrlAsync(successfulDeal.DocumentUrl!))
            .ReturnsAsync("0xok");
        blockchainService
            .Setup(x => x.AssignDocumentOwnerAsync("0xok", "0xwallet-ok"))
            .Callback(() => cts.Cancel())
            .ReturnsAsync("0xoktx");

        var notificationService = new Mock<INotificationService>();
        notificationService
            .Setup(x => x.SendNotificationAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<int?>(),
                It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        var provider = new ServiceCollection()
            .AddSingleton(unitOfWork.Object)
            .AddSingleton(blockchainService.Object)
            .AddSingleton(notificationService.Object)
            .BuildServiceProvider();

        var scopeFactory = CreateScopeFactory(provider);
        var logger = new Mock<ILogger<BlockchainOwnershipAssignmentBackgroundService>>();

        var service = new TestableBlockchainOwnershipAssignmentBackgroundService(
            queue.Object,
            scopeFactory,
            logger.Object);

        await service.RunUntilCanceledAsync(cts.Token);

        blockchainService.Verify(
            x => x.AssignDocumentOwnerAsync("0xok", "0xwallet-ok"),
            Times.Once);

        Assert.Equal(DealStatus.BlockchainFailed, failedDeal.Status);
        Assert.Equal(DealStatus.Completed, successfulDeal.Status);

        notificationService.Verify(
            x => x.SendNotificationAsync(
                successfulDeal.Investor.UserId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                NotificationType.Deal,
                successfulDeal.DealId,
                "Deal"),
            Times.Once);
    }

    private static Deal BuildDealForBlockchain(int dealId, string? documentUrl, string? investorWallet)
    {
        var startupUser = new User { Id = 4001, UserName = "startup.user" };
        var investorUser = new User { Id = 4002, UserName = "investor.user" };

        var startup = new Startup
        {
            StartupId = 5001,
            UserId = startupUser.Id,
            User = startupUser,
            CompanyName = "Startup Co"
        };

        var project = new Project
        {
            ProjectId = 6001,
            StartupId = startup.StartupId,
            Startup = startup,
            ProjectName = "Project X"
        };

        var investor = new Investor
        {
            InvestorId = 7001,
            UserId = investorUser.Id,
            User = investorUser,
            WalletAddress = investorWallet
        };

        return new Deal
        {
            DealId = dealId,
            InvestorId = investor.InvestorId,
            ProjectId = project.ProjectId,
            Investor = investor,
            Project = project,
            DocumentUrl = documentUrl,
            Status = DealStatus.ProcessingBlockchain
        };
    }

    [Fact]
    public async Task UT241_BookingResponseExpiryBackgroundService_ShouldInvokeExpirePendingAdvisorResponses_PerCycle()
    {
        var bookingService = new Mock<IBookingService>();
        bookingService.Setup(x => x.ExpirePendingAdvisorResponsesAsync()).ReturnsAsync(2);

        var provider = new ServiceCollection()
            .AddSingleton(bookingService.Object)
            .BuildServiceProvider();

        var scopeFactory = CreateScopeFactory(provider);
        var logger = new Mock<ILogger<BookingResponseExpiryBackgroundService>>();
        var service = new BookingResponseExpiryBackgroundService(scopeFactory, logger.Object);

        await InvokePrivateAsync(service, "ProcessExpiredBookingsAsync");

        bookingService.Verify(x => x.ExpirePendingAdvisorResponsesAsync(), Times.Once);
    }

    [Fact]
    public async Task UT242_ConsultingReportDeadlineBackgroundService_ShouldInvokeProcessReportDeadlines_PerCycle()
    {
        var reportService = new Mock<IConsultingReportService>();
        reportService.Setup(x => x.ProcessReportDeadlinesAsync()).ReturnsAsync(1);

        var provider = new ServiceCollection()
            .AddSingleton(reportService.Object)
            .BuildServiceProvider();

        var scopeFactory = CreateScopeFactory(provider);
        var logger = new Mock<ILogger<ConsultingReportDeadlineBackgroundService>>();
        var service = new ConsultingReportDeadlineBackgroundService(scopeFactory, logger.Object);

        await InvokePrivateAsync(service, "ProcessDeadlinesAsync");

        reportService.Verify(x => x.ProcessReportDeadlinesAsync(), Times.Once);
    }

    [Fact]
    public async Task UT243_SubscriptionExpiryBackgroundService_ShouldMarkExpiredAndRevokePremium_WhenNoActiveSubscriptionLeft()
    {
        var subscriptions = new List<Subscription>
        {
            new() { SubscriptionId = 1, UserId = 55, Status = SubscriptionStatus.Active },
            new() { SubscriptionId = 2, UserId = 55, Status = SubscriptionStatus.Active }
        };

        var user = new User { Id = 55, IsPremium = true };

        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        subscriptionRepository.Setup(x => x.GetExpiredActiveAsync()).ReturnsAsync(subscriptions);
        subscriptionRepository.Setup(x => x.HasActiveAsync(55)).ReturnsAsync(false);

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByIdAsync(55)).ReturnsAsync(user);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.Subscriptions).Returns(subscriptionRepository.Object);
        unitOfWork.SetupGet(x => x.Users).Returns(userRepository.Object);
        unitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var provider = new ServiceCollection()
            .AddSingleton(unitOfWork.Object)
            .BuildServiceProvider();

        var scopeFactory = CreateScopeFactory(provider);
        var logger = new Mock<ILogger<SubscriptionExpiryBackgroundService>>();
        var service = new SubscriptionExpiryBackgroundService(scopeFactory, logger.Object);

        await InvokePrivateAsync(service, "ProcessExpiredSubscriptionsAsync");

        Assert.All(subscriptions, s => Assert.Equal(SubscriptionStatus.Expired, s.Status));
        Assert.False(user.IsPremium);

        subscriptionRepository.Verify(x => x.Update(It.IsAny<Subscription>()), Times.Exactly(2));
        subscriptionRepository.Verify(x => x.HasActiveAsync(55), Times.Once);
        userRepository.Verify(x => x.GetByIdAsync(55), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task UT244_ProjectAdvisorAutoAssignBackgroundService_ShouldInvokeAutoAssignUnassignedApprovedProjects_PerCycle()
    {
        var token = new CancellationTokenSource().Token;

        var autoAssignService = new Mock<IProjectAdvisorAutoAssignService>();
        autoAssignService
            .Setup(x => x.AutoAssignUnassignedApprovedProjectsAsync(token))
            .ReturnsAsync(3);

        var provider = new ServiceCollection()
            .AddSingleton(autoAssignService.Object)
            .BuildServiceProvider();

        var scopeFactory = CreateScopeFactory(provider);
        var logger = new Mock<ILogger<ProjectAdvisorAutoAssignBackgroundService>>();
        var service = new ProjectAdvisorAutoAssignBackgroundService(scopeFactory, logger.Object);

        await InvokePrivateAsync(service, "ProcessAssignmentsAsync", token);

        autoAssignService.Verify(x => x.AutoAssignUnassignedApprovedProjectsAsync(token), Times.Once);
    }

    private static IServiceScopeFactory CreateScopeFactory(IServiceProvider provider)
    {
        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory
            .Setup(x => x.CreateScope())
            .Returns(() =>
            {
                var scope = new Mock<IServiceScope>();
                scope.SetupGet(s => s.ServiceProvider).Returns(provider);
                return scope.Object;
            });

        return scopeFactory.Object;
    }

    private static async Task<(DefaultHttpContext Context, ApiResponse<object>? Payload)> InvokeMiddlewareAndReadPayloadAsync(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/test";
        context.Response.Body = new MemoryStream();

        var logger = new Mock<ILogger<GlobalExceptionMiddleware>>();
        RequestDelegate next = _ => throw exception;

        var middleware = new GlobalExceptionMiddleware(next, logger.Object);
        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var payload = await JsonSerializer.DeserializeAsync<ApiResponse<object>>(
            context.Response.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return (context, payload);
    }

    private static async Task InvokePrivateAsync(object instance, string methodName, params object?[] args)
    {
        var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);

        var invokeResult = method!.Invoke(instance, args);
        if (invokeResult is Task task)
        {
            await task;
        }
    }

    private sealed class TestableBlockchainOwnershipAssignmentBackgroundService : BlockchainOwnershipAssignmentBackgroundService
    {
        public TestableBlockchainOwnershipAssignmentBackgroundService(
            IBlockchainOwnershipAssignmentQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<BlockchainOwnershipAssignmentBackgroundService> logger)
            : base(queue, scopeFactory, logger)
        {
        }

        public Task RunUntilCanceledAsync(CancellationToken stoppingToken)
        {
            return ExecuteAsync(stoppingToken);
        }
    }
}
