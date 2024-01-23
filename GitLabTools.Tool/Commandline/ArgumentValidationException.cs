using System.Diagnostics.CodeAnalysis;

namespace GitLabTools.Commandline;

[ExcludeFromCodeCoverage]
public class ArgumentValidationException(string message) : Exception(message);
