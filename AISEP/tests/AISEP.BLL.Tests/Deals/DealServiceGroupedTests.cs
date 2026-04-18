using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.BackgroundServices;
using AISEP.BLL.Services.Blockchain;
using AISEP.BLL.Services.Deals;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.Storage;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.DAL.Repositories.Deals;
using AISEP.DAL.Repositories.Documents;
using AISEP.DAL.Repositories.Investors;
using AISEP.DAL.Repositories.Projects;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using QuestPDF.Infrastructure;
using Sieve.Models;
using Sieve.Services;
using System.Linq.Expressions;
using Xunit;

namespace AISEP.BLL.Tests.Deals;

public class DealServiceGroupedTests
{
    static DealServiceGroupedTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private const string ValidPngDataUri =
        "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR4nGNgYAAAAAMAASsJTYQAAAAASUVORK5CYII=";

    [Fact]
    public async Task UT074_CreateDealAsync_ShouldThrow_WhenProjectIdIsNotPositive()
    {
        var (service, _, _, _, _, _, _, _, _, _, _) = CreateSut();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateDealAsync(1, new CreateDealDto { ProjectId = 0 }));

        Assert.Contains("ProjectId must be greater than 0.", ex.Message);
    }

    [Fact]
    public async Task UT075_CreateDealAsync_ShouldThrow_WhenInvestorNotFound()
    {
        var (service, _, _, investorRepo, _, _, _, _, _, _, _) = CreateSut();
        investorRepo.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Investor?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateDealAsync(999, new CreateDealDto { ProjectId = 300 }));

        Assert.Contains("Investor not found.", ex.Message);
    }

    [Fact]
    public async Task UT076_CreateDealAsync_ShouldThrow_WhenProjectNotFound()
    {
        var seedDeal = BuildDeal();
        var (service, _, _, investorRepo, projectRepo, _, _, _, _, _, _) = CreateSut(seedDeal);
        investorRepo.Setup(x => x.GetByIdAsync(seedDeal.InvestorId)).ReturnsAsync(seedDeal.Investor);
        projectRepo.Setup(x => x.GetByIdAsync(404)).ReturnsAsync((Project?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateDealAsync(seedDeal.InvestorId, new CreateDealDto { ProjectId = 404 }));

        Assert.Contains("Project not found.", ex.Message);
    }

    [Fact]
    public async Task UT077_CreateDealAsync_ShouldThrow_WhenBlockingDealExists()
    {
        var seedDeal = BuildDeal();
        var (service, _, dealRepo, investorRepo, projectRepo, _, _, _, _, _, _) = CreateSut(seedDeal);
        investorRepo.Setup(x => x.GetByIdAsync(seedDeal.InvestorId)).ReturnsAsync(seedDeal.Investor);
        projectRepo.Setup(x => x.GetByIdAsync(seedDeal.ProjectId)).ReturnsAsync(seedDeal.Project);
        dealRepo.Setup(x => x.HasBlockingDealAsync(seedDeal.InvestorId, seedDeal.ProjectId)).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateDealAsync(seedDeal.InvestorId, new CreateDealDto { ProjectId = seedDeal.ProjectId }));

        Assert.Contains("You already have an active deal", ex.Message);
    }

    [Fact]
    public async Task UT078_CreateDealAsync_ShouldSetPendingFlags_WhenCreated()
    {
        var seedDeal = BuildDeal(status: DealStatus.Pending);
        var (service, _, dealRepo, investorRepo, projectRepo, _, _, _, _, _, _) = CreateSut(seedDeal);

        Deal? addedDeal = null;
        dealRepo
            .Setup(x => x.AddAsync(It.IsAny<Deal>()))
            .Callback<Deal>(d =>
            {
                addedDeal = d;
                d.DealId = 700;
            })
            .Returns(Task.CompletedTask);

        dealRepo
            .Setup(x => x.GetByIdWithDetailsAsync(700))
            .ReturnsAsync(() =>
            {
                var loaded = BuildDeal(dealId: 700, investorId: seedDeal.InvestorId, startupId: seedDeal.Project.StartupId, status: addedDeal?.Status ?? DealStatus.Pending);
                loaded.InvestorConfirmed = addedDeal?.InvestorConfirmed ?? false;
                loaded.StartupConfirmed = addedDeal?.StartupConfirmed ?? false;
                loaded.IsCompleted = addedDeal?.IsCompleted ?? false;
                loaded.DealDate = addedDeal?.DealDate ?? DateTime.UtcNow;
                return loaded;
            });

        investorRepo.Setup(x => x.GetByIdAsync(seedDeal.InvestorId)).ReturnsAsync(seedDeal.Investor);
        projectRepo.Setup(x => x.GetByIdAsync(seedDeal.ProjectId)).ReturnsAsync(seedDeal.Project);
        dealRepo.Setup(x => x.HasBlockingDealAsync(seedDeal.InvestorId, seedDeal.ProjectId)).ReturnsAsync(false);

        var result = await service.CreateDealAsync(seedDeal.InvestorId, new CreateDealDto { ProjectId = seedDeal.ProjectId });

        Assert.NotNull(addedDeal);
        Assert.Equal(seedDeal.InvestorId, addedDeal!.InvestorId);
        Assert.True(addedDeal.InvestorConfirmed);
        Assert.False(addedDeal.StartupConfirmed);
        Assert.Equal(DealStatus.Pending, addedDeal.Status);
        Assert.False(addedDeal.IsCompleted);
        Assert.Equal(700, result.DealId);
    }

    [Fact]
    public async Task UT079_CreateDealAsync_ShouldSendNotificationToStartup_WhenCreated()
    {
        var seedDeal = BuildDeal(status: DealStatus.Pending);
        var (service, _, dealRepo, investorRepo, projectRepo, _, notification, _, _, _, _) = CreateSut(seedDeal);

        dealRepo
            .Setup(x => x.AddAsync(It.IsAny<Deal>()))
            .Callback<Deal>(d => d.DealId = 701)
            .Returns(Task.CompletedTask);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(701)).ReturnsAsync(BuildDeal(dealId: 701, investorId: seedDeal.InvestorId, startupId: seedDeal.Project.StartupId, status: DealStatus.Pending));
        investorRepo.Setup(x => x.GetByIdAsync(seedDeal.InvestorId)).ReturnsAsync(seedDeal.Investor);
        projectRepo.Setup(x => x.GetByIdAsync(seedDeal.ProjectId)).ReturnsAsync(seedDeal.Project);
        dealRepo.Setup(x => x.HasBlockingDealAsync(seedDeal.InvestorId, seedDeal.ProjectId)).ReturnsAsync(false);

        await service.CreateDealAsync(seedDeal.InvestorId, new CreateDealDto { ProjectId = seedDeal.ProjectId });

        notification.Verify(
            x => x.SendNotificationAsync(
                seedDeal.Project.Startup.UserId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                NotificationType.Deal,
                null,
                null),
            Times.Once);
    }

    [Fact]
    public async Task UT080_GetDealsAsync_ShouldReturnPagedDeals()
    {
        var deal1 = BuildDeal(dealId: 1, investorId: 10, startupId: 100, status: DealStatus.Pending);
        var deal2 = BuildDeal(dealId: 2, investorId: 11, startupId: 101, status: DealStatus.Confirmed);
        var queryDeals = new List<Deal> { deal1, deal2 };

        var (service, _, _, _, _, _, _, _, _, _, _) = CreateSut(seedDeal: deal1, queryDeals: queryDeals);

        var result = await service.GetDealsAsync(new SieveModel { Page = 1, PageSize = 10 });

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.Items.Count());
    }

    [Fact]
    public async Task UT081_GetInvestorDealsAsync_ShouldFilterByInvestorId()
    {
        var deal1 = BuildDeal(dealId: 11, investorId: 500, startupId: 100, status: DealStatus.Pending);
        var deal2 = BuildDeal(dealId: 12, investorId: 501, startupId: 100, status: DealStatus.Pending);
        var queryDeals = new List<Deal> { deal1, deal2 };

        var (service, _, _, _, _, _, _, _, _, _, _) = CreateSut(seedDeal: deal1, queryDeals: queryDeals);

        var result = await service.GetInvestorDealsAsync(500, new SieveModel { Page = 1, PageSize = 10 });

        var items = result.Items.ToList();
        Assert.Single(items);
        Assert.Equal(500, items[0].InvestorId);
    }

    [Fact]
    public async Task UT082_GetStartupDealsAsync_ShouldFilterByStartupId()
    {
        var deal1 = BuildDeal(dealId: 21, investorId: 500, startupId: 700, status: DealStatus.Pending);
        var deal2 = BuildDeal(dealId: 22, investorId: 501, startupId: 701, status: DealStatus.Pending);
        var queryDeals = new List<Deal> { deal1, deal2 };

        var (service, _, _, _, _, _, _, _, _, _, _) = CreateSut(seedDeal: deal1, queryDeals: queryDeals);

        var result = await service.GetStartupDealsAsync(700, new SieveModel { Page = 1, PageSize = 10 });

        var items = result.Items.ToList();
        Assert.Single(items);
        Assert.Equal(700, deal1.Project.StartupId);
        Assert.Equal(21, items[0].DealId);
    }

    [Fact]
    public async Task UT083_RespondDealAsync_ShouldThrow_WhenDealNotFound()
    {
        var (service, _, dealRepo, _, _, _, _, _, _, _, _) = CreateSut();
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(404)).ReturnsAsync((Deal?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.RespondDealAsync(1, 404, true));

        Assert.Contains("Deal not found.", ex.Message);
    }

    [Fact]
    public async Task UT084_RespondDealAsync_ShouldThrowForbidden_WhenStartupDoesNotOwnProject()
    {
        var deal = BuildDeal(dealId: 50, investorId: 200, startupId: 300, status: DealStatus.Pending);
        var (service, _, dealRepo, _, _, _, _, _, _, _, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(50)).ReturnsAsync(deal);

        var ex = await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.RespondDealAsync(999, 50, true));

        Assert.Contains("You do not have permission", ex.Message);
    }

    [Fact]
    public async Task UT085_RespondDealAsync_ShouldThrow_WhenDealStatusIsNotPending()
    {
        var deal = BuildDeal(dealId: 51, investorId: 200, startupId: 300, status: DealStatus.Confirmed);
        var (service, _, dealRepo, _, _, _, _, _, _, _, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(51)).ReturnsAsync(deal);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RespondDealAsync(300, 51, true));

        Assert.Contains("Only pending deals can be responded.", ex.Message);
    }

    [Fact]
    public async Task UT086_RespondDealAsync_ShouldSetConfirmed_WhenAccepted()
    {
        var deal = BuildDeal(dealId: 52, investorId: 200, startupId: 300, status: DealStatus.Pending);
        var (service, _, dealRepo, _, _, _, _, _, _, _, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(52)).ReturnsAsync(deal);

        var result = await service.RespondDealAsync(300, 52, true);

        Assert.True(deal.StartupConfirmed);
        Assert.Equal(DealStatus.Confirmed, deal.Status);
        Assert.Equal(DealStatus.Confirmed.ToString(), result.Status);
    }

    [Fact]
    public async Task UT087_RespondDealAsync_ShouldSetRejected_WhenRejected()
    {
        var deal = BuildDeal(dealId: 53, investorId: 200, startupId: 300, status: DealStatus.Pending);
        var (service, _, dealRepo, _, _, _, _, _, _, _, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(53)).ReturnsAsync(deal);

        var result = await service.RespondDealAsync(300, 53, false);

        Assert.False(deal.StartupConfirmed);
        Assert.Equal(DealStatus.Rejected, deal.Status);
        Assert.Equal(DealStatus.Rejected.ToString(), result.Status);
    }

    [Fact]
    public async Task UT088_RespondDealAsync_ShouldNotifyInvestor_WhenResponded()
    {
        var deal = BuildDeal(dealId: 54, investorId: 200, startupId: 300, status: DealStatus.Pending);
        var (service, _, dealRepo, _, _, _, notification, _, _, _, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(54)).ReturnsAsync(deal);

        await service.RespondDealAsync(300, 54, true);

        notification.Verify(
            x => x.SendNotificationAsync(
                deal.Investor.UserId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                NotificationType.Deal,
                54,
                "Deal"),
            Times.Once);
    }

    [Fact]
    public async Task UT089_GetContractPreviewForInvestorAsync_ShouldThrowForbidden_WhenInvestorDoesNotOwnDeal()
    {
        var deal = BuildDeal(dealId: 60, investorId: 200, startupId: 300, status: DealStatus.Confirmed);
        var (service, _, dealRepo, _, _, _, _, _, _, _, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(60)).ReturnsAsync(deal);

        var ex = await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.GetContractPreviewForInvestorAsync(60, 999));

        Assert.Contains("You do not have permission", ex.Message);
    }

    [Fact]
    public async Task UT090_GetContractPreviewForInvestorAsync_ShouldThrow_WhenStatusNotInSigningFlow()
    {
        var deal = BuildDeal(dealId: 61, investorId: 200, startupId: 300, status: DealStatus.Pending);
        var (service, _, dealRepo, _, _, _, _, _, _, _, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(61)).ReturnsAsync(deal);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetContractPreviewForInvestorAsync(61, 200));

        Assert.Contains("Contract preview is only available", ex.Message);
    }

    [Fact]
    public async Task UT091_GetContractPreviewForInvestorAsync_ShouldReturnHtml_WhenValid()
    {
        var deal = BuildDeal(dealId: 62, investorId: 200, startupId: 300, status: DealStatus.Confirmed);
        var (service, _, dealRepo, _, _, _, _, _, _, _, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(62)).ReturnsAsync(deal);

        var html = await service.GetContractPreviewForInvestorAsync(62, 200);

        Assert.Contains("AISEP Investment Contract", html);
        Assert.Contains("AISEP Startup", html);
        Assert.Contains("AISEP Investor Org", html);
    }

    [Fact]
    public async Task UT092_InvestorSignContractAsync_ShouldThrow_WhenFinalAmountIsNotPositive()
    {
        var deal = BuildDeal(dealId: 70, investorId: 200, startupId: 300, status: DealStatus.Confirmed);
        var (service, _, dealRepo, _, _, _, _, _, _, _, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(70)).ReturnsAsync(deal);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.InvestorSignContractAsync(70, 200, new InvestorSignContractDto
            {
                FinalAmount = 0,
                FinalEquityPercentage = 10,
                AdditionalTerms = "terms",
                SignatureBase64 = ValidPngDataUri
            }));

        Assert.Contains("FinalAmount must be greater than 0.", ex.Message);
    }

    [Fact]
    public async Task UT093_InvestorSignContractAsync_ShouldThrow_WhenFinalEquityPercentageIsNegative()
    {
        var deal = BuildDeal(dealId: 71, investorId: 200, startupId: 300, status: DealStatus.Confirmed);
        var (service, _, dealRepo, _, _, _, _, _, _, _, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(71)).ReturnsAsync(deal);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.InvestorSignContractAsync(71, 200, new InvestorSignContractDto
            {
                FinalAmount = 1000,
                FinalEquityPercentage = -0.5,
                AdditionalTerms = "terms",
                SignatureBase64 = ValidPngDataUri
            }));

        Assert.Contains("FinalEquityPercentage must be greater than or equal to 0.", ex.Message);
    }

    [Fact]
    public async Task UT094_InvestorSignContractAsync_ShouldThrow_WhenDealStatusIsNotConfirmed()
    {
        var deal = BuildDeal(dealId: 72, investorId: 200, startupId: 300, status: DealStatus.Pending);
        var (service, _, dealRepo, _, _, _, _, _, _, _, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(72)).ReturnsAsync(deal);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.InvestorSignContractAsync(72, 200, new InvestorSignContractDto
            {
                FinalAmount = 1000,
                FinalEquityPercentage = 10,
                AdditionalTerms = "terms",
                SignatureBase64 = ValidPngDataUri
            }));

        Assert.Contains("Only confirmed deals can be signed by investor.", ex.Message);
    }

    [Fact]
    public async Task UT095_InvestorSignContractAsync_ShouldSetWaitingForStartupSignature_WhenSuccessful()
    {
        var deal = BuildDeal(dealId: 73, investorId: 200, startupId: 300, status: DealStatus.Confirmed);
        var (service, unitOfWork, dealRepo, _, _, _, notification, _, _, _, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(73)).ReturnsAsync(deal);

        var result = await service.InvestorSignContractAsync(73, 200, new InvestorSignContractDto
        {
            FinalAmount = 250000,
            FinalEquityPercentage = 12.5,
            AdditionalTerms = "  milestone based payout  ",
            SignatureBase64 = ValidPngDataUri
        });

        Assert.Equal(DealStatus.Waiting_For_Startup_Signature, deal.Status);
        Assert.Equal(250000, deal.Amount);
        Assert.Equal(12.5m, deal.EquityPercentage);
        Assert.Equal("milestone based payout", deal.AdditionalTerms);
        Assert.NotNull(deal.InvestorSignedAt);
        Assert.NotNull(deal.InvestorSignature);
        Assert.Null(deal.StartupSignature);
        Assert.Null(deal.ContractPdfUrl);
        Assert.Equal(DealStatus.Waiting_For_Startup_Signature.ToString(), result.Status);

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

    [Fact]
    public async Task UT096_StartupSignContractAsync_ShouldThrow_WhenDealStatusIsNotWaitingForStartupSignature()
    {
        var deal = BuildDeal(dealId: 80, investorId: 200, startupId: 300, status: DealStatus.Confirmed);
        deal.InvestorSignature = ValidPngDataUri;

        var (service, _, dealRepo, _, _, _, _, _, _, _, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(80)).ReturnsAsync(deal);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.StartupSignContractAsync(80, 300, new StartupSignContractDto { SignatureBase64 = ValidPngDataUri }));

        Assert.Contains("Deal is not waiting for startup signature.", ex.Message);
    }

    [Fact]
    public async Task UT097_StartupSignContractAsync_ShouldThrow_WhenInvestorSignatureMissing()
    {
        var deal = BuildDeal(dealId: 81, investorId: 200, startupId: 300, status: DealStatus.Waiting_For_Startup_Signature);
        deal.InvestorSignature = null;

        var (service, _, dealRepo, _, _, _, _, _, _, _, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(81)).ReturnsAsync(deal);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.StartupSignContractAsync(81, 300, new StartupSignContractDto { SignatureBase64 = ValidPngDataUri }));

        Assert.Contains("Investor must sign first.", ex.Message);
    }

    [Fact]
    public async Task UT098_StartupSignContractAsync_ShouldThrow_WhenPdfGenerationFails()
    {
        var deal = BuildDeal(dealId: 82, investorId: 200, startupId: 300, status: DealStatus.Waiting_For_Startup_Signature);
        deal.InvestorSignature = ValidPngDataUri;
        deal.InvestorSignedAt = DateTime.UtcNow;

        var (service, _, dealRepo, _, _, documentRepo, _, _, _, _, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(82)).ReturnsAsync(deal);
        documentRepo.Setup(x => x.GetByProjectIdAsync(deal.ProjectId)).ReturnsAsync(new[]
        {
            new Document
            {
                ProjectId = deal.ProjectId,
                FileHash = "QmRegisteredHash",
                BlockchainTxHash = "0xabc"
            }
        });

        var corruptedPngBytes = new byte[]
        {
            0x89, 0x50, 0x4E, 0x47,
            0x0D, 0x0A, 0x1A, 0x0A,
            0x00, 0x00, 0x00, 0x00
        };
        var corruptedPngDataUri = "data:image/png;base64," + Convert.ToBase64String(corruptedPngBytes);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.StartupSignContractAsync(82, 300, new StartupSignContractDto { SignatureBase64 = corruptedPngDataUri }));

        Assert.Contains("not renderable by PDF engine", ex.Message);
    }

    [Fact]
    public async Task UT099_StartupSignContractAsync_ShouldThrow_WhenPdfUploadFails()
    {
        var deal = BuildDeal(dealId: 83, investorId: 200, startupId: 300, status: DealStatus.Waiting_For_Startup_Signature);
        deal.InvestorSignature = ValidPngDataUri;
        deal.InvestorSignedAt = DateTime.UtcNow;

        var (service, _, dealRepo, _, _, documentRepo, _, _, _, storage, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(83)).ReturnsAsync(deal);
        documentRepo.Setup(x => x.GetByProjectIdAsync(deal.ProjectId)).ReturnsAsync(new[]
        {
            new Document
            {
                ProjectId = deal.ProjectId,
                FileHash = "QmRegisteredHash",
                BlockchainTxHash = "0xabc"
            }
        });
        storage
            .Setup(x => x.UploadFileAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), "deal-contracts"))
            .ThrowsAsync(new InvalidOperationException("upload failed"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.StartupSignContractAsync(83, 300, new StartupSignContractDto { SignatureBase64 = ValidPngDataUri }));

        Assert.Contains("Failed to upload generated contract PDF.", ex.Message);
    }

    [Fact]
    public async Task UT100_StartupSignContractAsync_ShouldFallbackDirectBlockchainCall_WhenQueueFails()
    {
        var deal = BuildDeal(dealId: 84, investorId: 200, startupId: 300, status: DealStatus.Waiting_For_Startup_Signature);
        deal.InvestorSignature = ValidPngDataUri;
        deal.InvestorSignedAt = DateTime.UtcNow;

        var (service, _, dealRepo, _, _, documentRepo, notification, _, blockchain, storage, queue) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(84)).ReturnsAsync(deal);
        documentRepo.Setup(x => x.GetByProjectIdAsync(deal.ProjectId)).ReturnsAsync(new[]
        {
            new Document
            {
                ProjectId = deal.ProjectId,
                FileHash = "QmRegisteredHash",
                BlockchainTxHash = "0xabc"
            }
        });
        storage
            .Setup(x => x.UploadFileAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), "deal-contracts"))
            .ReturnsAsync("https://storage.test/deal-84-contract.pdf");
        queue
            .Setup(x => x.QueueAsync(It.IsAny<DocumentOwnerAssignmentWorkItem>(), It.IsAny<CancellationToken>()))
            .Throws(new InvalidOperationException("queue failed"));
        blockchain
            .Setup(x => x.AssignDocumentOwnerAsync("QmRegisteredHash", deal.Investor.WalletAddress!))
            .ReturnsAsync("0xtxhash");

        var result = await service.StartupSignContractAsync(84, 300, new StartupSignContractDto { SignatureBase64 = ValidPngDataUri });

        Assert.Equal(DealStatus.Contract_Signed.ToString(), result.Status);
        Assert.Equal(DealStatus.Contract_Signed, deal.Status);
        blockchain.Verify(
            x => x.AssignDocumentOwnerAsync("QmRegisteredHash", deal.Investor.WalletAddress!),
            Times.Once);
        notification.Verify(
            x => x.SendNotificationAsync(
                deal.Investor.UserId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                NotificationType.Deal,
                deal.DealId,
                "Deal"),
            Times.AtLeast(2));
    }

    [Fact]
    public async Task UT101_StartupSignContractAsync_ShouldSetContractSignedAndNotifyInvestor_WhenSuccessful()
    {
        var deal = BuildDeal(dealId: 85, investorId: 200, startupId: 300, status: DealStatus.Waiting_For_Startup_Signature);
        deal.InvestorSignature = ValidPngDataUri;
        deal.InvestorSignedAt = DateTime.UtcNow;

        var (service, _, dealRepo, _, _, documentRepo, notification, _, _, storage, queue) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(85)).ReturnsAsync(deal);
        documentRepo.Setup(x => x.GetByProjectIdAsync(deal.ProjectId)).ReturnsAsync(new[]
        {
            new Document
            {
                ProjectId = deal.ProjectId,
                FileHash = "QmRegisteredHash",
                BlockchainTxHash = "0xabc"
            }
        });
        storage
            .Setup(x => x.UploadFileAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), "deal-contracts"))
            .ReturnsAsync("https://storage.test/deal-85-contract.pdf");
        queue
            .Setup(x => x.QueueAsync(It.IsAny<DocumentOwnerAssignmentWorkItem>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var result = await service.StartupSignContractAsync(85, 300, new StartupSignContractDto { SignatureBase64 = ValidPngDataUri });

        Assert.Equal(DealStatus.Contract_Signed, deal.Status);
        Assert.NotNull(deal.StartupSignature);
        Assert.NotNull(deal.StartupSignedAt);
        Assert.Equal("https://storage.test/deal-85-contract.pdf", deal.ContractPdfUrl);
        Assert.Equal(DealStatus.Contract_Signed.ToString(), result.Status);

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
    public async Task UT102_StartupRejectContractAsync_ShouldThrow_WhenDealStatusIsNotWaitingForStartupSignature()
    {
        var deal = BuildDeal(dealId: 90, investorId: 200, startupId: 300, status: DealStatus.Confirmed);
        var (service, _, dealRepo, _, _, _, _, _, _, _, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(90)).ReturnsAsync(deal);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.StartupRejectContractAsync(90, 300, new StartupRejectContractDto { Reason = "Need revision" }));

        Assert.Contains("Only deals waiting for startup signature can be rejected", ex.Message);
    }

    [Fact]
    public async Task UT103_StartupRejectContractAsync_ShouldSetRejectedAndClearSignatures_WhenSuccessful()
    {
        var deal = BuildDeal(dealId: 91, investorId: 200, startupId: 300, status: DealStatus.Waiting_For_Startup_Signature);
        deal.StartupSignature = ValidPngDataUri;
        deal.StartupSignedAt = DateTime.UtcNow;
        deal.ContractPdfUrl = "https://storage.test/old.pdf";

        var (service, _, dealRepo, _, _, _, notification, _, _, _, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(91)).ReturnsAsync(deal);

        var result = await service.StartupRejectContractAsync(91, 300, new StartupRejectContractDto { Reason = "Need revision" });

        Assert.Equal(DealStatus.Rejected, deal.Status);
        Assert.Null(deal.StartupSignature);
        Assert.Null(deal.StartupSignedAt);
        Assert.Null(deal.ContractPdfUrl);
        Assert.Equal(DealStatus.Rejected.ToString(), result.Status);

        notification.Verify(
            x => x.SendNotificationAsync(
                deal.Investor.UserId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                NotificationType.Deal,
                91,
                "Deal"),
            Times.Once);
    }

    [Fact]
    public async Task UT104_GetContractStatusForInvestorAsync_ShouldThrowForbidden_WhenInvestorDoesNotOwnDeal()
    {
        var deal = BuildDeal(dealId: 92, investorId: 200, startupId: 300, status: DealStatus.Waiting_For_Startup_Signature);
        var (service, _, dealRepo, _, _, _, _, _, _, _, _) = CreateSut(deal);
        dealRepo.Setup(x => x.GetByIdWithDetailsAsync(92)).ReturnsAsync(deal);

        var ex = await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.GetContractStatusForInvestorAsync(92, 999));

        Assert.Contains("You do not have permission", ex.Message);
    }

    private static (
        DealService Service,
        Mock<IUnitOfWork> UnitOfWork,
        Mock<IDealRepository> DealRepository,
        Mock<IInvestorRepository> InvestorRepository,
        Mock<IProjectRepository> ProjectRepository,
        Mock<IDocumentRepository> DocumentRepository,
        Mock<INotificationService> NotificationService,
        Mock<IMapper> Mapper,
        Mock<IBlockchainService> BlockchainService,
        Mock<IStorageService> StorageService,
        Mock<IBlockchainOwnershipAssignmentQueue> Queue) CreateSut(
        Deal? seedDeal = null,
        List<Deal>? queryDeals = null)
    {
        seedDeal ??= BuildDeal();
        queryDeals ??= new List<Deal> { seedDeal };

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var dealRepositoryMock = new Mock<IDealRepository>();
        var investorRepositoryMock = new Mock<IInvestorRepository>();
        var projectRepositoryMock = new Mock<IProjectRepository>();
        var documentRepositoryMock = new Mock<IDocumentRepository>();
        var notificationServiceMock = new Mock<INotificationService>();
        var mapperMock = new Mock<IMapper>();
        var blockchainServiceMock = new Mock<IBlockchainService>();
        var storageServiceMock = new Mock<IStorageService>();
        var queueMock = new Mock<IBlockchainOwnershipAssignmentQueue>();
        var environmentMock = new Mock<IWebHostEnvironment>();
        var loggerMock = new Mock<ILogger<DealService>>();

        environmentMock.SetupGet(x => x.ContentRootPath).Returns(GetApiContentRootPath());

        unitOfWorkMock.SetupGet(x => x.Deals).Returns(dealRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.Investors).Returns(investorRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.Projects).Returns(projectRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.Documents).Returns(documentRepositoryMock.Object);
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
        documentRepositoryMock.Setup(x => x.GetByProjectIdAsync(It.IsAny<int>())).ReturnsAsync(Array.Empty<Document>());

        storageServiceMock
            .Setup(x => x.UploadFileAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), "deal-contracts"))
            .ReturnsAsync("https://storage.test/default-contract.pdf");

        queueMock
            .Setup(x => x.QueueAsync(It.IsAny<DocumentOwnerAssignmentWorkItem>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        mapperMock
            .Setup(x => x.Map<Deal>(It.IsAny<CreateDealDto>()))
            .Returns<CreateDealDto>(dto => new Deal
            {
                ProjectId = dto.ProjectId,
                DealDate = DateTime.UtcNow
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
                Amount = d.Amount,
                StartupConfirmed = d.StartupConfirmed,
                InvestorConfirmed = d.InvestorConfirmed,
                Status = d.Status.ToString(),
                DealDate = d.DealDate,
                EquityPercentage = d.EquityPercentage,
                AdditionalTerms = d.AdditionalTerms,
                IsCompleted = d.IsCompleted,
                ContractPdfUrl = d.ContractPdfUrl,
                InvestorSignedAt = d.InvestorSignedAt,
                StartupSignedAt = d.StartupSignedAt
            });

        mapperMock
            .Setup(x => x.Map<DealContractStatusResponse>(It.IsAny<Deal>()))
            .Returns<Deal>(d => new DealContractStatusResponse
            {
                DealId = d.DealId,
                Status = d.Status.ToString(),
                Amount = d.Amount,
                EquityPercentage = d.EquityPercentage,
                AdditionalTerms = d.AdditionalTerms,
                ContractPdfUrl = d.ContractPdfUrl,
                InvestorSignedAt = d.InvestorSignedAt,
                StartupSignedAt = d.StartupSignedAt,
                IsInvestorSigned = !string.IsNullOrWhiteSpace(d.InvestorSignature),
                IsStartupSigned = !string.IsNullOrWhiteSpace(d.StartupSignature),
                IsContractSigned = d.Status == DealStatus.Contract_Signed
            });

        var sieveProcessor = new ApplicationSieveProcessor(Options.Create(new SieveOptions()));

        var service = new DealService(
            unitOfWorkMock.Object,
            notificationServiceMock.Object,
            mapperMock.Object,
            blockchainServiceMock.Object,
            queueMock.Object,
            storageServiceMock.Object,
            sieveProcessor,
            environmentMock.Object,
            loggerMock.Object);

        return (
            service,
            unitOfWorkMock,
            dealRepositoryMock,
            investorRepositoryMock,
            projectRepositoryMock,
            documentRepositoryMock,
            notificationServiceMock,
            mapperMock,
            blockchainServiceMock,
            storageServiceMock,
            queueMock);
    }

    private static Deal BuildDeal(
        int dealId = 100,
        int investorId = 200,
        int startupId = 300,
        DealStatus status = DealStatus.Pending)
    {
        var startupUser = new User
        {
            Id = 3100,
            FullName = "AISEP Startup Rep",
            UserName = "startup.rep",
            Email = "startup@aisep.test"
        };

        var investorUser = new User
        {
            Id = 2100,
            FullName = "AISEP Investor",
            UserName = "investor.owner",
            Email = "investor@aisep.test"
        };

        var startup = new Startup
        {
            StartupId = startupId,
            UserId = startupUser.Id,
            User = startupUser,
            CompanyName = "AISEP Startup",
            Email = "contact@startup.test"
        };

        var project = new Project
        {
            ProjectId = 300,
            StartupId = startupId,
            Startup = startup,
            ProjectName = "AISEP Growth Project"
        };

        var investor = new Investor
        {
            InvestorId = investorId,
            UserId = investorUser.Id,
            User = investorUser,
            OrganizationName = "AISEP Investor Org",
            WalletAddress = "0xA1SEP123456789"
        };

        return new Deal
        {
            DealId = dealId,
            InvestorId = investorId,
            ProjectId = project.ProjectId,
            Investor = investor,
            Project = project,
            Amount = 100000,
            StartupConfirmed = status is DealStatus.Confirmed or DealStatus.Waiting_For_Startup_Signature or DealStatus.Contract_Signed,
            InvestorConfirmed = true,
            Status = status,
            DealDate = DateTime.UtcNow,
            EquityPercentage = 15,
            AdditionalTerms = "Default terms",
            IsCompleted = status == DealStatus.Contract_Signed
        };
    }

    private static string GetApiContentRootPath()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "AISEP.API"));
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

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(Expression) })!
                .MakeGenericMethod(expectedResultType)
                .Invoke(_inner, new[] { expression });

            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult })!;
        }
    }

    private sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
        {
        }

        public TestAsyncEnumerable(Expression expression) : base(expression)
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.ToList().GetEnumerator());
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
            return new ValueTask<bool>(_inner.MoveNext());
        }
    }
}
