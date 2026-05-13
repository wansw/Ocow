using Serilog.Core;
using Serilog.Events;

namespace Ocow.Observability.Logging;

/// <summary>
/// 敏感日志属性脱敏器，用于避免密码、Token、密钥等字段以结构化属性形式写入日志。
/// </summary>
public class SensitiveLogEventEnricher : ILogEventEnricher
{
    private static readonly string[] SensitiveKeywords =
    [
        "password",
        "pwd",
        "token",
        "secret",
        "key",
        "authorization",
        "cookie"
    ];

    private static readonly ScalarValue MaskedValue = new("***");

    /// <summary>
    /// 对日志事件中的敏感属性执行脱敏。
    /// </summary>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (var property in logEvent.Properties.ToArray())
        {
            var sanitizedValue = IsSensitiveName(property.Key)
                ? MaskedValue
                : SanitizeValue(property.Value);

            if (!ReferenceEquals(sanitizedValue, property.Value))
            {
                logEvent.AddOrUpdateProperty(new LogEventProperty(property.Key, sanitizedValue));
            }
        }
    }

    /// <summary>
    /// 按属性名判断是否属于敏感信息。
    /// </summary>
    private static bool IsSensitiveName(string name)
    {
        return SensitiveKeywords.Any(keyword => name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 递归脱敏结构化日志属性值。
    /// </summary>
    private static LogEventPropertyValue SanitizeValue(LogEventPropertyValue value)
    {
        return value switch
        {
            StructureValue structureValue => new StructureValue(
                structureValue.Properties
                    .Select(property => new LogEventProperty(
                        property.Name,
                        IsSensitiveName(property.Name) ? MaskedValue : SanitizeValue(property.Value)))
                    .ToList(),
                structureValue.TypeTag),
            DictionaryValue dictionaryValue => new DictionaryValue(
                dictionaryValue.Elements.Select(element => new KeyValuePair<ScalarValue, LogEventPropertyValue>(
                    element.Key,
                    IsSensitiveDictionaryKey(element.Key) ? MaskedValue : SanitizeValue(element.Value)))),
            SequenceValue sequenceValue => new SequenceValue(sequenceValue.Elements.Select(SanitizeValue)),
            _ => value
        };
    }

    /// <summary>
    /// 按字典键判断是否属于敏感信息。
    /// </summary>
    private static bool IsSensitiveDictionaryKey(ScalarValue key)
    {
        return key.Value is string keyName && IsSensitiveName(keyName);
    }
}
