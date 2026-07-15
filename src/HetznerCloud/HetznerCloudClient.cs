using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using HetznerCloud.Authentication;
using HetznerCloud.Clients;
using HetznerCloud.Exceptions;
using HetznerCloud.Models;
using HetznerCloud.Pagination;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace HetznerCloud;

public class HetznerCloudClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly HetznerCloudClientOptions _options;
    private readonly ILogger<HetznerCloudClient>? _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly JsonSerializerOptions _jsonOptions;

    public IServerClient Servers { get; }
    public IServerTypeClient ServerTypes { get; }
    public IImageClient Images { get; }
    public ILocationClient Locations { get; }
    public IDatacenterClient Datacenters { get; }
    public IVolumeClient Volumes { get; }
    public ILoadBalancerClient LoadBalancers { get; }
    public IFloatingIpClient FloatingIps { get; }
    public INetworkClient Networks { get; }
    public IActionClient Actions { get; }
    public ISSHKeyClient SSHKeys { get; }
    public ICertificateClient Certificates { get; }
    public IPlacementGroupClient PlacementGroups { get; }
    public IFirewallClient Firewalls { get; }
    public IIsoImageClient IsoImages { get; }
    public IPricingClient Pricing { get; }
    public IPrimaryIpClient PrimaryIps { get; }
    public IDnsZoneClient DnsZones { get; }
    public IStorageBoxClient StorageBoxes { get; }

    public HetznerCloudClient(
        HttpClient httpClient,
        IOptions<HetznerCloudClientOptions> options,
        ILogger<HetznerCloudClient>? logger = null)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _options.Validate();

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = _options.Timeout;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        _retryPolicy = CreateRetryPolicy();

        Servers = new ServerClient(this);
        ServerTypes = new ServerTypeClient(this);
        Images = new ImageClient(this);
        Locations = new LocationClient(this);
        Datacenters = new DatacenterClient(this);
        Volumes = new VolumeClient(this);
        LoadBalancers = new LoadBalancerClient(this);
        FloatingIps = new FloatingIpClient(this);
        Networks = new NetworkClient(this);
        Actions = new ActionClient(this);
        SSHKeys = new SshKeyClient(this);
        Certificates = new CertificateClient(this);
        PlacementGroups = new PlacementGroupClient(this);
        Firewalls = new FirewallClient(this);
        IsoImages = new IsoImageClient(this);
        Pricing = new PricingClient(this);
        PrimaryIps = new PrimaryIpClient(this);
        DnsZones = new DnsZoneClient(this);
        StorageBoxes = new StorageBoxClient(this);
    }

    private AsyncRetryPolicy<HttpResponseMessage> CreateRetryPolicy()
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => IsRetryableStatusCode(r.StatusCode))
            .WaitAndRetryAsync(
                _options.MaxRetries,
                retryAttempt => _options.RetryDelay * retryAttempt,
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger?.LogWarning("Retry {RetryCount} after {Delay}ms due to: {Reason}",
                        retryCount, timespan.TotalMilliseconds, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                });
    }

    private static bool IsRetryableStatusCode(System.Net.HttpStatusCode statusCode)
    {
        return statusCode == System.Net.HttpStatusCode.TooManyRequests ||
               statusCode == System.Net.HttpStatusCode.InternalServerError ||
               statusCode == System.Net.HttpStatusCode.BadGateway ||
               statusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
               statusCode == System.Net.HttpStatusCode.GatewayTimeout;
    }

    internal async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        var response = await ExecuteAsync(() => _httpClient.GetAsync(path, cancellationToken), cancellationToken);
        return await HandleResponse<T>(response, cancellationToken);
    }

    internal async Task<T> PostAsync<T, TRequest>(string path, TRequest request, CancellationToken cancellationToken = default)
    {
        var response = await ExecuteAsync(() => _httpClient.PostAsJsonAsync(path, request, _jsonOptions, cancellationToken), cancellationToken);
        return await HandleResponse<T>(response, cancellationToken);
    }

    internal async Task<T> PutAsync<T, TRequest>(string path, TRequest request, CancellationToken cancellationToken = default)
    {
        var response = await ExecuteAsync(() => _httpClient.PutAsJsonAsync(path, request, _jsonOptions, cancellationToken), cancellationToken);
        return await HandleResponse<T>(response, cancellationToken);
    }

    internal async Task<T> DeleteAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        var response = await ExecuteAsync(() => _httpClient.DeleteAsync(path, cancellationToken), cancellationToken);
        return await HandleResponse<T>(response, cancellationToken);
    }

    internal async Task<HttpResponseMessage> ExecuteAsync(Func<Task<HttpResponseMessage>> action, CancellationToken cancellationToken)
    {
        return await _retryPolicy.ExecuteAsync(async ct =>
        {
            var response = await action();
            
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(60);
                throw new RateLimitExceededException("Rate limit exceeded", retryAfter);
            }

            return response;
        }, cancellationToken);
    }

    internal async Task<T> HandleResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            if (typeof(T) == typeof(HttpResponseMessage))
            {
                return (T)(object)response;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrEmpty(content) && typeof(T) != typeof(string))
            {
                return default!;
            }

            return JsonSerializer.Deserialize<T>(content, _jsonOptions)!;
        }

        await HandleErrorResponse(response, cancellationToken);
        return default!;
    }

    private async Task HandleErrorResponse(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        ErrorResponse? errorResponse = null;

        try
        {
            errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);
        }
        catch
        {
        }

        var message = errorResponse?.Error?.Message ?? content ?? response.ReasonPhrase ?? "Unknown error";
        var details = errorResponse?.Details ?? new List<ErrorDetail>();

        switch (response.StatusCode)
        {
            case System.Net.HttpStatusCode.NotFound:
                throw new NotFoundException(message, details);
            case System.Net.HttpStatusCode.Unauthorized:
                throw new UnauthorizedException(message, details);
            case System.Net.HttpStatusCode.Forbidden:
                throw new ForbiddenException(message, details);
            case System.Net.HttpStatusCode.UnprocessableEntity:
                throw new ValidationException(message, details);
            case System.Net.HttpStatusCode.Conflict:
                throw new ConflictException(message, details);
            case System.Net.HttpStatusCode.TooManyRequests:
                var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(60);
                throw new RateLimitExceededException(message, retryAfter, details);
            case System.Net.HttpStatusCode.InternalServerError:
                throw new ServerErrorException(message, details);
            case System.Net.HttpStatusCode.ServiceUnavailable:
                var retryAfter503 = response.Headers.RetryAfter?.Delta;
                throw new ServiceUnavailableException(message, retryAfter503, details);
            default:
                if (_options.ThrowOnError)
                {
                    throw new HetznerCloudException(message, response.StatusCode, "", details);
                }
                break;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

public interface IServerClient
{
    Task<ServerListResponse> GetAllAsync(ServerListOptions? options = null, CancellationToken cancellationToken = default);
    Task<ServerResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ServerResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<ServerCreateResponse> CreateAsync(ServerCreateRequest request, CancellationToken cancellationToken = default);
    Task<ServerResponse> UpdateAsync(long id, ServerUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> PowerOnAsync(long id, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> PowerOffAsync(long id, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> RebootAsync(long id, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> ResetAsync(long id, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> RebuildAsync(long id, ServerRebuildRequest request, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> ChangeTypeAsync(long id, ServerChangeTypeRequest request, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> ChangeProtectionAsync(long id, ServerChangeProtectionRequest request, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> EnableBackupAsync(long id, ServerEnableBackupRequest request, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> DisableBackupAsync(long id, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> ChangeDnsPtrAsync(long id, ServerChangeDnsPtrRequest request, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> AttachToNetworkAsync(long id, ServerAttachToNetworkRequest request, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> DetachFromNetworkAsync(long id, ServerDetachFromNetworkRequest request, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> CreateImageAsync(long id, ServerCreateImageRequest request, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> ReassignIpAsync(long id, ServerReassignIpRequest request, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> ResetPasswordAsync(long id, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> EnableRescueAsync(long id, ServerEnableRescueRequest request, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> DisableRescueAsync(long id, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> ChangeAliasIpsAsync(long id, ServerChangeAliasIpsRequest request, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> AttachIsoAsync(long id, ServerAttachIsoRequest request, CancellationToken cancellationToken = default);
    Task<ServerActionResponse> DetachIsoAsync(long id, CancellationToken cancellationToken = default);
    Task<ActionResponse> WaitForActionAsync(long actionId, CancellationToken cancellationToken = default);
    Task<List<Models.Action>> WaitForActionsAsync(IEnumerable<long> actionIds, CancellationToken cancellationToken = default);
}

public interface IServerTypeClient
{
    Task<ServerTypeListResponse> GetAllAsync(ServerTypeListOptions? options = null, CancellationToken cancellationToken = default);
    Task<ServerTypeResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ServerTypeResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}

public interface IImageClient
{
    Task<ImageListResponse> GetAllAsync(ImageListOptions? options = null, CancellationToken cancellationToken = default);
    Task<ImageResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ImageResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<ImageResponse> UpdateAsync(long id, ImageUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<ImageActionResponse> ChangeProtectionAsync(long id, ImageChangeProtectionRequest request, CancellationToken cancellationToken = default);
}

public interface ILocationClient
{
    Task<LocationListResponse> GetAllAsync(LocationListOptions? options = null, CancellationToken cancellationToken = default);
    Task<LocationResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<LocationResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}

public interface IDatacenterClient
{
    Task<DatacenterListResponse> GetAllAsync(DatacenterListOptions? options = null, CancellationToken cancellationToken = default);
    Task<DatacenterResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<DatacenterResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}

public interface IVolumeClient
{
    Task<VolumeListResponse> GetAllAsync(VolumeListOptions? options = null, CancellationToken cancellationToken = default);
    Task<VolumeResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<VolumeResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<VolumeResponse> CreateAsync(VolumeCreateRequest request, CancellationToken cancellationToken = default);
    Task<VolumeResponse> UpdateAsync(long id, VolumeUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<VolumeActionResponse> AttachAsync(long id, VolumeAttachRequest request, CancellationToken cancellationToken = default);
    Task<VolumeActionResponse> DetachAsync(long id, CancellationToken cancellationToken = default);
    Task<VolumeActionResponse> ResizeAsync(long id, VolumeResizeRequest request, CancellationToken cancellationToken = default);
    Task<VolumeActionResponse> ChangeProtectionAsync(long id, VolumeChangeProtectionRequest request, CancellationToken cancellationToken = default);
}

public interface ILoadBalancerClient
{
    Task<LoadBalancerListResponse> GetAllAsync(LoadBalancerListOptions? options = null, CancellationToken cancellationToken = default);
    Task<LoadBalancerResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<LoadBalancerResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<LoadBalancerResponse> CreateAsync(LoadBalancerCreateRequest request, CancellationToken cancellationToken = default);
    Task<LoadBalancerResponse> UpdateAsync(long id, LoadBalancerUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<LoadBalancerTypeListResponse> GetTypesAsync(LoadBalancerTypeListOptions? options = null, CancellationToken cancellationToken = default);
    Task<LoadBalancerTypeResponse> GetTypeByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<LoadBalancerTypeResponse> GetTypeByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<LoadBalancerActionResponse> AddServiceAsync(long id, LoadBalancerAddServiceRequest request, CancellationToken cancellationToken = default);
    Task<LoadBalancerActionResponse> UpdateServiceAsync(long id, LoadBalancerUpdateServiceRequest request, CancellationToken cancellationToken = default);
    Task<LoadBalancerActionResponse> DeleteServiceAsync(long id, LoadBalancerDeleteServiceRequest request, CancellationToken cancellationToken = default);
    Task<LoadBalancerActionResponse> AddTargetAsync(long id, LoadBalancerAddTargetRequest request, CancellationToken cancellationToken = default);
    Task<LoadBalancerActionResponse> RemoveTargetAsync(long id, LoadBalancerRemoveTargetRequest request, CancellationToken cancellationToken = default);
    Task<LoadBalancerActionResponse> ChangeAlgorithmAsync(long id, LoadBalancerChangeAlgorithmRequest request, CancellationToken cancellationToken = default);
    Task<LoadBalancerActionResponse> ChangeTypeAsync(long id, LoadBalancerChangeTypeRequest request, CancellationToken cancellationToken = default);
    Task<LoadBalancerActionResponse> EnablePublicInterfaceAsync(long id, CancellationToken cancellationToken = default);
    Task<LoadBalancerActionResponse> DisablePublicInterfaceAsync(long id, CancellationToken cancellationToken = default);
    Task<LoadBalancerActionResponse> AttachToNetworkAsync(long id, LoadBalancerAttachToNetworkRequest request, CancellationToken cancellationToken = default);
    Task<LoadBalancerActionResponse> DetachFromNetworkAsync(long id, LoadBalancerDetachFromNetworkRequest request, CancellationToken cancellationToken = default);
    Task<LoadBalancerActionResponse> ChangeProtectionAsync(long id, LoadBalancerChangeProtectionRequest request, CancellationToken cancellationToken = default);
    Task<LoadBalancerActionResponse> ChangeIpAsync(long id, LoadBalancerChangeIpRequest request, CancellationToken cancellationToken = default);
}

public interface IFloatingIpClient
{
    Task<FloatingIpListResponse> GetAllAsync(FloatingIpListOptions? options = null, CancellationToken cancellationToken = default);
    Task<FloatingIpResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<FloatingIpResponse> CreateAsync(FloatingIpCreateRequest request, CancellationToken cancellationToken = default);
    Task<FloatingIpResponse> UpdateAsync(long id, FloatingIpUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<FloatingIpActionResponse> AssignAsync(long id, FloatingIpAssignRequest request, CancellationToken cancellationToken = default);
    Task<FloatingIpActionResponse> UnassignAsync(long id, CancellationToken cancellationToken = default);
    Task<FloatingIpActionResponse> ChangeDnsPtrAsync(long id, FloatingIpChangeDnsPtrRequest request, CancellationToken cancellationToken = default);
    Task<FloatingIpActionResponse> ChangeProtectionAsync(long id, FloatingIpChangeProtectionRequest request, CancellationToken cancellationToken = default);
}

public interface INetworkClient
{
    Task<NetworkListResponse> GetAllAsync(NetworkListOptions? options = null, CancellationToken cancellationToken = default);
    Task<NetworkResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<NetworkResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<NetworkResponse> CreateAsync(NetworkCreateRequest request, CancellationToken cancellationToken = default);
    Task<NetworkResponse> UpdateAsync(long id, NetworkUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<NetworkActionResponse> AddSubnetAsync(long id, NetworkAddSubnetRequest request, CancellationToken cancellationToken = default);
    Task<NetworkActionResponse> DeleteSubnetAsync(long id, NetworkDeleteSubnetRequest request, CancellationToken cancellationToken = default);
    Task<NetworkActionResponse> ChangeIpRangeAsync(long id, NetworkChangeIpRangeRequest request, CancellationToken cancellationToken = default);
    Task<NetworkActionResponse> AddRouteAsync(long id, NetworkAddRouteRequest request, CancellationToken cancellationToken = default);
    Task<NetworkActionResponse> DeleteRouteAsync(long id, NetworkDeleteRouteRequest request, CancellationToken cancellationToken = default);
    Task<NetworkActionResponse> ChangeProtectionAsync(long id, NetworkChangeProtectionRequest request, CancellationToken cancellationToken = default);
}

public interface IActionClient
{
    Task<ActionListResponse> GetAllAsync(ActionListOptions? options = null, CancellationToken cancellationToken = default);
    Task<ActionResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ActionResponse> WaitForAsync(long actionId, CancellationToken cancellationToken = default);
    Task<List<Models.Action>> WaitForAllAsync(IEnumerable<long> actionIds, CancellationToken cancellationToken = default);
}

public interface ISSHKeyClient
{
    Task<SshKeyListResponse> GetAllAsync(SshKeyListOptions? options = null, CancellationToken cancellationToken = default);
    Task<SshKeyResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<SshKeyResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<SshKeyResponse> CreateAsync(SshKeyCreateRequest request, CancellationToken cancellationToken = default);
    Task<SshKeyResponse> UpdateAsync(long id, SshKeyUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}

public interface ICertificateClient
{
    Task<CertificateListResponse> GetAllAsync(CertificateListOptions? options = null, CancellationToken cancellationToken = default);
    Task<CertificateResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<CertificateResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<CertificateResponse> CreateAsync(CertificateCreateRequest request, CancellationToken cancellationToken = default);
    Task<CertificateResponse> UpdateAsync(long id, CertificateUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}

public interface IPlacementGroupClient
{
    Task<PlacementGroupListResponse> GetAllAsync(PlacementGroupListOptions? options = null, CancellationToken cancellationToken = default);
    Task<PlacementGroupResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<PlacementGroupResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<PlacementGroupResponse> CreateAsync(PlacementGroupCreateRequest request, CancellationToken cancellationToken = default);
    Task<PlacementGroupResponse> UpdateAsync(long id, PlacementGroupUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}

public interface IFirewallClient
{
    Task<FirewallListResponse> GetAllAsync(FirewallListOptions? options = null, CancellationToken cancellationToken = default);
    Task<FirewallResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<FirewallResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<FirewallResponse> CreateAsync(FirewallCreateRequest request, CancellationToken cancellationToken = default);
    Task<FirewallResponse> UpdateAsync(long id, FirewallUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<FirewallActionResponse> SetRulesAsync(long id, FirewallSetRulesRequest request, CancellationToken cancellationToken = default);
    Task<FirewallActionResponse> ApplyToResourcesAsync(long id, FirewallApplyToResourcesRequest request, CancellationToken cancellationToken = default);
    Task<FirewallActionResponse> RemoveFromResourcesAsync(long id, FirewallRemoveFromResourcesRequest request, CancellationToken cancellationToken = default);
    Task<FirewallActionResponse> ChangeProtectionAsync(long id, FirewallChangeProtectionRequest request, CancellationToken cancellationToken = default);
}

public interface IIsoImageClient
{
    Task<IsoImageListResponse> GetAllAsync(IsoImageListOptions? options = null, CancellationToken cancellationToken = default);
    Task<IsoImageResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default);
}

public interface IPricingClient
{
    Task<PricingListResponse> GetAsync(CancellationToken cancellationToken = default);
}

public interface IPrimaryIpClient
{
    Task<PrimaryIpListResponse> GetAllAsync(PrimaryIpListOptions? options = null, CancellationToken cancellationToken = default);
    Task<PrimaryIpResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<PrimaryIpResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<PrimaryIpResponse> CreateAsync(PrimaryIpCreateRequest request, CancellationToken cancellationToken = default);
    Task<PrimaryIpResponse> UpdateAsync(long id, PrimaryIpUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<PrimaryIpActionResponse> AssignAsync(long id, PrimaryIpAssignRequest request, CancellationToken cancellationToken = default);
    Task<PrimaryIpActionResponse> UnassignAsync(long id, CancellationToken cancellationToken = default);
    Task<PrimaryIpActionResponse> ChangeDnsPtrAsync(long id, PrimaryIpChangeDnsPtrRequest request, CancellationToken cancellationToken = default);
    Task<PrimaryIpActionResponse> ChangeProtectionAsync(long id, PrimaryIpChangeProtectionRequest request, CancellationToken cancellationToken = default);
}

public interface IDnsZoneClient
{
    Task<DnsZoneListResponse> GetAllAsync(DnsZoneListOptions? options = null, CancellationToken cancellationToken = default);
    Task<DnsZoneResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<DnsZoneResponse> CreateAsync(DnsZoneCreateRequest request, CancellationToken cancellationToken = default);
    Task<DnsZoneResponse> UpdateAsync(long id, DnsZoneUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<DnsZoneRecordListResponse> GetAllRecordsAsync(long zoneId, DnsZoneRecordListOptions? options = null, CancellationToken cancellationToken = default);
    Task<DnsZoneRecordResponse> GetRecordByIdAsync(long zoneId, long recordId, CancellationToken cancellationToken = default);
    Task<DnsZoneRecordResponse> CreateRecordAsync(long zoneId, DnsZoneRecordCreateRequest request, CancellationToken cancellationToken = default);
    Task<DnsZoneRecordResponse> UpdateRecordAsync(long zoneId, long recordId, DnsZoneRecordUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteRecordAsync(long zoneId, long recordId, CancellationToken cancellationToken = default);
    Task ChangeProtectionAsync(long zoneId, DnsZoneProtectionChangeRequest request, CancellationToken cancellationToken = default);
}

public interface IStorageBoxClient
{
    Task<StorageBoxListResponse> GetAllAsync(StorageBoxListOptions? options = null, CancellationToken cancellationToken = default);
    Task<StorageBoxResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<StorageBoxResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<StorageBoxResponse> UpdateAsync(long id, StorageBoxUpdateRequest request, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(long id, StorageBoxChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task EnableSshAsync(long id, CancellationToken cancellationToken = default);
    Task DisableSshAsync(long id, CancellationToken cancellationToken = default);
    Task EnableWebdavAsync(long id, CancellationToken cancellationToken = default);
    Task DisableWebdavAsync(long id, CancellationToken cancellationToken = default);
    Task EnableSambaAsync(long id, CancellationToken cancellationToken = default);
    Task DisableSambaAsync(long id, CancellationToken cancellationToken = default);
}

public class ServerListOptions : PaginationRequest { }
public class ServerTypeListOptions : PaginationRequest { }
public class ImageListOptions : PaginationRequest 
{ 
    public string? Type { get; set; }
    public string? BoundTo { get; set; }
}
public class LocationListOptions : PaginationRequest { }
public class DatacenterListOptions : PaginationRequest { }
public class VolumeListOptions : PaginationRequest { }
public class LoadBalancerListOptions : PaginationRequest { }
public class LoadBalancerTypeListOptions : PaginationRequest { }
public class FloatingIpListOptions : PaginationRequest { }
public class NetworkListOptions : PaginationRequest { }
public class SshKeyListOptions : PaginationRequest { }
public class CertificateListOptions : PaginationRequest { }
public class PlacementGroupListOptions : PaginationRequest { }
public class FirewallListOptions : PaginationRequest { }
public class IsoImageListOptions : PaginationRequest { }
public class PrimaryIpListOptions : PaginationRequest
{
    public string? Name { get; set; }
}
public class DnsZoneListOptions : PaginationRequest { }
public class DnsZoneRecordListOptions : PaginationRequest { }
public class StorageBoxListOptions : PaginationRequest { }