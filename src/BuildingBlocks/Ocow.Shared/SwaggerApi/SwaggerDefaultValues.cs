using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ocow.Shared.SwaggerApi;

/// <summary>
/// Swagger 默认值过滤器，用于补齐接口描述中的默认响应信息。
///  </summary>
public class SwaggerDefaultValues : IOperationFilter
{
    /// <summary>
    /// 为接口补齐默认响应描述。    
    ///  </summary>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses.Count == 0)
        {
            operation.Responses.TryAdd("200", new OpenApiResponse { Description = "请求成功" });
        }
    }
}
