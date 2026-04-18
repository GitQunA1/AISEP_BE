using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Notifications;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.DAL.Repositories.Notifications;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Sieve.Models;
using Sieve.Services;
using System.Linq.Expressions;
using Xunit;

namespace AISEP.BLL.Tests.Notifications;

public class NotificationServiceGroupedTests
{
    [Fact]
    public async Task UT214_SendNotificationAsync_ShouldPersistNotification_BeforeRealtimePublish()
    {
        Notification? added = null;
        var (service, unitOfWork, notifications, realtimePublisher, _, _) = CreateSut();

        var sequence = new MockSequence();
        notifications
            .InSequence(sequence)
            .Setup(x => x.AddAsync(It.IsAny<Notification>()))
            .Callback<Notification>(n =>
            {
                added = n;
                n.NotificationId = 2141;
            })
            .Returns(Task.CompletedTask);
        unitOfWork
            .InSequence(sequence)
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        realtimePublisher
            .InSequence(sequence)
            .Setup(x => x.PublishToUserAsync(1214, It.IsAny<NotificationDto>()))
            .Returns(Task.CompletedTask);

        await service.SendNotificationAsync(
            userId: 1214,
            title: "System Alert",
            message: "Important update",
            type: NotificationType.System,
            referenceId: 7001,
            referenceType: "Deal");

        Assert.NotNull(added);
        Assert.Equal(1214, added!.UserId);
        Assert.Equal("System", added.Type);
        Assert.False(added.IsRead);

        notifications.Verify(x => x.AddAsync(It.IsAny<Notification>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        realtimePublisher.Verify(x => x.PublishToUserAsync(1214, It.IsAny<NotificationDto>()), Times.Once);
    }

    [Fact]
    public async Task UT215_SendNotificationAsync_ShouldNotThrow_WhenRealtimePublishFails()
    {
        var (service, unitOfWork, notifications, realtimePublisher, _, _) = CreateSut();
        notifications.Setup(x => x.AddAsync(It.IsAny<Notification>())).Returns(Task.CompletedTask);
        realtimePublisher
            .Setup(x => x.PublishToUserAsync(1215, It.IsAny<NotificationDto>()))
            .ThrowsAsync(new InvalidOperationException("SignalR push failed"));

        var exception = await Record.ExceptionAsync(() =>
            service.SendNotificationAsync(
                userId: 1215,
                title: "Realtime",
                message: "Push fails",
                type: NotificationType.General));

        Assert.Null(exception);
        notifications.Verify(x => x.AddAsync(It.IsAny<Notification>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        realtimePublisher.Verify(x => x.PublishToUserAsync(1215, It.IsAny<NotificationDto>()), Times.Once);
    }

    [Fact]
    public async Task UT216_GetUserNotificationsAsync_ShouldApplyDefaultPagination_WhenModelIsEmpty()
    {
        const int userId = 1216;
        var source = Enumerable.Range(1, 25)
            .Select(i => BuildNotification(notificationId: i, userId: userId, isRead: i % 2 == 0, createdAt: DateTime.UtcNow.AddMinutes(-i)))
            .ToList();

        var (service, _, notifications, _, _, _) = CreateSut(source);
        notifications
            .Setup(x => x.GetByUserIdQuery(userId))
            .Returns(() => new TestAsyncEnumerable<Notification>(source.AsQueryable()));

        var result = await service.GetUserNotificationsAsync(userId, new SieveModel());

        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(10, result.Items.Count());
    }

    [Fact]
    public async Task UT217_GetUserNotificationsAsync_ShouldCapPageSizeTo100_WhenRequestedTooLarge()
    {
        const int userId = 1217;
        var source = Enumerable.Range(1, 150)
            .Select(i => BuildNotification(notificationId: i, userId: userId, isRead: false, createdAt: DateTime.UtcNow.AddMinutes(-i)))
            .ToList();

        var (service, _, notifications, _, _, _) = CreateSut(source);
        notifications
            .Setup(x => x.GetByUserIdQuery(userId))
            .Returns(() => new TestAsyncEnumerable<Notification>(source.AsQueryable()));

        var result = await service.GetUserNotificationsAsync(userId, new SieveModel { Page = 1, PageSize = 999 });

        Assert.Equal(1, result.Page);
        Assert.Equal(100, result.PageSize);
        Assert.Equal(150, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(100, result.Items.Count());
    }

    [Fact]
    public async Task UT218_MarkAsReadAsync_ShouldReturnFalse_WhenRepositoryReturnsFalse()
    {
        var (service, unitOfWork, notifications, _, _, _) = CreateSut();
        notifications.Setup(x => x.MarkAsReadAsync(2180, 1218)).ReturnsAsync(false);

        var result = await service.MarkAsReadAsync(2180, 1218);

        Assert.False(result);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UT219_MarkAsReadAsync_ShouldSaveChangesAndReturnTrue_WhenRepositoryReturnsTrue()
    {
        var (service, unitOfWork, notifications, _, _, _) = CreateSut();
        notifications.Setup(x => x.MarkAsReadAsync(2190, 1219)).ReturnsAsync(true);

        var result = await service.MarkAsReadAsync(2190, 1219);

        Assert.True(result);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UT220_MarkAllAsReadAsync_ShouldSaveChangesAndReturnTrue()
    {
        var (service, unitOfWork, notifications, _, _, _) = CreateSut();
        notifications.Setup(x => x.MarkAllAsReadAsync(1220)).ReturnsAsync(5);

        var result = await service.MarkAllAsReadAsync(1220);

        Assert.True(result);
        notifications.Verify(x => x.MarkAllAsReadAsync(1220), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UT221_DeleteNotificationAsync_ShouldFollowRepositoryResult_AndSaveOnSuccess()
    {
        var (service, unitOfWork, notifications, _, _, _) = CreateSut();
        notifications.Setup(x => x.DeleteAsync(2211, 1221)).ReturnsAsync(false);
        notifications.Setup(x => x.DeleteAsync(2212, 1221)).ReturnsAsync(true);

        var missingResult = await service.DeleteNotificationAsync(2211, 1221);
        var successResult = await service.DeleteNotificationAsync(2212, 1221);

        Assert.False(missingResult);
        Assert.True(successResult);
        unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    private static (
        NotificationService Service,
        Mock<IUnitOfWork> UnitOfWork,
        Mock<INotificationRepository> Notifications,
        Mock<INotificationRealtimePublisher> RealtimePublisher,
        IMapper Mapper,
        ISieveProcessor SieveProcessor) CreateSut(List<Notification>? queryItems = null)
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var notificationsMock = new Mock<INotificationRepository>();
        var realtimePublisherMock = new Mock<INotificationRealtimePublisher>();
        var loggerMock = new Mock<ILogger<NotificationService>>();

        queryItems ??= new List<Notification>
        {
            BuildNotification(notificationId: 1, userId: 999, isRead: false, createdAt: DateTime.UtcNow.AddMinutes(-1))
        };

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Notification, NotificationDto>();
        });
        var mapper = mapperConfig.CreateMapper();

        var sieveProcessor = new ApplicationSieveProcessor(Options.Create(new SieveOptions()));

        unitOfWorkMock.SetupGet(x => x.Notifications).Returns(notificationsMock.Object);
        unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        notificationsMock
            .Setup(x => x.AddAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);
        notificationsMock
            .Setup(x => x.GetByUserIdQuery(It.IsAny<int>()))
            .Returns((int _) => new TestAsyncEnumerable<Notification>(queryItems.AsQueryable()));
        notificationsMock
            .Setup(x => x.MarkAsReadAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);
        notificationsMock
            .Setup(x => x.MarkAllAsReadAsync(It.IsAny<int>()))
            .ReturnsAsync(0);
        notificationsMock
            .Setup(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        realtimePublisherMock
            .Setup(x => x.PublishToUserAsync(It.IsAny<int>(), It.IsAny<NotificationDto>()))
            .Returns(Task.CompletedTask);

        var service = new NotificationService(
            unitOfWorkMock.Object,
            mapper,
            realtimePublisherMock.Object,
            loggerMock.Object,
            sieveProcessor);

        return (
            service,
            unitOfWorkMock,
            notificationsMock,
            realtimePublisherMock,
            mapper,
            sieveProcessor);
    }

    private static Notification BuildNotification(int notificationId, int userId, bool isRead, DateTime createdAt)
    {
        return new Notification
        {
            NotificationId = notificationId,
            UserId = userId,
            ReferenceId = 5000 + notificationId,
            ReferenceType = "Booking",
            Title = $"Notification-{notificationId}",
            Message = $"Message-{notificationId}",
            Type = NotificationType.General.ToString(),
            IsRead = isRead,
            CreatedAt = createdAt
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
