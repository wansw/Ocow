namespace Ocow.Shared.OpenApi;

/// <summary>
/// OpenAPI 分组名称常量，用于区分小程序端、后台端、内部服务和回调接口。/// </summary>
public static class OpenApiGroupNames
{
    /// <summary>
    /// 小程序或用户端接口分组。    
    ///  </summary>
    public const string Client = "Client";

    /// <summary>
    /// PC 后台接口分组。    
    ///  </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// 内部服务接口分组。   
    ///  </summary>
    public const string Internal = "Internal";

    /// <summary>
    /// 第三方回调接口分组。   
    ///  </summary>
    public const string Notify = "Notify";
}
