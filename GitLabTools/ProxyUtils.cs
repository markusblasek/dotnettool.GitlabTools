using System.Diagnostics.CodeAnalysis;
using Flurl;

namespace GitLabTools;
[ExcludeFromCodeCoverage]
public static class ProxyUtils
{
    public static string? ReadProxyFromEnvironmentVariables()
    {
        var proxyFromhttpProxyEnvVar = Environment.GetEnvironmentVariable(EnvironmentVariableConstants.HttpProxy);
        if (IsValidUrl(proxyFromhttpProxyEnvVar))
        {
            return proxyFromhttpProxyEnvVar;
        }
        var proxyFromhttpsProxyEnvVar = Environment.GetEnvironmentVariable(EnvironmentVariableConstants.HttpsProxy);
        return IsValidUrl(proxyFromhttpsProxyEnvVar) ? proxyFromhttpsProxyEnvVar : null;
    }

    public static string[] ReadNoProxyValueAsArrayFromEnvironmentVariables()
    {
        var noProxyValueFromEnvVar = Environment.GetEnvironmentVariable(EnvironmentVariableConstants.NoProxy);
        if (!string.IsNullOrWhiteSpace(noProxyValueFromEnvVar))
        {
            return noProxyValueFromEnvVar
                .Split(EnvironmentVariableConstants.NoProxyDelimiters, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.ToLowerInvariant().Trim())
                .Distinct()
                .ToArray();
        }

        return Array.Empty<string>();
    }

    private static bool IsValidUrl(string? urlToValidate)
    {
        return !string.IsNullOrWhiteSpace(urlToValidate) && Url.IsValid(urlToValidate);
    }
}
