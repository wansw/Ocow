using System.Net;
using System.Net.Sockets;

namespace Ocow.Gateway.Middleware;

/// <summary>
/// 网关 IP 地址匹配器，用于支持精确 IP、通配符和 CIDR 规则。
/// </summary>
public static class GatewayIpAddressMatcher
{
    /// <summary>
    /// 判断 IP 地址是否命中任一规则。
    /// </summary>
    public static bool MatchesAny(IPAddress? address, IEnumerable<string> rules)
    {
        if (address is null)
        {
            return false;
        }

        return rules.Any(rule => Matches(address, rule));
    }

    /// <summary>
    /// 判断 IP 地址是否命中单条规则。
    /// </summary>
    public static bool Matches(IPAddress address, string rule)
    {
        if (string.IsNullOrWhiteSpace(rule))
        {
            return false;
        }

        var normalizedRule = rule.Trim();
        if (normalizedRule == "*")
        {
            return true;
        }

        return normalizedRule.Contains('/', StringComparison.Ordinal)
            ? MatchesCidr(address, normalizedRule)
            : MatchesExact(address, normalizedRule);
    }

    /// <summary>
    /// 判断 IP 地址是否和精确 IP 规则一致。
    /// </summary>
    private static bool MatchesExact(IPAddress address, string rule)
    {
        return IPAddress.TryParse(rule, out var ruleAddress) &&
               Normalize(address).Equals(Normalize(ruleAddress));
    }

    /// <summary>
    /// 判断 IP 地址是否落在 CIDR 网段内。
    /// </summary>
    private static bool MatchesCidr(IPAddress address, string rule)
    {
        var parts = rule.Split('/', StringSplitOptions.TrimEntries);
        if (parts.Length != 2 ||
            !IPAddress.TryParse(parts[0], out var networkAddress) ||
            !int.TryParse(parts[1], out var prefixLength))
        {
            return false;
        }

        var normalizedAddress = Normalize(address);
        var normalizedNetwork = Normalize(networkAddress);
        if (normalizedAddress.AddressFamily != normalizedNetwork.AddressFamily)
        {
            return false;
        }

        var addressBytes = normalizedAddress.GetAddressBytes();
        var networkBytes = normalizedNetwork.GetAddressBytes();
        var maxPrefixLength = addressBytes.Length * 8;
        if (prefixLength < 0 || prefixLength > maxPrefixLength)
        {
            return false;
        }

        var fullBytes = prefixLength / 8;
        var remainingBits = prefixLength % 8;
        for (var i = 0; i < fullBytes; i++)
        {
            if (addressBytes[i] != networkBytes[i])
            {
                return false;
            }
        }

        if (remainingBits == 0)
        {
            return true;
        }

        var mask = (byte)(byte.MaxValue << (8 - remainingBits));
        return (addressBytes[fullBytes] & mask) == (networkBytes[fullBytes] & mask);
    }

    /// <summary>
    /// 规范化 IPv4 映射地址，避免同一地址因表示方式不同而匹配失败。
    /// </summary>
    private static IPAddress Normalize(IPAddress address)
    {
        return address.IsIPv4MappedToIPv6 || address.AddressFamily == AddressFamily.InterNetworkV6 && address.ToString().StartsWith("::ffff:", StringComparison.OrdinalIgnoreCase)
            ? address.MapToIPv4()
            : address;
    }
}
