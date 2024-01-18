using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Flurl.Http;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace GitLabTools;
[ExcludeFromCodeCoverage]
public static class PollyUtils
{
    private const int MaxNumberOfAttemptsMinValue = 0;
    private const int MaxNumberOfAttemptsDefaultValue = 2;

    public static readonly ImmutableHashSet<int> HttpStatusCodesWorthRetrying = [
        (int)HttpStatusCode.RequestTimeout,
        (int)HttpStatusCode.InternalServerError,
        (int)HttpStatusCode.BadGateway,
        (int)HttpStatusCode.ServiceUnavailable,
        (int)HttpStatusCode.GatewayTimeout];

    public static AsyncRetryPolicy GetAsyncRetryPolicyForExecuteHttpRequests(
        int maxNumberOfAttempts = MaxNumberOfAttemptsDefaultValue,
        ICollection<int>? httpStatusCodeWorthRetrying = null,
        Func<int, TimeSpan>? provideSleepDurationFunc = null)
    {
        if (maxNumberOfAttempts < MaxNumberOfAttemptsMinValue)
        {
            throw new ArgumentException(
                $"Value in parameter {nameof(maxNumberOfAttempts)} is smaller than {MaxNumberOfAttemptsMinValue}", nameof(maxNumberOfAttempts));
        }

        ICollection<int> httpStatusCodeWorthRetryingToUse = HttpStatusCodesWorthRetrying;
        if (httpStatusCodeWorthRetrying != null)
        {
            httpStatusCodeWorthRetryingToUse = httpStatusCodeWorthRetrying;
        }

        var provideSleepDurationFuncToUse = ProvideSleepDuration;
        if (provideSleepDurationFunc != null)
        {
            provideSleepDurationFuncToUse = provideSleepDurationFunc;
        }

        return Policy
            .Handle<FlurlHttpException>(ex => IsWorthRetrying(ex, httpStatusCodeWorthRetryingToUse))
            .WaitAndRetryAsync(maxNumberOfAttempts, provideSleepDurationFuncToUse);
    }

    // ReSharper disable once UnusedMember.Global
    public static AsyncCircuitBreakerPolicy GetAsyncCircuitBreakerPolicyForExecuteHttpRequests(
        int exceptionsAllowedBeforeBreaking,
        TimeSpan durationOfBreak,
        Action<Exception, TimeSpan, Context>? onBreak = null,
        Action<Context>? onReset = null,
        Action? onHalfOpen = null)
    {
        return GetAsyncCircuitBreakerPolicyForExecuteHttpRequests(HttpStatusCodesWorthRetrying,
            exceptionsAllowedBeforeBreaking, durationOfBreak, onBreak, onReset, onHalfOpen);
    }

    public static AsyncCircuitBreakerPolicy GetAsyncCircuitBreakerPolicyForExecuteHttpRequests(
        ICollection<int>? httpStatusCodeWorthRetrying,
        int exceptionsAllowedBeforeBreaking,
        TimeSpan durationOfBreak,
        Action<Exception, TimeSpan, Context>? onBreak = null,
        Action<Context>? onReset = null,
        Action? onHalfOpen = null)
    {
        ICollection<int> httpStatusCodeWorthRetryingToUse = HttpStatusCodesWorthRetrying;
        if (httpStatusCodeWorthRetrying != null)
        {
            httpStatusCodeWorthRetryingToUse = httpStatusCodeWorthRetrying;
        }

        var onBreakToUse = onBreak ?? ((_, _, _) => { });
        var onResetToUse = onReset ?? (_ => { });
        var onHalfOpenToUse = onHalfOpen ?? (() => { });

        return Policy
            .Handle<FlurlHttpException>(ex => IsWorthRetrying(ex, httpStatusCodeWorthRetryingToUse))
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking,
                durationOfBreak,
                onBreakToUse,
                onResetToUse,
                onHalfOpenToUse);
    }

    private static bool IsWorthRetrying(FlurlHttpException ex, IEnumerable<int> httpStatusCodesWorthRetrying)
    {
        if (ex is FlurlHttpTimeoutException)
        {
            return false;
        }

        var statusCode = ex.Call?.Response?.StatusCode ?? 0;
        return httpStatusCodesWorthRetrying.Contains(statusCode);
    }

    private static TimeSpan ProvideSleepDuration(int retryAttempt)
    {
        return retryAttempt == 1 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(500);
    }
}
