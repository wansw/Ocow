using Ocow.ERP.Interfaces;

namespace Ocow.ERP.Services;

/// <summary>
/// 默认 ERP 客户端工厂，用于分发已注册的 ERP 客户端。
/// </summary>
public class DefaultErpClientFactory : IErpClientFactory
{
    private readonly DemoErpOrderClient _demoErpOrderClient;

    /// <summary>
    /// 初始化默认 ERP 客户端工厂。
    /// </summary>
    public DefaultErpClientFactory(DemoErpOrderClient demoErpOrderClient)
    {
        _demoErpOrderClient = demoErpOrderClient;
    }

    /// <summary>
    /// 创建指定 ERP 编码的订单客户端。
    /// </summary>
    public IErpOrderClient Create(string erpCode)
    {
        if (string.Equals(erpCode, "demo", StringComparison.OrdinalIgnoreCase))
        {
            return _demoErpOrderClient;
        }

        throw new NotSupportedException($"暂不支持 ERP：{erpCode}");
    }
}
