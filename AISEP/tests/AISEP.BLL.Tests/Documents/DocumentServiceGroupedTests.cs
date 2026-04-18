using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Services.Blockchain;
using AISEP.BLL.Services.Documents;
using AISEP.BLL.Services.ProjectAdvisorAssignments;
using AISEP.BLL.Services.Storage;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.DAL.Repositories.Advisors;
using AISEP.DAL.Repositories.Documents;
using AISEP.DAL.Repositories.ProjectAdvisorAssignments;
using AISEP.DAL.Repositories.Projects;
using AISEP.DAL.Repositories.Startups;
using AISEP.DAL.Repositories.UnlockedProjects;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Moq;
using Sieve.Services;
using Xunit;

namespace AISEP.BLL.Tests.Documents;

public class DocumentServiceGroupedTests
{
    [Fact]
    public async Task UT164_UploadDocumentAsync_ShouldThrow_WhenProjectNotFound()
    {
        var request = BuildUploadRequest();

        var (service, _, _, projectRepository, _, _, _, _, _, _, _, _, _, _) = CreateSut();
        projectRepository.Setup(x => x.GetByIdAsync(164)).ReturnsAsync((Project?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UploadDocumentAsync(164, 7000, request));

        Assert.Contains("Project not found.", ex.Message);
    }

    [Fact]
    public async Task UT165_UploadDocumentAsync_ShouldThrowUnauthorized_WhenStartupDoesNotOwnProject()
    {
        var request = BuildUploadRequest();
        var project = BuildProject(projectId: 165, startupId: 801, startupUserId: 5001, status: ProjectStatus.Draft);
        var startup = BuildStartup(startupId: 802, userId: 7000);

        var (service, _, _, projectRepository, startupRepository, _, _, _, _, _, _, _, _, _) = CreateSut();
        projectRepository.Setup(x => x.GetByIdAsync(165)).ReturnsAsync(project);
        startupRepository.Setup(x => x.GetByUserIdAsync(7000)).ReturnsAsync(startup);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UploadDocumentAsync(165, 7000, request));

        Assert.Contains("do not have permission", ex.Message);
    }

    [Fact]
    public async Task UT166_UploadDocumentAsync_ShouldThrow_WhenProjectStatusIsNotDraft()
    {
        var request = BuildUploadRequest();
        var project = BuildProject(projectId: 166, startupId: 803, startupUserId: 7000, status: ProjectStatus.Pending);
        var startup = BuildStartup(startupId: 803, userId: 7000);

        var (service, _, _, projectRepository, startupRepository, _, _, _, _, _, _, _, _, _) = CreateSut();
        projectRepository.Setup(x => x.GetByIdAsync(166)).ReturnsAsync(project);
        startupRepository.Setup(x => x.GetByUserIdAsync(7000)).ReturnsAsync(startup);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UploadDocumentAsync(166, 7000, request));

        Assert.Contains("DRAFT", ex.Message);
    }

    [Fact]
    public async Task UT167_UploadDocumentAsync_ShouldThrow_WhenDuplicateFileHashExistsInDatabase()
    {
        var file = BuildFile("duplicate-pitch.pdf");
        var request = BuildUploadRequest(file);
        var project = BuildProject(projectId: 167, startupId: 804, startupUserId: 7000, status: ProjectStatus.Draft);
        var startup = BuildStartup(startupId: 804, userId: 7000);
        var existing = BuildDocument(documentId: 1, projectId: 555, projectStatus: ProjectStatus.Approved, projectStartupId: 900, fileHash: "0xdup-hash", txHash: "0xtx");

        var (service, _, documentRepository, projectRepository, startupRepository, _, blockchainService, storageService, _, _, _, _, _, _) = CreateSut();
        projectRepository.Setup(x => x.GetByIdAsync(167)).ReturnsAsync(project);
        startupRepository.Setup(x => x.GetByUserIdAsync(7000)).ReturnsAsync(startup);
        blockchainService.Setup(x => x.ComputeFileHashAsync(file)).ReturnsAsync("0xdup-hash");
        documentRepository.Setup(x => x.GetQueryable()).Returns(new List<Document> { existing }.AsQueryable());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UploadDocumentAsync(167, 7000, request));

        Assert.Contains("upload", ex.Message, StringComparison.OrdinalIgnoreCase);
        blockchainService.Verify(x => x.VerifyDocumentAsync(It.IsAny<string>()), Times.Never);
        storageService.Verify(x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UT168_UploadDocumentAsync_ShouldThrow_WhenFileHashAlreadyExistsOnBlockchain()
    {
        var file = BuildFile("onchain-pitch.pdf");
        var request = BuildUploadRequest(file);
        var project = BuildProject(projectId: 168, startupId: 805, startupUserId: 7000, status: ProjectStatus.Draft);
        var startup = BuildStartup(startupId: 805, userId: 7000);

        var (service, _, documentRepository, projectRepository, startupRepository, _, blockchainService, storageService, _, _, _, _, _, _) = CreateSut();
        projectRepository.Setup(x => x.GetByIdAsync(168)).ReturnsAsync(project);
        startupRepository.Setup(x => x.GetByUserIdAsync(7000)).ReturnsAsync(startup);
        blockchainService.Setup(x => x.ComputeFileHashAsync(file)).ReturnsAsync("0xonchain-hash");
        documentRepository.Setup(x => x.GetQueryable()).Returns(new List<Document>().AsQueryable());
        blockchainService
            .Setup(x => x.VerifyDocumentAsync("0xonchain-hash"))
            .ReturnsAsync((805, 1_711_111_111L, (IReadOnlyList<string>)Array.Empty<string>()));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UploadDocumentAsync(168, 7000, request));

        Assert.Contains("blockchain", ex.Message, StringComparison.OrdinalIgnoreCase);
        storageService.Verify(x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UT169_UploadDocumentAsync_ShouldWrapError_WhenBlockchainVerifyFailsUnexpectedly()
    {
        var file = BuildFile("verify-error.pdf");
        var request = BuildUploadRequest(file);
        var project = BuildProject(projectId: 169, startupId: 806, startupUserId: 7000, status: ProjectStatus.Draft);
        var startup = BuildStartup(startupId: 806, userId: 7000);

        var (service, _, documentRepository, projectRepository, startupRepository, _, blockchainService, _, _, _, _, _, _, _) = CreateSut();
        projectRepository.Setup(x => x.GetByIdAsync(169)).ReturnsAsync(project);
        startupRepository.Setup(x => x.GetByUserIdAsync(7000)).ReturnsAsync(startup);
        blockchainService.Setup(x => x.ComputeFileHashAsync(file)).ReturnsAsync("0xverify-error");
        documentRepository.Setup(x => x.GetQueryable()).Returns(new List<Document>().AsQueryable());
        blockchainService
            .Setup(x => x.VerifyDocumentAsync("0xverify-error"))
            .ThrowsAsync(new Exception("rpc timeout"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UploadDocumentAsync(169, 7000, request));

        Assert.Contains("blockchain", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rpc timeout", ex.Message);
    }

    [Fact]
    public async Task UT170_UploadDocumentAsync_ShouldPersistDocumentWithHashAndUrl_WhenValid()
    {
        var file = BuildFile("pitch-170.pdf");
        var request = BuildUploadRequest(file);
        var project = BuildProject(projectId: 170, startupId: 807, startupUserId: 7000, status: ProjectStatus.Draft);
        var startup = BuildStartup(startupId: 807, userId: 7000);
        Document? addedDocument = null;

        var (service, unitOfWork, documentRepository, projectRepository, startupRepository, _, blockchainService, storageService, _, _, _, _, _, _) = CreateSut();
        projectRepository.Setup(x => x.GetByIdAsync(170)).ReturnsAsync(project);
        startupRepository.Setup(x => x.GetByUserIdAsync(7000)).ReturnsAsync(startup);
        blockchainService.Setup(x => x.ComputeFileHashAsync(file)).ReturnsAsync("0xhash-170");
        documentRepository.Setup(x => x.GetQueryable()).Returns(new List<Document>().AsQueryable());
        blockchainService
            .Setup(x => x.VerifyDocumentAsync("0xhash-170"))
            .ReturnsAsync((0, 0L, (IReadOnlyList<string>)Array.Empty<string>()));
        storageService.Setup(x => x.UploadFileAsync(file, "aisep-documents")).ReturnsAsync("https://cdn.test/pitch-170.pdf");
        documentRepository
            .Setup(x => x.AddAsync(It.IsAny<Document>()))
            .Callback<Document>(document =>
            {
                addedDocument = document;
                document.DocumentId = 1700;
            })
            .Returns(Task.CompletedTask);

        var result = await service.UploadDocumentAsync(170, 7000, request);

        Assert.NotNull(addedDocument);
        Assert.Equal(170, addedDocument!.ProjectId);
        Assert.Equal(DocumentType.PitchDeck, addedDocument.DocumentType);
        Assert.Equal("pitch-170.pdf", addedDocument.FileName);
        Assert.Equal("https://cdn.test/pitch-170.pdf", addedDocument.FileUrl);
        Assert.Equal("0xhash-170", addedDocument.FileHash);
        Assert.Null(addedDocument.BlockchainTxHash);
        Assert.False(addedDocument.IsIpProtected);

        Assert.Equal(1700, result.DocumentId);
        Assert.Equal("0xhash-170", result.FileHash);
        Assert.Equal("https://cdn.test/pitch-170.pdf", result.FileUrl);

        storageService.Verify(x => x.UploadFileAsync(file, "aisep-documents"), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UT171_GetByIdAsync_ShouldReturnNull_WhenDocumentNotFound()
    {
        var (service, _, documentRepository, _, _, _, _, _, _, _, _, _, _, _) = CreateSut();
        documentRepository.Setup(x => x.GetByIdAsync(171)).ReturnsAsync((Document?)null);

        var result = await service.GetByIdAsync(171, 7000, "Investor");

        Assert.Null(result);
    }

    [Fact]
    public async Task UT172_GetByIdAsync_ShouldThrowUnauthorized_WhenUserCannotViewProjectDocuments()
    {
        var project = BuildProject(projectId: 901, startupId: 808, startupUserId: 5008, status: ProjectStatus.Approved);
        var document = BuildDocument(documentId: 172, projectId: 901, projectStatus: ProjectStatus.Approved, projectStartupId: 808, fileHash: "0xhash-172", txHash: "0xtx-172");

        var (service, _, documentRepository, projectRepository, _, unlockedProjectRepository, _, _, _, _, _, _, _, _) = CreateSut();
        documentRepository.Setup(x => x.GetByIdAsync(172)).ReturnsAsync(document);
        projectRepository.Setup(x => x.GetByIdAsync(901)).ReturnsAsync(project);
        unlockedProjectRepository.Setup(x => x.ExistsAsync(7001, 901)).ReturnsAsync(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetByIdAsync(172, 7001, "Investor"));
    }

    [Fact]
    public async Task UT173_DeleteAsync_ShouldReturnFalse_WhenDocumentNotFound()
    {
        var (service, unitOfWork, documentRepository, _, _, _, _, _, _, _, _, _, _, _) = CreateSut();
        documentRepository.Setup(x => x.GetByIdAsync(173)).ReturnsAsync((Document?)null);

        var deleted = await service.DeleteAsync(173, 7000, "Admin");

        Assert.False(deleted);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UT174_DeleteAsync_ShouldThrowUnauthorized_WhenStartupCannotDeleteForeignDocument()
    {
        var document = BuildDocument(documentId: 174, projectId: 902, projectStatus: ProjectStatus.Draft, projectStartupId: 810, fileHash: "0xhash-174", txHash: "0xtx-174");
        var foreignStartup = BuildStartup(startupId: 811, userId: 7000);

        var (service, _, documentRepository, _, startupRepository, _, _, _, _, _, _, _, _, _) = CreateSut();
        documentRepository.Setup(x => x.GetByIdAsync(174)).ReturnsAsync(document);
        startupRepository.Setup(x => x.GetByUserIdAsync(7000)).ReturnsAsync(foreignStartup);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteAsync(174, 7000, "Startup"));

        Assert.Contains("do not have permission", ex.Message);
    }

    [Fact]
    public async Task UT175_DeleteAsync_ShouldThrow_WhenProjectIsLockedByApprovedStatus()
    {
        var document = BuildDocument(documentId: 175, projectId: 903, projectStatus: ProjectStatus.Approved, projectStartupId: 812, fileHash: "0xhash-175", txHash: "0xtx-175");

        var (service, _, documentRepository, _, _, _, _, _, _, _, _, _, _, _) = CreateSut();
        documentRepository.Setup(x => x.GetByIdAsync(175)).ReturnsAsync(document);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(175, 7000, "Admin"));

        Assert.Contains("locked", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UT176_DeleteAsync_ShouldDeleteDocument_WhenAllowed()
    {
        var document = BuildDocument(documentId: 176, projectId: 904, projectStatus: ProjectStatus.Draft, projectStartupId: 813, fileHash: "0xhash-176", txHash: "0xtx-176");

        var (service, unitOfWork, documentRepository, _, _, _, _, _, _, _, _, _, _, _) = CreateSut();
        documentRepository.Setup(x => x.GetByIdAsync(176)).ReturnsAsync(document);

        var deleted = await service.DeleteAsync(176, 7000, "Admin");

        Assert.True(deleted);
        documentRepository.Verify(x => x.Delete(document), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UT177_VerifyDocumentAsync_ShouldThrow_WhenDocumentNotFound()
    {
        var (service, _, documentRepository, _, _, _, _, _, _, _, _, _, _, _) = CreateSut();
        documentRepository.Setup(x => x.GetByIdAsync(177)).ReturnsAsync((Document?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.VerifyDocumentAsync(177));

        Assert.Contains("177", ex.Message);
    }

    [Fact]
    public async Task UT178_VerifyDocumentAsync_ShouldThrow_WhenDocumentNotRegisteredOnBlockchain()
    {
        var document = BuildDocument(documentId: 178, projectId: 905, projectStatus: ProjectStatus.Approved, projectStartupId: 814, fileHash: "0xhash-178", txHash: null);

        var (service, _, documentRepository, _, _, _, blockchainService, _, _, _, _, _, _, _) = CreateSut();
        documentRepository.Setup(x => x.GetByIdAsync(178)).ReturnsAsync(document);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.VerifyDocumentAsync(178));

        Assert.Contains("not registered on the blockchain", ex.Message);
        blockchainService.Verify(x => x.VerifyDocumentAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UT179_ApproveProjectAsync_ShouldThrow_WhenProjectStatusIsNotPending()
    {
        var project = BuildProject(projectId: 179, startupId: 815, startupUserId: 7000, status: ProjectStatus.Draft);

        var (service, _, _, projectRepository, _, _, _, _, _, _, _, _, _, _) = CreateSut();
        projectRepository.Setup(x => x.GetByIdAsync(179)).ReturnsAsync(project);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApproveProjectAsync(179));

        Assert.Contains("Pending", ex.Message);
    }

    [Fact]
    public async Task UT180_ApproveProjectAsync_ShouldThrow_WhenNoProjectDocumentExists()
    {
        var project = BuildProject(projectId: 180, startupId: 816, startupUserId: 7000, status: ProjectStatus.Pending);

        var (service, _, documentRepository, projectRepository, _, _, _, _, _, _, _, _, _, _) = CreateSut();
        projectRepository.Setup(x => x.GetByIdAsync(180)).ReturnsAsync(project);
        documentRepository.Setup(x => x.GetQueryable()).Returns(new List<Document>().AsQueryable());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApproveProjectAsync(180));

        Assert.Contains("upload", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UT181_ApproveProjectAsync_ShouldRegisterHashAndApproveProject_WhenValid()
    {
        var project = BuildProject(projectId: 181, startupId: 817, startupUserId: 7000, status: ProjectStatus.Pending);
        var document = BuildDocument(documentId: 1810, projectId: 181, projectStatus: ProjectStatus.Pending, projectStartupId: 817, fileHash: "0xhash-181", txHash: null);

        var (service, unitOfWork, documentRepository, projectRepository, _, _, blockchainService, _, userService, projectAdvisorAutoAssignService, _, _, _, _) = CreateSut();
        projectRepository.Setup(x => x.GetByIdAsync(181)).ReturnsAsync(project);
        documentRepository.Setup(x => x.GetQueryable()).Returns(new List<Document> { document }.AsQueryable());
        blockchainService.Setup(x => x.RegisterDocumentAsync("0xhash-181", 181)).ReturnsAsync("0xtx-181");
        userService.Setup(x => x.GetUserId()).Returns(8801);

        var result = await service.ApproveProjectAsync(181);

        Assert.Equal(ProjectStatus.Approved, project.Status);
        Assert.Equal(8801, project.ApprovedById);
        Assert.NotNull(project.ApprovedAt);

        Assert.Equal("0xtx-181", document.BlockchainTxHash);
        Assert.True(document.IsIpProtected);
        Assert.NotNull(document.VerifiedAt);

        Assert.Equal(1810, result.DocumentId);
        Assert.Equal("0xtx-181", result.BlockchainTxHash);
        Assert.True(result.IsIpProtected);

        documentRepository.Verify(x => x.Update(document), Times.Once);
        projectRepository.Verify(x => x.Update(project), Times.Once);
        projectAdvisorAutoAssignService.Verify(x => x.TryAssignAdvisorAsync(project, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    private static (
        DocumentService Service,
        Mock<IUnitOfWork> UnitOfWork,
        Mock<IDocumentRepository> DocumentRepository,
        Mock<IProjectRepository> ProjectRepository,
        Mock<IStartupRepository> StartupRepository,
        Mock<IUnlockedProjectRepository> UnlockedProjectRepository,
        Mock<IBlockchainService> BlockchainService,
        Mock<IStorageService> StorageService,
        Mock<IUserService> UserService,
        Mock<IProjectAdvisorAutoAssignService> ProjectAdvisorAutoAssignService,
        IMapper Mapper,
        Mock<ISieveProcessor> SieveProcessor,
        Mock<IAdvisorsRepository> AdvisorsRepository,
        Mock<IProjectAdvisorAssignmentRepository> ProjectAdvisorAssignmentRepository) CreateSut()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var documentRepositoryMock = new Mock<IDocumentRepository>();
        var projectRepositoryMock = new Mock<IProjectRepository>();
        var startupRepositoryMock = new Mock<IStartupRepository>();
        var unlockedProjectRepositoryMock = new Mock<IUnlockedProjectRepository>();
        var blockchainServiceMock = new Mock<IBlockchainService>();
        var storageServiceMock = new Mock<IStorageService>();
        var userServiceMock = new Mock<IUserService>();
        var projectAdvisorAutoAssignServiceMock = new Mock<IProjectAdvisorAutoAssignService>();
        var sieveProcessorMock = new Mock<ISieveProcessor>();
        var advisorsRepositoryMock = new Mock<IAdvisorsRepository>();
        var projectAdvisorAssignmentRepositoryMock = new Mock<IProjectAdvisorAssignmentRepository>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Document, DocumentResponse>();
        });
        var mapper = mapperConfig.CreateMapper();

        var defaultProject = BuildProject(projectId: 100, startupId: 101, startupUserId: 7000, status: ProjectStatus.Draft);

        unitOfWorkMock.SetupGet(x => x.Documents).Returns(documentRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.Projects).Returns(projectRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.Startups).Returns(startupRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.UnlockedProjects).Returns(unlockedProjectRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.Advisors).Returns(advisorsRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.ProjectAdvisorAssignments).Returns(projectAdvisorAssignmentRepositoryMock.Object);
        unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        documentRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Document?)null);
        documentRepositoryMock.Setup(x => x.GetQueryable()).Returns(new List<Document>().AsQueryable());
        documentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Document>())).Returns(Task.CompletedTask);

        projectRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(defaultProject);
        startupRepositoryMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int userId) => BuildStartup(startupId: 101, userId: userId));

        unlockedProjectRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);

        blockchainServiceMock.Setup(x => x.ComputeFileHashAsync(It.IsAny<IFormFile>())).ReturnsAsync("0xdefault-hash");
        blockchainServiceMock
            .Setup(x => x.VerifyDocumentAsync(It.IsAny<string>()))
            .ReturnsAsync((0, 0L, (IReadOnlyList<string>)Array.Empty<string>()));
        blockchainServiceMock.Setup(x => x.RegisterDocumentAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync("0xdefault-tx");

        storageServiceMock
            .Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), "aisep-documents"))
            .ReturnsAsync("https://cdn.test/default-document.pdf");

        userServiceMock.Setup(x => x.GetUserId()).Returns(7000);
        projectAdvisorAutoAssignServiceMock
            .Setup(x => x.TryAssignAdvisorAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        advisorsRepositoryMock.Setup(x => x.GetByUserIdAsync(It.IsAny<int>())).ReturnsAsync((Advisor?)null);
        projectAdvisorAssignmentRepositoryMock
            .Setup(x => x.GetByAdvisorIdQuery(It.IsAny<int>()))
            .Returns(new List<ProjectAdvisorAssignment>().AsQueryable());

        var service = new DocumentService(
            unitOfWorkMock.Object,
            storageServiceMock.Object,
            blockchainServiceMock.Object,
            sieveProcessorMock.Object,
            mapper,
            userServiceMock.Object,
            projectAdvisorAutoAssignServiceMock.Object);

        return (
            service,
            unitOfWorkMock,
            documentRepositoryMock,
            projectRepositoryMock,
            startupRepositoryMock,
            unlockedProjectRepositoryMock,
            blockchainServiceMock,
            storageServiceMock,
            userServiceMock,
            projectAdvisorAutoAssignServiceMock,
            mapper,
            sieveProcessorMock,
            advisorsRepositoryMock,
            projectAdvisorAssignmentRepositoryMock);
    }

    private static Project BuildProject(int projectId, int startupId, int startupUserId, ProjectStatus status)
    {
        return new Project
        {
            ProjectId = projectId,
            StartupId = startupId,
            ProjectName = "AISEP Document Project",
            Industry = Industry.SaaS,
            Status = status,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            Startup = BuildStartup(startupId, startupUserId)
        };
    }

    private static Startup BuildStartup(int startupId, int userId)
    {
        return new Startup
        {
            StartupId = startupId,
            UserId = userId,
            CompanyName = "AISEP Startup",
            Email = $"startup{startupId}@test.local",
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

    private static Document BuildDocument(
        int documentId,
        int projectId,
        ProjectStatus projectStatus,
        int projectStartupId,
        string? fileHash,
        string? txHash)
    {
        return new Document
        {
            DocumentId = documentId,
            ProjectId = projectId,
            DocumentType = DocumentType.PitchDeck,
            FileName = $"doc-{documentId}.pdf",
            FileUrl = $"https://cdn.test/doc-{documentId}.pdf",
            FileHash = fileHash,
            BlockchainTxHash = txHash,
            IsIpProtected = txHash is not null,
            Project = BuildProject(projectId, projectStartupId, startupUserId: 5000 + projectStartupId, status: projectStatus)
        };
    }

    private static UploadDocumentRequest BuildUploadRequest(IFormFile? file = null)
    {
        return new UploadDocumentRequest
        {
            DocumentType = DocumentType.PitchDeck,
            File = file ?? BuildFile("pitch-deck.pdf")
        };
    }

    private static IFormFile BuildFile(string fileName)
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.SetupGet(x => x.FileName).Returns(fileName);
        fileMock.SetupGet(x => x.Length).Returns(128);
        return fileMock.Object;
    }
}
