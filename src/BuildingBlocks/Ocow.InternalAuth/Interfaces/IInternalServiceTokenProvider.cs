namespace Ocow.InternalAuth.Interfaces;

/// <summary>
/// 内部服务 Token 签发接口，用于给服务间调用生成 JWT。
/// </summary>
public interface IInternalServiceTokenProvider
{
    /// <summary>
    /// 创建当前服务调用内部接口使用的 JWT。
    /// </summary>
    string CreateToken();
}
