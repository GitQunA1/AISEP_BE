using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Services.FormValidationRules;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.Projects;
using AISEP.BLL.Services.Storage;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.DAL.Repositories.IndustryOptions;
using AISEP.DAL.Repositories.Investors;
using AISEP.DAL.Repositories.Packages;
using AISEP.DAL.Repositories.Projects;
using AISEP.DAL.Repositories.Startups;
using AISEP.DAL.Repositories.Subscriptions;
using AISEP.DAL.Repositories.StageOptions;
using AISEP.DAL.Repositories.UnlockedProjects;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Moq;
using Sieve.Services;
using Xunit;

namespace AISEP.BLL.Tests.Projects;

public class ProjectServiceGroupedTests
{
    [Fact]
    public async Task UT149_GetProjectByIdAsync_ShouldThrow_WhenProjectNotFound()
    {
        var (service, _, projectRepository, _, _, _, _, _, _, _, _, _) = CreateSut();
        projectRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Project?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetProjectByIdAsync(999));

        Assert.Contains("Project not found.", ex.Message);
    }

    [Fact]
    public async Task UT150_GetProjectByIdAsync_ShouldBypassQuota_WhenRoleIsStaffOrAdmin()
    {
        var project = BuildProject(projectId: 149, startupId: 101, startupUserId: 5001, status: ProjectStatus.Approved);

        var (service, _, projectRepository, _, _, _, unlockedProjectRepository, _, userService, _, _, _) = CreateSut();
        userService.Setup(x => x.GetUserId()).Returns(7000);
        userService.Setup(x => x.GetUserRole()).Returns("Staff");
        projectRepository.Setup(x => x.GetByIdAsync(149)).ReturnsAsync(project);

        var result = await service.GetProjectByIdAsync(149);

        Assert.NotNull(result);
        Assert.Equal(149, result!.ProjectId);
        unlockedProjectRepository.Verify(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        unlockedProjectRepository.Verify(x => x.AddAsync(It.IsAny<UnlockedProject>()), Times.Never);
    }

    [Fact]
    public async Task UT151_GetProjectByIdAsync_ShouldBypassQuota_WhenStartupOwnsProject()
    {
        var project = BuildProject(projectId: 150, startupId: 102, startupUserId: 7001, status: ProjectStatus.Approved);

        var (service, _, projectRepository, _, _, _, unlockedProjectRepository, _, userService, _, _, _) = CreateSut();
        userService.Setup(x => x.GetUserId()).Returns(7001);
        userService.Setup(x => x.GetUserRole()).Returns("Startup");
        projectRepository.Setup(x => x.GetByIdAsync(150)).ReturnsAsync(project);

        var result = await service.GetProjectByIdAsync(150);

        Assert.NotNull(result);
        Assert.Equal(150, result!.ProjectId);
        unlockedProjectRepository.Verify(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        unlockedProjectRepository.Verify(x => x.AddAsync(It.IsAny<UnlockedProject>()), Times.Never);
    }

    [Fact]
    public async Task UT152_GetProjectByIdAsync_ShouldThrow_WhenNoActiveSubscriptionForQuotaRoles()
    {
        var project = BuildProject(projectId: 151, startupId: 103, startupUserId: 5003, status: ProjectStatus.Approved);

        var (service, _, projectRepository, _, subscriptionRepository, _, unlockedProjectRepository, _, userService, _, _, _) = CreateSut();
        userService.Setup(x => x.GetUserId()).Returns(7002);
        userService.Setup(x => x.GetUserRole()).Returns("Investor");
        projectRepository.Setup(x => x.GetByIdAsync(151)).ReturnsAsync(project);
        unlockedProjectRepository.Setup(x => x.ExistsAsync(7002, 151)).ReturnsAsync(false);
        subscriptionRepository.Setup(x => x.GetLatestActiveAsync(7002)).ReturnsAsync((Subscription?)null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetProjectByIdAsync(151));

        Assert.Contains("No active subscription.", ex.Message);
    }

    [Fact]
    public async Task UT153_GetProjectByIdAsync_ShouldThrow_WhenProjectViewQuotaExceeded()
    {
        var project = BuildProject(projectId: 152, startupId: 104, startupUserId: 5004, status: ProjectStatus.Approved);
        var subscription = BuildSubscription(userId: 7003, packageId: 901, usedProjectViews: 3);
        var package = BuildPackage(packageId: 901, maxProjectViews: 3);

        var (service, _, projectRepository, _, subscriptionRepository, packageRepository, unlockedProjectRepository, _, userService, _, _, _) = CreateSut();
        userService.Setup(x => x.GetUserId()).Returns(7003);
        userService.Setup(x => x.GetUserRole()).Returns("Investor");
        projectRepository.Setup(x => x.GetByIdAsync(152)).ReturnsAsync(project);
        unlockedProjectRepository.Setup(x => x.ExistsAsync(7003, 152)).ReturnsAsync(false);
        subscriptionRepository.Setup(x => x.GetLatestActiveAsync(7003)).ReturnsAsync(subscription);
        packageRepository.Setup(x => x.GetByIdAsync(901)).ReturnsAsync(package);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetProjectByIdAsync(152));

        Assert.Contains("hết lượt xem dự án", ex.Message);
    }

    [Fact]
    public async Task UT154_GetProjectByIdAsync_ShouldConsumeQuotaAndUnlockProject_WhenFirstView()
    {
        var project = BuildProject(projectId: 153, startupId: 105, startupUserId: 5005, status: ProjectStatus.Approved);
        var subscription = BuildSubscription(userId: 7004, packageId: 902, usedProjectViews: 1);
        var package = BuildPackage(packageId: 902, maxProjectViews: 10);
        UnlockedProject? unlockedRecord = null;

        var (service, unitOfWork, projectRepository, _, subscriptionRepository, packageRepository, unlockedProjectRepository, _, userService, _, _, _) = CreateSut();
        userService.Setup(x => x.GetUserId()).Returns(7004);
        userService.Setup(x => x.GetUserRole()).Returns("Investor");
        projectRepository.Setup(x => x.GetByIdAsync(153)).ReturnsAsync(project);
        unlockedProjectRepository.Setup(x => x.ExistsAsync(7004, 153)).ReturnsAsync(false);
        unlockedProjectRepository
            .Setup(x => x.AddAsync(It.IsAny<UnlockedProject>()))
            .Callback<UnlockedProject>(record => unlockedRecord = record)
            .Returns(Task.CompletedTask);
        subscriptionRepository.Setup(x => x.GetLatestActiveAsync(7004)).ReturnsAsync(subscription);
        packageRepository.Setup(x => x.GetByIdAsync(902)).ReturnsAsync(package);

        var result = await service.GetProjectByIdAsync(153);

        Assert.NotNull(result);
        Assert.Equal(153, result!.ProjectId);
        Assert.Equal(2, subscription.UsedProjectViews);
        Assert.NotNull(unlockedRecord);
        Assert.Equal(7004, unlockedRecord!.UserId);
        Assert.Equal(153, unlockedRecord.ProjectId);

        subscriptionRepository.Verify(x => x.Update(subscription), Times.Once);
        unlockedProjectRepository.Verify(x => x.AddAsync(It.IsAny<UnlockedProject>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UT155_CreateProjectAsync_ShouldThrow_WhenStartupProfileNotFound()
    {
        var request = BuildCreateRequest();

        var (service, _, _, startupRepository, _, _, _, _, userService, _, _, _) = CreateSut();
        userService.Setup(x => x.GetUserId()).Returns(8100);
        startupRepository.Setup(x => x.GetByUserIdAsync(8100)).ReturnsAsync((Startup?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateProjectAsync(request));

        Assert.Contains("Startup profile not found", ex.Message);
    }

    [Fact]
    public async Task UT156_CreateProjectAsync_ShouldSetDraftAndUploadImage_WhenValidRequest()
    {
        var imageFile = new Mock<IFormFile>().Object;
        var request = BuildCreateRequest(imageFile);
        var startup = BuildStartup(startupId: 220, userId: 8101);
        Project? addedProject = null;

        var (service, unitOfWork, projectRepository, startupRepository, _, _, _, _, userService, storageService, _, _) = CreateSut();
        userService.Setup(x => x.GetUserId()).Returns(8101);
        startupRepository.Setup(x => x.GetByUserIdAsync(8101)).ReturnsAsync(startup);
        storageService.Setup(x => x.UploadFileAsync(imageFile, "project-images")).ReturnsAsync("https://cdn.test/project-220.png");
        projectRepository
            .Setup(x => x.AddAsync(It.IsAny<Project>()))
            .Callback<Project>(project =>
            {
                addedProject = project;
                project.ProjectId = 2200;
            })
            .Returns(Task.CompletedTask);

        var result = await service.CreateProjectAsync(request);

        Assert.NotNull(addedProject);
        Assert.Equal(220, addedProject!.StartupId);
        Assert.Equal(ProjectStatus.Draft, addedProject.Status);
        Assert.Equal(1, addedProject.StageOptionId);
        Assert.Equal(2, addedProject.IndustryOptionId);
        Assert.Equal("https://cdn.test/project-220.png", addedProject.ProjectImageUrl);
        Assert.NotNull(addedProject.Scorecard);
        Assert.Equal(TeamSizeEnum.TwoFounders, addedProject.Scorecard!.TeamSize);
        Assert.Equal(TargetMarketSizeEnum.Large, addedProject.Scorecard.TargetMarketSize);

        Assert.Equal(2200, result.ProjectId);
        Assert.Equal("AISEP Growth Platform", result.ProjectName);
        Assert.Equal(1, result.StageOptionId);
        Assert.Contains("SaaS", result.Industries);
        Assert.NotNull(result.ProjectScorecard);

        storageService.Verify(x => x.UploadFileAsync(imageFile, "project-images"), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UT157_UpdateProjectAsync_ShouldThrow_WhenProjectNotFound()
    {
        var (service, _, projectRepository, _, _, _, _, _, _, _, _, _) = CreateSut();
        projectRepository.Setup(x => x.GetByIdAsync(157)).ReturnsAsync((Project?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.UpdateProjectAsync(157, BuildUpdateRequest()));

        Assert.Contains("Project not found.", ex.Message);
    }

    [Fact]
    public async Task UT158_UpdateProjectAsync_ShouldThrowForbidden_WhenStartupDoesNotOwnProject()
    {
        var project = BuildProject(projectId: 158, startupId: 300, startupUserId: 9100, status: ProjectStatus.Draft);
        var foreignStartup = BuildStartup(startupId: 301, userId: 8102);

        var (service, _, projectRepository, startupRepository, _, _, _, _, userService, _, _, _) = CreateSut();
        userService.Setup(x => x.GetUserId()).Returns(8102);
        projectRepository.Setup(x => x.GetByIdAsync(158)).ReturnsAsync(project);
        startupRepository.Setup(x => x.GetByUserIdAsync(8102)).ReturnsAsync(foreignStartup);

        var ex = await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.UpdateProjectAsync(158, BuildUpdateRequest()));

        Assert.Contains("do not have permission", ex.Message);
    }

    [Fact]
    public async Task UT159_UpdateProjectAsync_ShouldThrow_WhenStatusIsNotDraftOrRejected()
    {
        var project = BuildProject(projectId: 159, startupId: 320, startupUserId: 8103, status: ProjectStatus.Approved);
        var startup = BuildStartup(startupId: 320, userId: 8103);

        var (service, _, projectRepository, startupRepository, _, _, _, _, userService, _, _, _) = CreateSut();
        userService.Setup(x => x.GetUserId()).Returns(8103);
        projectRepository.Setup(x => x.GetByIdAsync(159)).ReturnsAsync(project);
        startupRepository.Setup(x => x.GetByUserIdAsync(8103)).ReturnsAsync(startup);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateProjectAsync(159, BuildUpdateRequest()));

        Assert.Contains("Only draft projects or rejected projects can update.", ex.Message);
    }

    [Fact]
    public async Task UT160_UpdateProjectAsync_ShouldMoveRejectedToDraft_BeforeApplyingUpdates()
    {
        var project = BuildProject(projectId: 160, startupId: 330, startupUserId: 8104, status: ProjectStatus.Rejected);
        var startup = BuildStartup(startupId: 330, userId: 8104);
        var request = new UpdateProjectRequest
        {
            ProjectName = "Project Updated From Rejected",
            ShortDescription = "Updated short description"
        };

        var (service, unitOfWork, projectRepository, startupRepository, _, _, _, _, userService, _, _, _) = CreateSut();
        userService.Setup(x => x.GetUserId()).Returns(8104);
        projectRepository.Setup(x => x.GetByIdAsync(160)).ReturnsAsync(project);
        startupRepository.Setup(x => x.GetByUserIdAsync(8104)).ReturnsAsync(startup);

        var result = await service.UpdateProjectAsync(160, request);

        Assert.Equal(ProjectStatus.Draft, project.Status);
        Assert.Equal("Project Updated From Rejected", project.ProjectName);
        Assert.Equal("Updated short description", project.ShortDescription);
        Assert.Equal(160, result.ProjectId);

        projectRepository.Verify(x => x.Update(project), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UT161_SubmitProjectAsync_ShouldThrow_WhenProjectStatusIsNotDraft()
    {
        var project = BuildProject(projectId: 161, startupId: 340, startupUserId: 8105, status: ProjectStatus.Pending);

        var (service, _, projectRepository, _, _, _, _, _, _, _, _, _) = CreateSut();
        projectRepository.Setup(x => x.GetByIdAsync(161)).ReturnsAsync(project);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.SubmitProjectAsync(161));

        Assert.Contains("Only draft projects can be submitted", ex.Message);
    }

    [Fact]
    public async Task UT162_RejectProjectAsync_ShouldThrow_WhenProjectStatusIsNotPending()
    {
        var project = BuildProject(projectId: 162, startupId: 350, startupUserId: 8106, status: ProjectStatus.Draft);

        var (service, _, projectRepository, _, _, _, _, _, _, _, _, _) = CreateSut();
        projectRepository.Setup(x => x.GetByIdAsync(162)).ReturnsAsync(project);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RejectProjectAsync(162, new RejectProjectRequest { Reason = "Missing docs" }));

        Assert.Contains("Only Pending projects can be rejected", ex.Message);
    }

    [Fact]
    public async Task UT163_RejectProjectAsync_ShouldSetRejectedMetadata_WhenSuccessful()
    {
        var project = BuildProject(projectId: 163, startupId: 360, startupUserId: 8107, status: ProjectStatus.Pending);

        var (service, unitOfWork, projectRepository, _, _, _, _, _, userService, _, _, _) = CreateSut();
        userService.Setup(x => x.GetUserId()).Returns(9200);
        projectRepository.Setup(x => x.GetByIdAsync(163)).ReturnsAsync(project);

        await service.RejectProjectAsync(163, new RejectProjectRequest { Reason = "  Need more traction evidence  " });

        Assert.Equal(ProjectStatus.Rejected, project.Status);
        Assert.Equal("Need more traction evidence", project.RejectionReason);
        Assert.Equal(9200, project.RejectedById);
        Assert.NotNull(project.RejectedAt);

        projectRepository.Verify(x => x.Update(project), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    private static (
        ProjectService Service,
        Mock<IUnitOfWork> UnitOfWork,
        Mock<IProjectRepository> ProjectRepository,
        Mock<IStartupRepository> StartupRepository,
        Mock<ISubscriptionRepository> SubscriptionRepository,
        Mock<IPackageRepository> PackageRepository,
        Mock<IUnlockedProjectRepository> UnlockedProjectRepository,
        Mock<IInvestorRepository> InvestorRepository,
        Mock<IUserService> UserService,
        Mock<IStorageService> StorageService,
        IMapper Mapper,
        Mock<ISieveProcessor> SieveProcessor) CreateSut()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var projectRepositoryMock = new Mock<IProjectRepository>();
        var startupRepositoryMock = new Mock<IStartupRepository>();
        var subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
        var packageRepositoryMock = new Mock<IPackageRepository>();
        var unlockedProjectRepositoryMock = new Mock<IUnlockedProjectRepository>();
        var investorRepositoryMock = new Mock<IInvestorRepository>();
        var userServiceMock = new Mock<IUserService>();
        var storageServiceMock = new Mock<IStorageService>();
        var dynamicFormValidationServiceMock = new Mock<IDynamicFormSubmissionValidationService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var industryOptionRepositoryMock = new Mock<IIndustryOptionRepository>();
        var stageOptionRepositoryMock = new Mock<IStageOptionRepository>();
        var sieveProcessorMock = new Mock<ISieveProcessor>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CreateProjectRequest, Project>()
                .ForMember(dest => dest.Scorecard, opt => opt.MapFrom(src => new ProjectScorecard
                {
                    TeamSize = src.TeamSize,
                    TeamExperience = src.TeamExperience,
                    HasTechnicalCofounder = src.HasTechnicalCofounder,
                    TargetMarketSize = src.TargetMarketSize,
                    MarketGrowth = src.MarketGrowth,
                    ProductReadiness = src.ProductReadiness,
                    IPProtection = src.IPProtection,
                    BarrierToEntry = src.BarrierToEntry,
                    CurrentTraction = src.CurrentTraction,
                    RunwayMonths = src.RunwayMonths
                }));
            cfg.CreateMap<UpdateProjectRequest, Project>()
                .ForAllMembers(opt => opt.Condition((_, _, srcMember) => srcMember is not null));
            cfg.CreateMap<ProjectScorecard, ProjectScorecardDto>();
            cfg.CreateMap<Project, ProjectResponse>()
                .ForMember(d => d.ProjectScorecard, opt => opt.MapFrom(s => s.Scorecard))
                .ForMember(d => d.Industries, opt => opt.MapFrom(s => s.IndustryOption != null
                    ? new List<string> { s.IndustryOption.Value }
                    : new List<string>()))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
        });
        var mapper = mapperConfig.CreateMapper();

        var defaultProject = BuildProject(projectId: 100, startupId: 200, startupUserId: 5000, status: ProjectStatus.Approved);
        var defaultStartup = BuildStartup(startupId: 200, userId: 7000);

        unitOfWorkMock.SetupGet(x => x.Projects).Returns(projectRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.Startups).Returns(startupRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.Subscriptions).Returns(subscriptionRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.Packages).Returns(packageRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.UnlockedProjects).Returns(unlockedProjectRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.Investors).Returns(investorRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.IndustryOptions).Returns(industryOptionRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.StageOptions).Returns(stageOptionRepositoryMock.Object);
        unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        projectRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(defaultProject);
        projectRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Project>())).Returns(Task.CompletedTask);

        startupRepositoryMock.Setup(x => x.GetByUserIdAsync(It.IsAny<int>())).ReturnsAsync(defaultStartup);

        subscriptionRepositoryMock
            .Setup(x => x.GetLatestActiveAsync(It.IsAny<int>()))
            .ReturnsAsync((int userId) => BuildSubscription(userId, packageId: 800, usedProjectViews: 0));

        packageRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int packageId) => BuildPackage(packageId, maxProjectViews: 10));

        unlockedProjectRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
        unlockedProjectRepositoryMock.Setup(x => x.AddAsync(It.IsAny<UnlockedProject>())).Returns(Task.CompletedTask);

        dynamicFormValidationServiceMock
            .Setup(x => x.ValidateAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        industryOptionRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync((IEnumerable<int> ids) => ids.Select(id => new IndustryOption
            {
                Id = id,
                Value = id == 2 ? "SaaS" : "Fintech",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList());
        industryOptionRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new IndustryOption
            {
                Id = id,
                Value = id == 2 ? "SaaS" : "Fintech",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

        stageOptionRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new StageOption
            {
                Id = id,
                Value = id == 1 ? "Idea" : "Growth",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

        investorRepositoryMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int userId) => new Investor
            {
                InvestorId = 4000 + userId,
                UserId = userId,
                User = new User
                {
                    Id = userId,
                    UserName = $"investor-{userId}",
                    Email = $"investor{userId}@test.local",
                    Role = UserRole.Investor,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow
                }
            });

        userServiceMock.Setup(x => x.GetUserId()).Returns(7000);
        userServiceMock.Setup(x => x.GetUserRole()).Returns("Investor");

        storageServiceMock
            .Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), "project-images"))
            .ReturnsAsync("https://cdn.test/default-project.png");

        var service = new ProjectService(
            unitOfWorkMock.Object,
            sieveProcessorMock.Object,
            mapper,
            userServiceMock.Object,
            storageServiceMock.Object,
            dynamicFormValidationServiceMock.Object,
            notificationServiceMock.Object);

        return (
            service,
            unitOfWorkMock,
            projectRepositoryMock,
            startupRepositoryMock,
            subscriptionRepositoryMock,
            packageRepositoryMock,
            unlockedProjectRepositoryMock,
            investorRepositoryMock,
            userServiceMock,
            storageServiceMock,
            mapper,
            sieveProcessorMock);
    }

    private static Project BuildProject(int projectId, int startupId, int startupUserId, ProjectStatus status)
    {
        var startup = BuildStartup(startupId, startupUserId);

        return new Project
        {
            ProjectId = projectId,
            StartupId = startupId,
            Startup = startup,
            ProjectName = "AISEP Growth Platform",
            ProjectImageUrl = "https://cdn.test/old-image.png",
            ShortDescription = "Growth project short description",
            StageOptionId = 1,
            StageOption = new StageOption { Id = 1, Value = "Idea", IsActive = true },
            ProblemStatement = "SMEs need better data visibility.",
            SolutionDescription = "Unified analytics and decision engine.",
            TargetCustomers = "SME founders",
            Scorecard = new ProjectScorecard
            {
                TeamSize = TeamSizeEnum.TwoFounders,
                TeamExperience = TeamExperienceEnum.IndustryExpert,
                HasTechnicalCofounder = true,
                TargetMarketSize = TargetMarketSizeEnum.Large,
                MarketGrowth = MarketGrowthEnum.Fast,
                ProductReadiness = ProductReadinessEnum.MVP,
                IPProtection = IPProtectionEnum.Defensible,
                BarrierToEntry = BarrierToEntryEnum.Medium,
                CurrentTraction = CurrentTractionEnum.UserAcquisition,
                RunwayMonths = RunwayMonthsEnum.SixToTwelveMonths
            },
            IndustryOptionId = 1,
            IndustryOption = new IndustryOption { Id = 1, Value = "Fintech", IsActive = true },
            Status = status,
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        };
    }

    private static Startup BuildStartup(int startupId, int userId)
    {
        return new Startup
        {
            StartupId = startupId,
            UserId = userId,
            CompanyName = "AISEP Startup",
            Email = "startup@test.local",
            User = new User
            {
                Id = userId,
                UserName = $"startup-{userId}",
                Email = $"startup{userId}@test.local",
                Role = UserRole.Startup,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    private static Subscription BuildSubscription(int userId, int packageId, int usedProjectViews)
    {
        return new Subscription
        {
            SubscriptionId = 9000 + userId,
            UserId = userId,
            PackageId = packageId,
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(20),
            UsedProjectViews = usedProjectViews,
            RemainingFreeBookings = 0
        };
    }

    private static Package BuildPackage(int packageId, int maxProjectViews)
    {
        return new Package
        {
            PackageId = packageId,
            PackageName = "Investor Plus",
            Price = 99,
            DurationMonths = 1,
            MaxAiRequests = 100,
            MaxProjectViews = maxProjectViews,
            FreeBookingCount = 0,
            TargetRole = UserRole.Investor
        };
    }

    private static CreateProjectRequest BuildCreateRequest(IFormFile? imageFile = null)
    {
        return new CreateProjectRequest
        {
            ProjectName = "AISEP Growth Platform",
            ProjectImageFile = imageFile,
            ShortDescription = "Growth platform for startup scaling.",
            StageOptionId = 1,
            ProblemStatement = "Founders lack strategic clarity.",
            SolutionDescription = "Advisor-driven execution intelligence.",
            TargetCustomers = "Startup founders",
            IndustryOptionId = 2,
            TeamSize = TeamSizeEnum.TwoFounders,
            TeamExperience = TeamExperienceEnum.IndustryExpert,
            HasTechnicalCofounder = true,
            TargetMarketSize = TargetMarketSizeEnum.Large,
            MarketGrowth = MarketGrowthEnum.Fast,
            ProductReadiness = ProductReadinessEnum.MVP,
            IPProtection = IPProtectionEnum.Defensible,
            BarrierToEntry = BarrierToEntryEnum.Medium,
            CurrentTraction = CurrentTractionEnum.UserAcquisition,
            RunwayMonths = RunwayMonthsEnum.SixToTwelveMonths
        };
    }

    private static UpdateProjectRequest BuildUpdateRequest()
    {
        return new UpdateProjectRequest
        {
            ProjectName = "Updated AISEP Project",
            ShortDescription = "Updated short description"
        };
    }
}
