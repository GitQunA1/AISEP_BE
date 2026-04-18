using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.Payments;
using AISEP.BLL.Settings;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.DAL.Repositories.Bookings;
using AISEP.DAL.Repositories.Packages;
using AISEP.DAL.Repositories.Transactions;
using AISEP.DAL.Repositories.Users;
using AutoMapper;
using Microsoft.Extensions.Options;
using Moq;
using Sieve.Services;
using Xunit;

namespace AISEP.BLL.Tests.Payments;

public class PaymentServiceGroupedTests
{
    [Fact]
    public async Task UT038_GetInvestorPackagesAsync_ShouldReturnOnlyInvestorPackages()
    {
        var investorPackages = new[]
        {
            BuildPackage(1, "Investor Basic", UserRole.Investor),
            BuildPackage(2, "Investor Pro", UserRole.Investor)
        };

        var (service, _, packageRepo, _, _, _, _, _) = CreateSut();
        packageRepo.Setup(x => x.GetByRoleAsync(UserRole.Investor)).ReturnsAsync(investorPackages);

        var result = (await service.GetInvestorPackagesAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, x => Assert.Contains("Investor", x.PackageName));
        packageRepo.Verify(x => x.GetByRoleAsync(UserRole.Investor), Times.Once);
    }

    [Fact]
    public async Task UT039_GetStartupPackagesAsync_ShouldReturnOnlyStartupPackages()
    {
        var startupPackages = new[]
        {
            BuildPackage(3, "Startup Basic", UserRole.Startup),
            BuildPackage(4, "Startup Growth", UserRole.Startup)
        };

        var (service, _, packageRepo, _, _, _, _, _) = CreateSut();
        packageRepo.Setup(x => x.GetByRoleAsync(UserRole.Startup)).ReturnsAsync(startupPackages);

        var result = (await service.GetStartupPackagesAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, x => Assert.Contains("Startup", x.PackageName));
        packageRepo.Verify(x => x.GetByRoleAsync(UserRole.Startup), Times.Once);
    }

    [Fact]
    public async Task UT040_CheckoutSubscriptionAsync_ShouldThrow_WhenPackageIdIsNotPositive()
    {
        var (service, _, _, _, _, _, _, _) = CreateSut();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CheckoutSubscriptionAsync(10, 0));

        Assert.Contains("PackageId must be greater than 0.", ex.Message);
    }

    [Fact]
    public async Task UT041_CheckoutSubscriptionAsync_ShouldThrow_WhenPackageNotFound()
    {
        var (service, _, packageRepo, _, _, _, _, _) = CreateSut();
        packageRepo.Setup(x => x.GetByIdAsync(404)).ReturnsAsync((Package?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CheckoutSubscriptionAsync(10, 404));

        Assert.Contains("Package not found.", ex.Message);
    }

    [Fact]
    public async Task UT042_CheckoutSubscriptionAsync_ShouldThrow_WhenUserNotFound()
    {
        var package = BuildPackage(100, "Investor Basic", UserRole.Investor);
        var (service, _, packageRepo, _, _, userRepo, _, _) = CreateSut();
        packageRepo.Setup(x => x.GetByIdAsync(100)).ReturnsAsync(package);
        userRepo.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((User?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CheckoutSubscriptionAsync(99, 100));

        Assert.Contains("User not found.", ex.Message);
    }

    [Fact]
    public async Task UT043_CheckoutSubscriptionAsync_ShouldThrow_WhenPackageRoleMismatch()
    {
        var startupPackage = BuildPackage(110, "Startup Plan", UserRole.Startup);
        var investorUser = BuildUser(22, UserRole.Investor);

        var (service, _, packageRepo, _, _, userRepo, _, _) = CreateSut();
        packageRepo.Setup(x => x.GetByIdAsync(110)).ReturnsAsync(startupPackage);
        userRepo.Setup(x => x.GetByIdAsync(22)).ReturnsAsync(investorUser);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CheckoutSubscriptionAsync(22, 110));

        Assert.Contains("Selected package is not available for your role.", ex.Message);
    }

    [Fact]
    public async Task UT044_CheckoutSubscriptionAsync_ShouldReusePendingTransaction_WhenPendingNotExpired()
    {
        var package = BuildPackage(120, "Investor Plus", UserRole.Investor, price: 500);
        var user = BuildUser(23, UserRole.Investor);
        var existingPending = BuildPendingTransaction(700, 23, ReferenceType.Subscription, 120, 500, DateTime.UtcNow.AddMinutes(-5));
        existingPending.PaymentCode = "AISEP700";

        var (service, _, packageRepo, transactionRepo, _, userRepo, _, _) = CreateSut();
        packageRepo.Setup(x => x.GetByIdAsync(120)).ReturnsAsync(package);
        userRepo.Setup(x => x.GetByIdAsync(23)).ReturnsAsync(user);
        transactionRepo
            .Setup(x => x.GetPendingByUserAndReferenceAsync(23, ReferenceType.Subscription.ToString(), 120))
            .ReturnsAsync(existingPending);

        var result = await service.CheckoutSubscriptionAsync(23, 120);

        Assert.Equal(700, result.TransactionId);
        Assert.Equal("AISEP700", result.PaymentCode);
        Assert.Contains("addInfo=AISEP700", result.QrCodeUrl);
        transactionRepo.Verify(x => x.AddAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task UT045_CheckoutSubscriptionAsync_ShouldFailOldPendingAndCreateNew_WhenPendingExpired()
    {
        var package = BuildPackage(130, "Investor Premium", UserRole.Investor, price: 900);
        var user = BuildUser(24, UserRole.Investor);
        var expiredPending = BuildPendingTransaction(701, 24, ReferenceType.Subscription, 130, 900, DateTime.UtcNow.AddMinutes(-45));
        expiredPending.PaymentCode = "AISEP701";

        Transaction? createdTransaction = null;
        var (service, unitOfWork, packageRepo, transactionRepo, _, userRepo, _, _) = CreateSut();
        packageRepo.Setup(x => x.GetByIdAsync(130)).ReturnsAsync(package);
        userRepo.Setup(x => x.GetByIdAsync(24)).ReturnsAsync(user);
        transactionRepo
            .Setup(x => x.GetPendingByUserAndReferenceAsync(24, ReferenceType.Subscription.ToString(), 130))
            .ReturnsAsync(expiredPending);
        transactionRepo
            .Setup(x => x.AddAsync(It.IsAny<Transaction>()))
            .Callback<Transaction>(t =>
            {
                createdTransaction = t;
                t.TransactionId = 801;
            })
            .Returns(Task.CompletedTask);

        var result = await service.CheckoutSubscriptionAsync(24, 130);

        Assert.Equal(TransactionStatus.Failed, expiredPending.Status);
        Assert.NotNull(createdTransaction);
        Assert.Equal("AISEP801", result.PaymentCode);
        transactionRepo.Verify(x => x.Update(expiredPending), Times.Once);
        transactionRepo.Verify(x => x.AddAsync(It.IsAny<Transaction>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Exactly(3));
    }

    [Fact]
    public async Task UT046_CheckoutSubscriptionAsync_ShouldGeneratePaymentCodeWithPrefix_WhenCreated()
    {
        var package = BuildPackage(140, "Investor Gold", UserRole.Investor, price: 300);
        var user = BuildUser(25, UserRole.Investor);

        var (service, _, packageRepo, transactionRepo, _, userRepo, _, _) = CreateSut();
        packageRepo.Setup(x => x.GetByIdAsync(140)).ReturnsAsync(package);
        userRepo.Setup(x => x.GetByIdAsync(25)).ReturnsAsync(user);
        transactionRepo
            .Setup(x => x.GetPendingByUserAndReferenceAsync(25, ReferenceType.Subscription.ToString(), 140))
            .ReturnsAsync((Transaction?)null);
        transactionRepo
            .Setup(x => x.AddAsync(It.IsAny<Transaction>()))
            .Callback<Transaction>(t => t.TransactionId = 900)
            .Returns(Task.CompletedTask);

        var result = await service.CheckoutSubscriptionAsync(25, 140);

        Assert.Equal("AISEP900", result.PaymentCode);
    }

    [Fact]
    public async Task UT047_CheckoutSubscriptionAsync_ShouldReturnQrCodeUrl_WhenCreated()
    {
        var package = BuildPackage(150, "Investor Diamond", UserRole.Investor, price: 250);
        var user = BuildUser(26, UserRole.Investor);

        var (service, _, packageRepo, transactionRepo, _, userRepo, _, _) = CreateSut();
        packageRepo.Setup(x => x.GetByIdAsync(150)).ReturnsAsync(package);
        userRepo.Setup(x => x.GetByIdAsync(26)).ReturnsAsync(user);
        transactionRepo
            .Setup(x => x.GetPendingByUserAndReferenceAsync(26, ReferenceType.Subscription.ToString(), 150))
            .ReturnsAsync((Transaction?)null);
        transactionRepo
            .Setup(x => x.AddAsync(It.IsAny<Transaction>()))
            .Callback<Transaction>(t => t.TransactionId = 901)
            .Returns(Task.CompletedTask);

        var result = await service.CheckoutSubscriptionAsync(26, 150);

        Assert.Contains("https://img.vietqr.io/image/MB-123456789-compact2.jpg", result.QrCodeUrl);
        Assert.Contains("amount=250", result.QrCodeUrl);
        Assert.Contains("addInfo=AISEP901", result.QrCodeUrl);
        Assert.Contains("accountName=AISEP%20ACCOUNT", result.QrCodeUrl);
    }

    [Fact]
    public async Task UT048_CheckoutBookingAsync_ShouldThrow_WhenBookingIdIsNotPositive()
    {
        var (service, _, _, _, _, _, _, _) = CreateSut();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CheckoutBookingAsync(30, 0));

        Assert.Contains("BookingId must be greater than 0.", ex.Message);
    }

    [Fact]
    public async Task UT049_CheckoutBookingAsync_ShouldThrow_WhenPayableBookingNotFound()
    {
        var (service, _, _, _, bookingRepo, _, _, _) = CreateSut();
        bookingRepo.Setup(x => x.GetPayableByIdAndCustomerAsync(501, 30)).ReturnsAsync((Booking?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CheckoutBookingAsync(30, 501));

        Assert.Contains("Booking not found or not in ApprovedAwaitingPayment status.", ex.Message);
    }

    [Fact]
    public async Task UT050_CheckoutBookingAsync_ShouldReusePendingTransaction_WhenPendingNotExpired()
    {
        var booking = new Booking
        {
            BookingId = 510,
            CustomerId = 31,
            Status = BookingStatus.ApprovedAwaitingPayment,
            Price = 400
        };
        var existingPending = BuildPendingTransaction(710, 31, ReferenceType.Booking, 510, 400, DateTime.UtcNow.AddMinutes(-2));
        existingPending.PaymentCode = "AISEP710";

        var (service, _, _, transactionRepo, bookingRepo, _, _, _) = CreateSut();
        bookingRepo.Setup(x => x.GetPayableByIdAndCustomerAsync(510, 31)).ReturnsAsync(booking);
        transactionRepo
            .Setup(x => x.GetPendingByUserAndReferenceAsync(31, ReferenceType.Booking.ToString(), 510))
            .ReturnsAsync(existingPending);

        var result = await service.CheckoutBookingAsync(31, 510);

        Assert.Equal(710, result.TransactionId);
        Assert.Equal("AISEP710", result.PaymentCode);
        transactionRepo.Verify(x => x.AddAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task UT051_CheckoutBookingAsync_ShouldCreatePendingTransaction_WhenNoPendingExists()
    {
        var booking = new Booking
        {
            BookingId = 511,
            CustomerId = 32,
            Status = BookingStatus.ApprovedAwaitingPayment,
            Price = 450
        };

        var (service, _, _, transactionRepo, bookingRepo, _, _, _) = CreateSut();
        bookingRepo.Setup(x => x.GetPayableByIdAndCustomerAsync(511, 32)).ReturnsAsync(booking);
        transactionRepo
            .Setup(x => x.GetPendingByUserAndReferenceAsync(32, ReferenceType.Booking.ToString(), 511))
            .ReturnsAsync((Transaction?)null);
        transactionRepo
            .Setup(x => x.AddAsync(It.IsAny<Transaction>()))
            .Callback<Transaction>(t => t.TransactionId = 1001)
            .Returns(Task.CompletedTask);

        var result = await service.CheckoutBookingAsync(32, 511);

        Assert.Equal("AISEP1001", result.PaymentCode);
        transactionRepo.Verify(x => x.AddAsync(It.IsAny<Transaction>()), Times.Once);
    }

    [Fact]
    public async Task UT052_UpdatePackageAsync_ShouldThrow_WhenPackageIdIsNotPositive()
    {
        var (service, _, _, _, _, _, _, _) = CreateSut();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdatePackageAsync(0, BuildValidUpdateRequest()));

        Assert.Contains("PackageId must be greater than 0.", ex.Message);
    }

    [Fact]
    public async Task UT053_UpdatePackageAsync_ShouldThrow_WhenPackageNotFound()
    {
        var (service, _, packageRepo, _, _, _, _, _) = CreateSut();
        packageRepo.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Package?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.UpdatePackageAsync(999, BuildValidUpdateRequest()));

        Assert.Contains("Package not found.", ex.Message);
    }

    [Fact]
    public async Task UT054_UpdatePackageAsync_ShouldThrow_WhenPackageTargetRoleNotSupported()
    {
        var unsupportedPackage = BuildPackage(201, "Advisor Plan", UserRole.Advisor, price: 100);

        var (service, _, packageRepo, _, _, _, _, _) = CreateSut();
        packageRepo.Setup(x => x.GetByIdAsync(201)).ReturnsAsync(unsupportedPackage);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdatePackageAsync(201, BuildValidUpdateRequest()));

        Assert.Contains("Only Investor and Startup packages can be updated", ex.Message);
    }

    [Fact]
    public async Task UT055_UpdatePackageAsync_ShouldThrow_WhenPriceIsNotPositive()
    {
        var package = BuildPackage(202, "Investor Plan", UserRole.Investor, price: 100);
        var request = BuildValidUpdateRequest();
        request.Price = 0;

        var (service, _, packageRepo, _, _, _, _, _) = CreateSut();
        packageRepo.Setup(x => x.GetByIdAsync(202)).ReturnsAsync(package);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdatePackageAsync(202, request));

        Assert.Contains("Price must be greater than 0.", ex.Message);
    }

    [Fact]
    public async Task UT056_UpdatePackageAsync_ShouldThrow_WhenDurationMonthsIsNotPositive()
    {
        var package = BuildPackage(203, "Startup Plan", UserRole.Startup, price: 100);
        var request = BuildValidUpdateRequest();
        request.DurationMonths = 0;

        var (service, _, packageRepo, _, _, _, _, _) = CreateSut();
        packageRepo.Setup(x => x.GetByIdAsync(203)).ReturnsAsync(package);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdatePackageAsync(203, request));

        Assert.Contains("DurationMonths must be greater than 0.", ex.Message);
    }

    [Fact]
    public async Task UT057_UpdatePackageAsync_ShouldThrow_WhenPackageNameIsEmpty()
    {
        var package = BuildPackage(204, "Investor Plan", UserRole.Investor, price: 100);
        var request = BuildValidUpdateRequest();
        request.PackageName = "   ";

        var (service, _, packageRepo, _, _, _, _, _) = CreateSut();
        packageRepo.Setup(x => x.GetByIdAsync(204)).ReturnsAsync(package);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdatePackageAsync(204, request));

        Assert.Contains("PackageName is required.", ex.Message);
    }

    [Fact]
    public async Task UT058_UpdatePackageAsync_ShouldPersistFields_WhenInputValid()
    {
        var package = BuildPackage(205, "Old Name", UserRole.Investor, price: 99);
        package.Description = "Old";
        package.DurationMonths = 1;
        package.MaxAiRequests = 10;
        package.MaxProjectViews = 15;
        package.FreeBookingCount = 1;

        var request = new UpdatePackageRequest
        {
            PackageName = "  New Investor Name  ",
            Description = "  Better package  ",
            Price = 299,
            DurationMonths = 6,
            MaxAiRequests = 120,
            MaxProjectViews = 80,
            FreeBookingCount = 3
        };

        var (service, unitOfWork, packageRepo, _, _, _, _, _) = CreateSut();
        packageRepo.Setup(x => x.GetByIdAsync(205)).ReturnsAsync(package);

        var result = await service.UpdatePackageAsync(205, request);

        Assert.Equal("New Investor Name", package.PackageName);
        Assert.Equal("Better package", package.Description);
        Assert.Equal(299, package.Price);
        Assert.Equal(6, package.DurationMonths);
        Assert.Equal(120, package.MaxAiRequests);
        Assert.Equal(80, package.MaxProjectViews);
        Assert.Equal(3, package.FreeBookingCount);

        Assert.Equal(205, result.PackageId);
        Assert.Equal("New Investor Name", result.PackageName);

        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    private static (
        PaymentService Service,
        Mock<IUnitOfWork> UnitOfWork,
        Mock<IPackageRepository> PackageRepository,
        Mock<ITransactionRepository> TransactionRepository,
        Mock<IBookingRepository> BookingRepository,
        Mock<IUserRepository> UserRepository,
        Mock<IMapper> Mapper,
        Mock<INotificationService> NotificationService) CreateSut()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var packageRepositoryMock = new Mock<IPackageRepository>();
        var transactionRepositoryMock = new Mock<ITransactionRepository>();
        var bookingRepositoryMock = new Mock<IBookingRepository>();
        var userRepositoryMock = new Mock<IUserRepository>();
        var mapperMock = new Mock<IMapper>();
        var sieveProcessorMock = new Mock<ISieveProcessor>();
        var notificationServiceMock = new Mock<INotificationService>();

        unitOfWorkMock.SetupGet(x => x.Packages).Returns(packageRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.Transactions).Returns(transactionRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.Bookings).Returns(bookingRepositoryMock.Object);
        unitOfWorkMock.SetupGet(x => x.Users).Returns(userRepositoryMock.Object);
        unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        mapperMock
            .Setup(x => x.Map<IEnumerable<PackageResponse>>(It.IsAny<IEnumerable<Package>>()))
            .Returns<IEnumerable<Package>>(packages => packages.Select(p => new PackageResponse
            {
                PackageId = p.PackageId,
                PackageName = p.PackageName,
                Description = p.Description,
                Price = p.Price,
                DurationMonths = p.DurationMonths,
                MaxAiRequests = p.MaxAiRequests,
                MaxProjectViews = p.MaxProjectViews,
                FreeBookingCount = p.FreeBookingCount
            }).ToList());

        mapperMock
            .Setup(x => x.Map<PackageResponse>(It.IsAny<Package>()))
            .Returns<Package>(p => new PackageResponse
            {
                PackageId = p.PackageId,
                PackageName = p.PackageName,
                Description = p.Description,
                Price = p.Price,
                DurationMonths = p.DurationMonths,
                MaxAiRequests = p.MaxAiRequests,
                MaxProjectViews = p.MaxProjectViews,
                FreeBookingCount = p.FreeBookingCount
            });

        mapperMock
            .Setup(x => x.Map<CheckoutResponse>(It.IsAny<Transaction>()))
            .Returns<Transaction>(t => new CheckoutResponse
            {
                TransactionId = t.TransactionId,
                Amount = t.Amount,
                PaymentCode = t.PaymentCode ?? string.Empty
            });

        var sePaySettings = Options.Create(new SePaySettings
        {
            PaymentPrefix = "AISEP",
            PendingTimeoutMinutes = 30,
            BankCode = "MB",
            AccountNumber = "123456789",
            AccountName = "AISEP ACCOUNT"
        });

        var service = new PaymentService(
            unitOfWorkMock.Object,
            sePaySettings,
            mapperMock.Object,
            sieveProcessorMock.Object,
            notificationServiceMock.Object);

        return (
            service,
            unitOfWorkMock,
            packageRepositoryMock,
            transactionRepositoryMock,
            bookingRepositoryMock,
            userRepositoryMock,
            mapperMock,
            notificationServiceMock);
    }

    private static Package BuildPackage(int id, string name, UserRole targetRole, decimal price = 100)
    {
        return new Package
        {
            PackageId = id,
            PackageName = name,
            Description = $"{name} description",
            TargetRole = targetRole,
            Price = price,
            DurationMonths = 1,
            MaxAiRequests = 10,
            MaxProjectViews = 10,
            FreeBookingCount = 0
        };
    }

    private static User BuildUser(int id, UserRole role)
    {
        return new User
        {
            Id = id,
            Email = $"user{id}@test.local",
            UserName = $"user{id}",
            Role = role,
            Status = UserStatus.Active,
            IsPremium = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static Transaction BuildPendingTransaction(
        int transactionId,
        int userId,
        ReferenceType referenceType,
        int referenceId,
        decimal amount,
        DateTime createdAt)
    {
        return new Transaction
        {
            TransactionId = transactionId,
            UserId = userId,
            Amount = amount,
            Type = TransactionType.Payment,
            Status = TransactionStatus.Pending,
            ReferenceType = referenceType.ToString(),
            ReferenceId = referenceId,
            CreatedAt = createdAt
        };
    }

    private static UpdatePackageRequest BuildValidUpdateRequest()
    {
        return new UpdatePackageRequest
        {
            PackageName = "Updated Package",
            Description = "Updated Description",
            Price = 199,
            DurationMonths = 3,
            MaxAiRequests = 50,
            MaxProjectViews = 20,
            FreeBookingCount = 2
        };
    }
}
