using Polly;
using Polly.CircuitBreaker;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Resilience;

public static class RedisCircuitBreaker
{
    public static readonly AsyncCircuitBreakerPolicy CircuitBreaker =
        Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30)
            );
}