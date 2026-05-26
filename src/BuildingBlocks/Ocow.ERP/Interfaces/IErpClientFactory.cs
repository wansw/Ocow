namespace Ocow.ERP.Interfaces;

/// <summary>
/// ERP 客户端工厂接口，用于根据 ERP 编码创建对应客户端。
/// </summary>
public interface IErpClientFactory
{
    /// <summary>
    /// 创建指定 ERP 编码的订单客户端。
    /// </summary>
    IErpOrderClient Create(string erpCode);
}
