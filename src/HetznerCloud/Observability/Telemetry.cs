using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using HetznerCloud.Core;
using HetznerCloud.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HetznerCloud.Observability;

/// <summary>
/// Options for configuring OpenTelemetry integration
/// </summary>
public sealed class HetznerCloudTelemetryOptions
{
    public string ServiceName { get; set; } = "HetznerCloud";
    public string ServiceVersion { get; set; } = "1.0.0";
    public bool EnableMetrics { get; set; } = true;
    public bool EnableTracing { get; set; } = true;
    public bool EnableLogging { get; set; } = true;
}

/// <summary>
/// OpenTelemetry integration for Hetzner Cloud client
/// </summary>
public sealed class HetznerCloudTelemetry : IDisposable
{
    private readonly Meter _meter;
    private readonly ActivitySource _activitySource;
    private readonly ILogger? _logger;
    private readonly Counter<long>? _requestsTotal;
    private readonly Counter<long>? _requestsFailed;
    private readonly Histogram<double>? _requestDuration;
    private readonly Histogram<long>? _requestSize;
    private readonly Histogram<long>? _responseSize;
    private readonly UpDownCounter<long>? _activeRequests;
    private readonly Gauge<long>? _lastErrorTimestamp;
    private readonly Dictionary<string, Counter<long>> _operationCounters = new();
    private readonly object _lock = new();

    public HetznerCloudTelemetry(
        IOptions<HetznerCloudTelemetryOptions> options,
        ILogger<HetznerCloudTelemetry>? logger = null)
    {
        var opts = options.Value;
        
        _meter = new Meter(opts.ServiceName, opts.ServiceVersion);
        _activitySource = new ActivitySource(opts.ServiceName);
        _logger = logger;

        if (opts.EnableMetrics)
        {
            _requestsTotal = _meter.CreateCounter<long>(
                "hetznercloud_requests_total",
                description: "Total number of API requests");

            _requestsFailed = _meter.CreateCounter<long>(
                "hetznercloud_requests_failed_total",
                description: "Total number of failed API requests");

            _requestDuration = _meter.CreateHistogram<double>(
                "hetznercloud_request_duration_seconds",
                unit: "s",
                description: "API request duration in seconds");

            _requestSize = _meter.CreateHistogram<long>(
                "hetznercloud_request_size_bytes",
                unit: "By",
                description: "API request size in bytes");

            _responseSize = _meter.CreateHistogram<long>(
                "hetznercloud_response_size_bytes",
                unit: "By",
                description: "API response size in bytes");

            _activeRequests = _meter.CreateUpDownCounter<long>(
                "hetznercloud_active_requests",
                description: "Number of currently active requests");

            _lastErrorTimestamp = _meter.CreateGauge<long>(
                "hetznercloud_last_error_timestamp_unix",
                description: "Unix timestamp of the last error");
        }
    }

    public Result<IDisposable, Exception> StartRequest(string operation, string method, string path)
    {
        var activity = _activitySource.StartActivity($"HetznerCloud.{operation}", ActivityKind.Client);
        activity?.SetTag("http.method", method);
        activity?.SetTag("http.route", path);
        activity?.SetTag("service.name", "HetznerCloud");

        return Result<IDisposable, Exception>.Success(new RequestScope(this, operation, _logger));
    }

    internal void RecordRequest(string operation, bool success, double durationSeconds, long? requestSize, long? responseSize, string? errorType = null)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("operation", operation),
            new("success", success)
        };

        _requestsTotal?.Add(1, tags);
        _requestDuration?.Record(durationSeconds, tags);

        if (success)
        {
            _requestsFailed?.Add(0);
        }
    }

    internal Counter<long> GetOrCreateOperationCounter(string operation)
    {
        lock (_lock)
        {
            if (!_operationCounters.TryGetValue(operation, out var counter))
            {
                counter = _meter.CreateCounter<long>(
                    $"hetznercloud_{operation}_total",
                    description: $"Total number of {operation} operations");
                _operationCounters[operation] = counter;
            }
            return counter;
        }
    }

    public void Dispose()
    {
        _meter.Dispose();
        _activitySource.Dispose();
    }
}

/// <summary>
/// Scope for tracking a single request
/// </summary>
internal sealed class RequestScope : IDisposable
{
    private readonly HetznerCloudTelemetry _telemetry;
    private readonly string _operation;
    private readonly ILogger? _logger;
    private readonly Stopwatch _stopwatch;
    private long? _requestSize;
    private long? _responseSize;
    private bool _disposed;

    public RequestScope(HetznerCloudTelemetry telemetry, string operation, ILogger? logger)
    {
        _telemetry = telemetry;
        _operation = operation;
        _logger = logger;
        _stopwatch = Stopwatch.StartNew();
    }

    public void SetRequestSize(long size) => _requestSize = size;
    public void SetResponseSize(long size) => _responseSize = size;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _stopwatch.Stop();
        var duration = _stopwatch.Elapsed.TotalSeconds;
    }
}

/// <summary>
/// Extension methods for easier telemetry usage
/// </summary>
public static class HetznerCloudTelemetryExtensions
{
    public static Result<IDisposable, Exception> StartRequest(this HetznerCloudTelemetry telemetry, string operation, string method, string path)
        => telemetry.StartRequest(operation, method, path);
}