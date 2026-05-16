using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Ocow.Shared.Dtos;

namespace Ocow.AspNetCore.Extensions;

/// <summary>
/// API 统一响应服务注册扩展。
/// </summary>
public static class ValidDtoServiceCollectionExtensions
{
    /// <summary>
    /// 注册统一 API 行为配置，用于把参数校验失败转换成标准接口响应。
    /// </summary>
    public static IServiceCollection AddOcowValidDtoResponse(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(item => item.Value?.Errors.Count > 0)
                    .ToDictionary(
                        item => item.Key,
                        item => item.Value!.Errors
                            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "参数格式不正确。" : error.ErrorMessage)
                            .ToArray());

                var response = new ApiResDto<IReadOnlyDictionary<string, string[]>>
                {
                    Success = false,
                    Code = "VALIDATION_FAILED",
                    Message = "参数校验失败。",
                    Data = errors,
                    TraceId = context.HttpContext.TraceIdentifier
                };

                return new BadRequestObjectResult(response);
            };
        });

        return services;
    }
}
