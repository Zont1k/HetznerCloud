using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HetznerCloud.Core;
using HetznerCloud.Observability;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace HetznerCloud.Resilience;

/// <summary>
/// Circuit breaker state for a specific operation
/// </summary>
public enum CircuitBreakerState
{
    Closed,
    Open,
    HalfOpen
}

/// <summary>
/// Configuration for circuit breaker
/// </summary>
public sealed record CircuitBreakerOptions
{
    /// <summary>
    /// Number of failures before opening the circuit
    /// </summary>
    public int FailureThreshold { get; init; } = 5;

    /// <summary>
    /// Time window for counting failures
    /// </summary>
    public TimeSpan FailureWindow { get; init; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Time to wait before transitioning from Open to HalfOpen
    /// </summary>
    public TimeSpan OpenTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Number of successful requests in HalfOpen before closing
    /// </summary>
    public int SuccessThreshold { get; init; } = 2;

    /// <summary>
    /// Exceptions that count as failures
    /// </summary>
    public IEnumerable<Type> HandledExceptions { get; init; } = new[]
    {
        typeof(HttpRequestException),
        typeof(TaskCanceledException),
        typeof(TimeoutException),
        typeof(HetznerCloud.Exceptions.ServerErrorException),
        typeof(HetznerCloud.Exceptions.ServiceUnavailableException)
    };
}

/// <summary>
/// Hedging options for parallel requests
/// </summary>
public sealed record HedgingOptions
{
    /// <summary>
    /// Maximum number of hedged attempts (including original)
    /// </summary>
    public int MaxAttempts { get; init; } = 3;

    /// <summary>
    /// Delay before starting a hedged request
    /// </summary>
    public TimeSpan HedgingDelay { get; init; } = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// Maximum total time for all hedged attempts
    /// </summary>
    public TimeSpan MaxTotalDuration { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Cancellation token for all hedged requests
    /// </summary>
    public CancellationToken CancellationToken { get; init; } = default;
}

/// <summary>
/// Per-operation resilience configuration
/// </summary>
public sealed class ResiliencePipeline
{
    private readonly AsyncCircuitBreakerPolicy _circuitBreaker;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly HedgingOptions _hedgingOptions;
    private readonly HetznerCloudTelemetry? _telemetry;

    public CircuitBreakerState State => _circuitBreaker.CircuitState switch
    {
        CircuitState.Closed => CircuitBreakerState.Closed,
        CircuitState.Open => CircuitBreakerState.Open,
        CircuitState.HalfOpen => CircuitBreakerState.HalfOpen,
        _ => CircuitBreakerState.Closed
    };

    public ResiliencePipeline(
        CircuitBreakerOptions circuitBreakerOptions,
        AsyncRetryPolicy retryPolicy,
        HedgingOptions hedgingOptions,
        HetznerCloudTelemetry? telemetry = null)
    {
        _retryPolicy = retryPolicy;
        _hedgingOptions = hedgingOptions;
        _telemetry = telemetry;

        _circuitBreaker = Policy
            .Handle<Exception>(ex => circuitBreakerOptions.HandledExceptions.Any(t => t.IsInstanceOfType(ex)))
            .CircuitBreakerAsync(
                circuitBreakerOptions.FailureThreshold,
                circuitBreakerOptions.FailureWindow,
                onBreak: (ex, duration) =>
                {
                    _telemetry?.GetOrCreateOperationCounter("circuit_breaker_open").Add(1);
                    _telemetry?.GetOrCreateOperationCounter("circuit_breaker_open_duration_ms").Add((long)duration.TotalMilliseconds);
                },
                onReset: () =>
                {
                    _telemetry?.GetOrCreateOperationCounter("circuit_breaker_reset").Add(1);
                },
                onHalfOpen: () =>
                {
                    _telemetry?.GetOrCreateOperationCounter("circuit_breaker_half_open").Add(1);
                });
    }

    public async Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        // Try hedging if enabled
        if (_hedgingOptions.MaxAttempts > 1)
        {
            var result = await ExecuteWithHedgingAsync(action, cancellationToken);
            if (result.IsSuccess)
                return result.Value!;
            throw new Exception(result.Error?.ToString() ?? "Hedging failed");
        }

        // Standard execution with circuit breaker and retry
        return await _circuitBreaker.ExecuteAsync(async ct =>
            await _retryPolicy.ExecuteAsync(async ct2 => await action(ct2), ct),
            cancellationToken);
    }

    private async Task<Result<TResult, Exception>> ExecuteWithHedgingAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var tasks = new List<Task<Result<TResult, Exception>>>();
        var started = 0;
        var completed = 0;

        var startTime = Stopwatch.GetTimestamp();

        try
        {
            // Start first attempt
            var firstTask = ExecuteSingleAttemptAsync(action, cts.Token);
            tasks.Add(firstTask);
            started = 1;

            while (completed < _hedgingOptions.MaxAttempts)
            {
                var delay = TimeSpan.FromMilliseconds(
                    _hedgingOptions.HedgingDelay.TotalMilliseconds * started);

                var nextStartTime = startTime + (long)(delay.TotalSeconds * Stopwatch.Frequency);
                
                // Check if we should start another hedged request
                if (started < _hedgingOptions.MaxAttempts && 
                    Stopwatch.GetTimestamp() >= nextStartTime &&
                    completed < started)
                {
                    var hedgedTask = ExecuteSingleAttemptAsync(action, cts.Token);
                    tasks.Add(hedgedTask);
                    started++;
                }

                // Wait for any task to complete
                var completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);
                completed++;

                var result = await completedTask;
                if (result.IsSuccess)
                {
                    cts.Cancel();
                    return result.Value!;
                }

                // Check timeout
                var elapsed = TimeSpan.FromSeconds(
                    (Stopwatch.GetTimestamp() - startTime) / (double)Stopwatch.Frequency);
                if (elapsed >= _hedgingOptions.MaxTotalDuration)
                {
                    cts.Cancel();
                    break;
                }

                // Small delay before checking next
                await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
            }

            // All attempts failed
            return tasks.Count > 0 
                ? (await Task.WhenAll(tasks)).First(r => r.IsFailure).Error!
                : throw new InvalidOperationException("No hedging attempts completed");
        }
        finally
        {
            cts.Dispose();
        }
    }

    private async Task<Result<TResult, Exception>> ExecuteSingleAttemptAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _circuitBreaker.ExecuteAsync(async ct =>
                await _retryPolicy.ExecuteAsync(async ct2 => await action(ct2), ct),
                cancellationToken);
            
            return Result<TResult, Exception>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<TResult, Exception>.Failure(ex);
        }
    }
}

/// <summary>
/// Registry for per-operation resilience pipelines
/// </summary>
public sealed class ResilienceRegistry
{
    private readonly ConcurrentDictionary<string, ResiliencePipeline> _pipelines = new();
    private readonly CircuitBreakerOptions _defaultCircuitBreakerOptions;
    private readonly HedgingOptions _defaultHedgingOptions;
    private readonly AsyncRetryPolicy _defaultRetryPolicy;
    private readonly HetznerCloudTelemetry? _telemetry;

    public ResilienceRegistry(
        CircuitBreakerOptions? defaultCircuitBreakerOptions = null,
        AsyncRetryPolicy? defaultRetryPolicy = null,
        HedgingOptions? defaultHedgingOptions = null,
        HetznerCloudTelemetry? telemetry = null)
    {
        _defaultCircuitBreakerOptions = defaultCircuitBreakerOptions ?? new CircuitBreakerOptions();
        _defaultRetryPolicy = defaultRetryPolicy ?? CreateDefaultRetryPolicy();
        _defaultHedgingOptions = defaultHedgingOptions ?? new HedgingOptions { MaxAttempts = 1 };
        _telemetry = telemetry;
    }

    public ResiliencePipeline GetOrCreate(string operationName)
    {
        return _pipelines.GetOrAdd(operationName, _ => new ResiliencePipeline(
            _defaultCircuitBreakerOptions,
            _defaultRetryPolicy,
            _defaultHedgingOptions,
            _telemetry));
    }

    public ResiliencePipeline GetOrCreate(string operationName, 
        CircuitBreakerOptions circuitBreakerOptions,
        HedgingOptions hedgingOptions)
    {
        return _pipelines.GetOrAdd(operationName, _ => new ResiliencePipeline(
            circuitBreakerOptions,
            _defaultRetryPolicy,
            hedgingOptions,
            _telemetry));
    }

    public bool TryGet(string operationName, out ResiliencePipeline? pipeline)
        => _pipelines.TryGetValue(operationName, out pipeline);

    public IReadOnlyDictionary<string, ResiliencePipeline> GetAll()
        => _pipelines;

    public void Reset(string operationName)
    {
        if (_pipelines.TryRemove(operationName, out var pipeline))
        {
            // Pipeline will be garbage collected
        }
    }

    public void ResetAll() => _pipelines.Clear();

    private static AsyncRetryPolicy CreateDefaultRetryPolicy()
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<TimeoutException>()
            .Or<HetznerCloud.Exceptions.ServerErrorException>()
            .Or<HetznerCloud.Exceptions.ServiceUnavailableException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    // Log retry
                });
    }
}

/// <summary>
/// Extension methods for using resilience pipelines
/// </summary>
public static class ResilienceExtensions
{
    public static async Task<TResult> WithResilience<TResult>(
        this ResilienceRegistry registry,
        string operationName,
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        var pipeline = registry.GetOrCreate(operationName);
        return await pipeline.ExecuteAsync(action, cancellationToken);
    }

    public static async Task<TResult> WithResilience<TResult>(
        this ResilienceRegistry registry,
        string operationName,
        Func<CancellationToken, Task<TResult>> action,
        CircuitBreakerOptions circuitBreakerOptions,
        HedgingOptions hedgingOptions,
        CancellationToken cancellationToken = default)
    {
        var pipeline = registry.GetOrCreate(operationName, circuitBreakerOptions, hedgingOptions);
        return await pipeline.ExecuteAsync(action, cancellationToken);
    }
}