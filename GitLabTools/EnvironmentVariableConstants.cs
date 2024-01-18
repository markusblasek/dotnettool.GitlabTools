using System.Diagnostics.CodeAnalysis;

namespace GitLabTools;
[ExcludeFromCodeCoverage]
public static class EnvironmentVariableConstants
{
    public const string HttpProxy = "http_proxy";
    public const string HttpsProxy = "https_proxy";
    public const string NoProxy = "no_proxy";
    public static readonly char[] NoProxyDelimiters = [',', ';'];
}
