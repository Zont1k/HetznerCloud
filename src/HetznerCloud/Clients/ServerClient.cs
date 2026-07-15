using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HetznerCloud.Exceptions;
using HetznerCloud.Models;
using HetznerCloud.Pagination;

namespace HetznerCloud.Clients;

public class ServerClient : IServerClient
{
    private readonly HetznerCloudClient _client;

    public ServerClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<ServerListResponse> GetAllAsync(ServerListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<ServerListResponse>($"/servers{queryParams}", cancellationToken);
    }

    public async Task<ServerResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<ServerResponse>($"/servers/{id}", cancellationToken);
    }

    public async Task<ServerResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var response = await GetAllAsync(new ServerListOptions { Page = 1, PerPage = 1 }, cancellationToken);
        var server = response.Servers.Find(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (server == null)
        {
            throw new NotFoundException($"Server with name '{name}' not found");
        }
        return new ServerResponse { Server = server };
    }

    public async Task<ServerCreateResponse> CreateAsync(ServerCreateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerCreateResponse, ServerCreateRequest>("/servers", request, cancellationToken);
    }

    public async Task<ServerResponse> UpdateAsync(long id, ServerUpdateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PutAsync<ServerResponse, ServerUpdateRequest>($"/servers/{id}", request, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync<HttpResponseMessage>($"/servers/{id}", cancellationToken);
    }

    public async Task<ServerActionResponse> PowerOnAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerActionRequest>($"/servers/{id}/actions/poweron", new ServerActionRequest { Type = "poweron" }, cancellationToken);
    }

    public async Task<ServerActionResponse> PowerOffAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerActionRequest>($"/servers/{id}/actions/poweroff", new ServerActionRequest { Type = "poweroff" }, cancellationToken);
    }

    public async Task<ServerActionResponse> RebootAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerActionRequest>($"/servers/{id}/actions/reboot", new ServerActionRequest { Type = "reboot" }, cancellationToken);
    }

    public async Task<ServerActionResponse> ResetAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerActionRequest>($"/servers/{id}/actions/reset", new ServerActionRequest { Type = "reset" }, cancellationToken);
    }

    public async Task<ServerActionResponse> RebuildAsync(long id, ServerRebuildRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerRebuildRequest>($"/servers/{id}/actions/rebuild", request, cancellationToken);
    }

    public async Task<ServerActionResponse> ChangeTypeAsync(long id, ServerChangeTypeRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerChangeTypeRequest>($"/servers/{id}/actions/change_type", request, cancellationToken);
    }

    public async Task<ServerActionResponse> ChangeProtectionAsync(long id, ServerChangeProtectionRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerChangeProtectionRequest>($"/servers/{id}/actions/change_protection", request, cancellationToken);
    }

    public async Task<ServerActionResponse> EnableBackupAsync(long id, ServerEnableBackupRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerEnableBackupRequest>($"/servers/{id}/actions/enable_backup", request, cancellationToken);
    }

    public async Task<ServerActionResponse> DisableBackupAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerActionRequest>($"/servers/{id}/actions/disable_backup", new ServerActionRequest { Type = "disable_backup" }, cancellationToken);
    }

    public async Task<ServerActionResponse> ChangeDnsPtrAsync(long id, ServerChangeDnsPtrRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerChangeDnsPtrRequest>($"/servers/{id}/actions/change_dns_ptr", request, cancellationToken);
    }

    public async Task<ServerActionResponse> AttachToNetworkAsync(long id, ServerAttachToNetworkRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerAttachToNetworkRequest>($"/servers/{id}/actions/attach_to_network", request, cancellationToken);
    }

    public async Task<ServerActionResponse> DetachFromNetworkAsync(long id, ServerDetachFromNetworkRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerDetachFromNetworkRequest>($"/servers/{id}/actions/detach_from_network", request, cancellationToken);
    }

    public async Task<ServerActionResponse> CreateImageAsync(long id, ServerCreateImageRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerCreateImageRequest>($"/servers/{id}/actions/create_image", request, cancellationToken);
    }

    public async Task<ServerActionResponse> ReassignIpAsync(long id, ServerReassignIpRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerReassignIpRequest>($"/servers/{id}/actions/reassign_ip", request, cancellationToken);
    }

    public async Task<ServerActionResponse> ResetPasswordAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerActionRequest>($"/servers/{id}/actions/reset_password", new ServerActionRequest { Type = "reset_password" }, cancellationToken);
    }

    public async Task<ServerActionResponse> EnableRescueAsync(long id, ServerEnableRescueRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerEnableRescueRequest>($"/servers/{id}/actions/enable_rescue", request, cancellationToken);
    }

    public async Task<ServerActionResponse> DisableRescueAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerActionRequest>($"/servers/{id}/actions/disable_rescue", new ServerActionRequest { Type = "disable_rescue" }, cancellationToken);
    }

    public async Task<ServerActionResponse> ChangeAliasIpsAsync(long id, ServerChangeAliasIpsRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerChangeAliasIpsRequest>($"/servers/{id}/actions/change_alias_ips", request, cancellationToken);
    }

    public async Task<ServerActionResponse> AttachIsoAsync(long id, ServerAttachIsoRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerAttachIsoRequest>($"/servers/{id}/actions/attach_iso", request, cancellationToken);
    }

    public async Task<ServerActionResponse> DetachIsoAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ServerActionResponse, ServerActionRequest>($"/servers/{id}/actions/detach_iso", new ServerActionRequest { Type = "detach_iso" }, cancellationToken);
    }

    public async Task<ActionResponse> WaitForActionAsync(long actionId, CancellationToken cancellationToken = default)
    {
        return await _client.Actions.WaitForAsync(actionId, cancellationToken);
    }

    public async Task<List<Models.Action>> WaitForActionsAsync(IEnumerable<long> actionIds, CancellationToken cancellationToken = default)
    {
        return await _client.Actions.WaitForAllAsync(actionIds, cancellationToken);
    }

    private string BuildQueryString(ServerListOptions? options)
    {
        if (options == null) return string.Empty;
        var parts = new List<string>();
        if (options.Page > 0) parts.Add($"page={options.Page}");
        if (options.PerPage > 0) parts.Add($"per_page={options.PerPage}");
        if (!string.IsNullOrEmpty(options.Sort)) parts.Add($"sort={Uri.EscapeDataString(options.Sort)}");
        if (!string.IsNullOrEmpty(options.LabelSelector)) parts.Add($"label_selector={Uri.EscapeDataString(options.LabelSelector)}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}

public class ServerTypeClient : IServerTypeClient
{
    private readonly HetznerCloudClient _client;

    public ServerTypeClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<ServerTypeListResponse> GetAllAsync(ServerTypeListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<ServerTypeListResponse>($"/server_types{queryParams}", cancellationToken);
    }

    public async Task<ServerTypeResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<ServerTypeResponse>($"/server_types/{id}", cancellationToken);
    }

    public async Task<ServerTypeResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var response = await GetAllAsync(new ServerTypeListOptions { Page = 1, PerPage = 1 }, cancellationToken);
        var serverType = response.ServerTypes.Find(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (serverType == null)
        {
            throw new NotFoundException($"Server type with name '{name}' not found");
        }
        return new ServerTypeResponse { ServerType = serverType };
    }

    private string BuildQueryString(ServerTypeListOptions? options)
    {
        if (options == null) return string.Empty;
        var parts = new List<string>();
        if (options.Page > 0) parts.Add($"page={options.Page}");
        if (options.PerPage > 0) parts.Add($"per_page={options.PerPage}");
        if (!string.IsNullOrEmpty(options.Sort)) parts.Add($"sort={Uri.EscapeDataString(options.Sort)}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}

public class ImageClient : IImageClient
{
    private readonly HetznerCloudClient _client;

    public ImageClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<ImageListResponse> GetAllAsync(ImageListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<ImageListResponse>($"/images{queryParams}", cancellationToken);
    }

    public async Task<ImageResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<ImageResponse>($"/images/{id}", cancellationToken);
    }

    public async Task<ImageResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var response = await GetAllAsync(new ImageListOptions { Page = 1, PerPage = 1 }, cancellationToken);
        var image = response.Images.Find(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (image == null)
        {
            throw new NotFoundException($"Image with name '{name}' not found");
        }
        return new ImageResponse { Image = image };
    }

    public async Task<ImageResponse> UpdateAsync(long id, ImageUpdateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PutAsync<ImageResponse, ImageUpdateRequest>($"/images/{id}", request, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync<HttpResponseMessage>($"/images/{id}", cancellationToken);
    }

    public async Task<ImageActionResponse> ChangeProtectionAsync(long id, ImageChangeProtectionRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<ImageActionResponse, ImageChangeProtectionRequest>($"/images/{id}/actions/change_protection", request, cancellationToken);
    }

    private string BuildQueryString(ImageListOptions? options)
    {
        if (options == null) return string.Empty;
        var parts = new List<string>();
        if (options.Page > 0) parts.Add($"page={options.Page}");
        if (options.PerPage > 0) parts.Add($"per_page={options.PerPage}");
        if (!string.IsNullOrEmpty(options.Sort)) parts.Add($"sort={Uri.EscapeDataString(options.Sort)}");
        if (!string.IsNullOrEmpty(options.LabelSelector)) parts.Add($"label_selector={Uri.EscapeDataString(options.LabelSelector)}");
        if (!string.IsNullOrEmpty(options.Type)) parts.Add($"type={Uri.EscapeDataString(options.Type)}");
        if (!string.IsNullOrEmpty(options.BoundTo)) parts.Add($"bound_to={Uri.EscapeDataString(options.BoundTo)}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}

public class LocationClient : ILocationClient
{
    private readonly HetznerCloudClient _client;

    public LocationClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<LocationListResponse> GetAllAsync(LocationListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<LocationListResponse>($"/locations{queryParams}", cancellationToken);
    }

    public async Task<LocationResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<LocationResponse>($"/locations/{id}", cancellationToken);
    }

    public async Task<LocationResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var response = await GetAllAsync(new LocationListOptions { Page = 1, PerPage = 1 }, cancellationToken);
        var location = response.Locations.Find(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (location == null)
        {
            throw new NotFoundException($"Location with name '{name}' not found");
        }
        return new LocationResponse { Location = location };
    }

    private string BuildQueryString(LocationListOptions? options)
    {
        if (options == null) return string.Empty;
        var parts = new List<string>();
        if (options.Page > 0) parts.Add($"page={options.Page}");
        if (options.PerPage > 0) parts.Add($"per_page={options.PerPage}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}

public class DatacenterClient : IDatacenterClient
{
    private readonly HetznerCloudClient _client;

    public DatacenterClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<DatacenterListResponse> GetAllAsync(DatacenterListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<DatacenterListResponse>($"/datacenters{queryParams}", cancellationToken);
    }

    public async Task<DatacenterResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<DatacenterResponse>($"/datacenters/{id}", cancellationToken);
    }

    public async Task<DatacenterResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var response = await GetAllAsync(new DatacenterListOptions { Page = 1, PerPage = 1 }, cancellationToken);
        var datacenter = response.Datacenters.Find(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (datacenter == null)
        {
            throw new NotFoundException($"Datacenter with name '{name}' not found");
        }
        return new DatacenterResponse { Datacenter = datacenter };
    }

    private string BuildQueryString(DatacenterListOptions? options)
    {
        if (options == null) return string.Empty;
        var parts = new List<string>();
        if (options.Page > 0) parts.Add($"page={options.Page}");
        if (options.PerPage > 0) parts.Add($"per_page={options.PerPage}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}