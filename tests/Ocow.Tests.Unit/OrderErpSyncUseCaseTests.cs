using Microsoft.EntityFrameworkCore;
using Ocow.EntityFrameworkCore.Abstractions;
using Ocow.ERP.Dtos;
using Ocow.ERP.Interfaces;
using Ocow.ERP.Options;
using Ocow.Order.Application.Dtos;
using Ocow.Order.Application.Interfaces;
using Ocow.Order.Application.Services;
using Ocow.Order.Infrastructure.Data;
using Ocow.Order.Infrastructure.Repositories;
using OrderEntity = Ocow.Order.Domain.Models.Order;

namespace Ocow.Tests.Unit;

/// <summary>
/// 订单 ERP 同步用例测试，用于验证 Order 服务通过 ERP 抽象拉单并幂等入库。
/// </summary>
public class OrderErpSyncUseCaseTests
{
    /// <summary>
    /// 验证首次同步 ERP 订单时会写入订单表并记录外部幂等键。
    /// </summary>
    [Fact]
    public async Task SyncErpOrdersAsync_WhenExternalOrderIsNew_ShouldInsertOrder()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var reqDto = CreateReqDto();

        var result = await service.SyncErpOrdersAsync(reqDto, CancellationToken.None);

        var order = await dbContext.Orders.Include(x => x.Items).SingleAsync();
        Assert.Equal(1, result.SyncedCount);
        Assert.Equal("demo", order.SourceSystem);
        Assert.Equal("ERP-10001", order.ExternalOrderId);
        Assert.Single(order.Items);
    }

    /// <summary>
    /// 验证重复同步同一个 ERP 订单时不会重复写入订单表。
    /// </summary>
    [Fact]
    public async Task SyncErpOrdersAsync_WhenExternalOrderAlreadyExists_ShouldSkipDuplicate()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var reqDto = CreateReqDto();

        await service.SyncErpOrdersAsync(reqDto, CancellationToken.None);
        var secondResult = await service.SyncErpOrdersAsync(reqDto, CancellationToken.None);

        Assert.Equal(0, secondResult.SyncedCount);
        Assert.Equal(1, await dbContext.Orders.CountAsync());
    }

    private static SyncErpOrdersReqDto CreateReqDto()
    {
        return new SyncErpOrdersReqDto
        {
            SyncConfigId = "sync-001",
            ErpCode = "demo",
            FromTime = new DateTimeOffset(2026, 5, 17, 10, 0, 0, TimeSpan.Zero),
            ToTime = new DateTimeOffset(2026, 5, 17, 10, 5, 0, TimeSpan.Zero)
        };
    }

    private static OrderAppService CreateService(OrderDbContext dbContext)
    {
        return new OrderAppService(
            new OrderRepository(dbContext),
            new FakeOrderCreationTransaction(),
            new FakeOrderCancellationTransaction(),
            new DbContextUnitOfWork(dbContext),
            new FakeErpClientFactory());
    }

    private static OrderDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new OrderDbContext(options);
    }

    private sealed class FakeErpClientFactory : IErpClientFactory
    {
        /// <summary>
        /// 创建测试 ERP 订单客户端。
        /// </summary>
        public IErpOrderClient Create(string erpCode)
        {
            return new FakeErpOrderClient();
        }
    }

    private sealed class FakeErpOrderClient : IErpOrderClient
    {
        /// <summary>
        /// 返回固定外部订单，便于验证幂等写入。
        /// </summary>
        public Task<IReadOnlyList<ExternalErpOrderResDto>> GetOrdersAsync(
            ErpConnectionOption option,
            DateTimeOffset fromTime,
            DateTimeOffset toTime,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ExternalErpOrderResDto> orders = new[]
            {
                new ExternalErpOrderResDto
                {
                    ExternalOrderId = "ERP-10001",
                    CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    CreatedAt = fromTime,
                    Items =
                    [
                        new ExternalErpOrderItemResDto
                        {
                            ProductId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                            SkuId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                            ProductName = "ERP 测试商品",
                            Quantity = 2,
                            UnitPrice = 10
                        }
                    ]
                }
            };

            return Task.FromResult(orders);
        }
    }

    private sealed class FakeOrderCreationTransaction : IOrderCreationTransaction
    {
        /// <summary>
        /// 测试同步用例不经过订单创建事务边界。
        /// </summary>
        public Task<OrderEntity> CreateAsync(OrderEntity order, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeOrderCancellationTransaction : IOrderCancellationTransaction
    {
        /// <summary>
        /// 测试同步用例不经过订单取消事务边界。
        /// </summary>
        public Task<OrderEntity> CancelAsync(Guid orderId, string reason, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class DbContextUnitOfWork : IUnitOfWork
    {
        private readonly OrderDbContext _dbContext;

        /// <summary>
        /// 创建测试用 EF Core 工作单元。
        /// </summary>
        public DbContextUnitOfWork(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 保存测试数据库上下文变更。
        /// </summary>
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 直接执行测试事务委托。
        /// </summary>
        public Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            return action(cancellationToken);
        }

        /// <summary>
        /// 直接执行测试事务委托并返回结果。
        /// </summary>
        public Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default)
        {
            return action(cancellationToken);
        }
    }
}
