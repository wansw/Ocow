using Ocow.Contracts.Abstractions;
using Ocow.Contracts.Events.Orders;
using Ocow.EventBus.Abstractions;
using Ocow.EventBus.Abstractions.Services;

namespace Ocow.Tests.Unit
{
    /// <summary>
    /// EventBus 边界测试，用于验证集成事件契约、事件命名和 CAP 依赖隔离。
    /// </summary>
    public class EventBusBoundaryTests
    {
        /// <summary>
        /// 验证集成事件创建时会生成幂等编号和 UTC 发生时间。
        /// </summary>
        [Fact]
        public void IntegrationEvent_ShouldSetIdAndOccurredOnUtc()
        {
            var before = DateTime.UtcNow;

            var integrationEvent = new OrderCreatedIntegrationEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                25,
                "CNY");

            var after = DateTime.UtcNow;

            Assert.NotEqual(Guid.Empty, integrationEvent.Id);
            Assert.InRange(integrationEvent.OccurredOnUtc, before, after);
        }

        /// <summary>
        /// 验证事件名特性会拒绝空事件名。
        /// </summary>
        [Fact]
        public void IntegrationEventNameAttribute_WhenNameIsEmpty_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => new IntegrationEventNameAttribute(" "));
        }

        /// <summary>
        /// 验证事件名提供器会优先读取显式事件名特性。
        /// </summary>
        [Fact]
        public void DefaultIntegrationEventNameProvider_ShouldReadAttributeNameFirst()
        {
            var provider = new DefaultIntegrationEventNameProvider();

            var eventName = provider.GetName<OrderCreatedIntegrationEvent>();

            Assert.Equal("ocow.orders.created", eventName);
        }

        /// <summary>
        /// 验证未标注事件名时会按 IntegrationEvent 类型名推导 dot-case 事件名。
        /// </summary>
        [Fact]
        public void DefaultIntegrationEventNameProvider_WhenAttributeMissing_ShouldInferDotCaseName()
        {
            var provider = new DefaultIntegrationEventNameProvider();

            var eventName = provider.GetName(typeof(EventBusFallback.OrderCreatedIntegrationEvent));

            Assert.Equal("ocow.order.created", eventName);
        }

        /// <summary>
        /// 验证事件名提供器拒绝非 IntegrationEvent 类型。
        /// </summary>
        [Fact]
        public void DefaultIntegrationEventNameProvider_WhenTypeIsNotIntegrationEvent_ShouldThrow()
        {
            var provider = new DefaultIntegrationEventNameProvider();

            Assert.Throws<ArgumentException>(() => provider.GetName(typeof(string)));
        }

        /// <summary>
        /// 验证 Contracts 和 Abstractions 项目不会直接引用 CAP 或 RabbitMQ.Client。
        /// </summary>
        [Fact]
        public void ContractsAndAbstractions_ShouldNotReferenceCapOrRabbitMqClient()
        {
            var repositoryRoot = FindRepositoryRoot();
            var contractsProject = File.ReadAllText(Path.Combine(
                repositoryRoot,
                "src",
                "BuildingBlocks",
                "Ocow.Contracts",
                "Ocow.Contracts.csproj"));
            var abstractionsProject = File.ReadAllText(Path.Combine(
                repositoryRoot,
                "src",
                "BuildingBlocks",
                "Ocow.EventBus.Abstractions",
                "Ocow.EventBus.Abstractions.csproj"));

            Assert.DoesNotContain("DotNetCore.CAP", contractsProject);
            Assert.DoesNotContain("RabbitMQ.Client", contractsProject);
            Assert.DoesNotContain("DotNetCore.CAP", abstractionsProject);
            Assert.DoesNotContain("RabbitMQ.Client", abstractionsProject);
        }

        /// <summary>
        /// 从测试输出目录向上查找解决方案根目录。
        /// </summary>
        private static string FindRepositoryRoot()
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);

            while (current is not null)
            {
                if (File.Exists(Path.Combine(current.FullName, "Ocow.sln")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            throw new DirectoryNotFoundException("无法找到 Ocow.sln 所在目录。");
        }
    }
}

namespace Ocow.Tests.Unit.EventBusFallback
{
    /// <summary>
    /// 未显式标注事件名的测试事件，用于验证事件名 fallback 规则。
    /// </summary>
    internal sealed record OrderCreatedIntegrationEvent : IntegrationEvent;
}
