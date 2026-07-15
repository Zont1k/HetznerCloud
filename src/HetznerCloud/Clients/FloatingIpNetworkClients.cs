using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HetznerCloud.Exceptions;
using HetznerCloud.Models;
using HetznerCloud.Pagination;

namespace HetznerCloud.Clients;

public class FloatingIpClient : IFloatingIpClient
{
    private readonly HetznerCloudClient _client;

    public FloatingIpClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<FloatingIpListResponse> GetAllAsync(FloatingIpListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<FloatingIpListResponse>($"/floating_ips{queryParams}", cancellationToken);
    }

    public async Task<FloatingIpResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<FloatingIpResponse>($"/floating_ips/{id}", cancellationToken);
    }

    public async Task<FloatingIpResponse> CreateAsync(FloatingIpCreateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<FloatingIpResponse, FloatingIpCreateRequest>("/floating_ips", request, cancellationToken);
    }

    public async Task<FloatingIpResponse> UpdateAsync(long id, FloatingIpUpdateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PutAsync<FloatingIpResponse, FloatingIpUpdateRequest>($"/floating_ips/{id}", request, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync<HttpResponseMessage>($"/floating_ips/{id}", cancellationToken);
    }

    public async Task<FloatingIpActionResponse> AssignAsync(long id, FloatingIpAssignRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<FloatingIpActionResponse, FloatingIpAssignRequest>($"/floating_ips/{id}/actions/assign", request, cancellationToken);
    }

    public async Task<FloatingIpActionResponse> UnassignAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<FloatingIpActionResponse, FloatingIpActionRequest>($"/floating_ips/{id}/actions/unassign", new FloatingIpActionRequest { Type = "unassign" }, cancellationToken);
    }

    public async Task<FloatingIpActionResponse> ChangeDnsPtrAsync(long id, FloatingIpChangeDnsPtrRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<FloatingIpActionResponse, FloatingIpChangeDnsPtrRequest>($"/floating_ips/{id}/actions/change_dns_ptr", request, cancellationToken);
    }

    public async Task<FloatingIpActionResponse> ChangeProtectionAsync(long id, FloatingIpChangeProtectionRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<FloatingIpActionResponse, FloatingIpChangeProtectionRequest>($"/floating_ips/{id}/actions/change_protection", request, cancellationToken);
    }

    private string BuildQueryString(FloatingIpListOptions? options)
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

public class NetworkClient : INetworkClient
{
    private readonly HetznerCloudClient _client;

    public NetworkClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<NetworkListResponse> GetAllAsync(NetworkListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<NetworkListResponse>($"/networks{queryParams}", cancellationToken);
    }

    public async Task<NetworkResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<NetworkResponse>($"/networks/{id}", cancellationToken);
    }

    public async Task<NetworkResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var response = await GetAllAsync(new NetworkListOptions { Page = 1, PerPage = 1 }, cancellationToken);
        var network = response.Networks.Find(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (network == null)
        {
            throw new NotFoundException($"Network with name '{name}' not found");
        }
        return new NetworkResponse { Network = network };
    }

    public async Task<NetworkResponse> CreateAsync(NetworkCreateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<NetworkResponse, NetworkCreateRequest>("/networks", request, cancellationToken);
    }

    public async Task<NetworkResponse> UpdateAsync(long id, NetworkUpdateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PutAsync<NetworkResponse, NetworkUpdateRequest>($"/networks/{id}", request, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync<HttpResponseMessage>($"/networks/{id}", cancellationToken);
    }

    public async Task<NetworkActionResponse> AddSubnetAsync(long id, NetworkAddSubnetRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<NetworkActionResponse, NetworkAddSubnetRequest>($"/networks/{id}/actions/add_subnet", request, cancellationToken);
    }

    public async Task<NetworkActionResponse> DeleteSubnetAsync(long id, NetworkDeleteSubnetRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<NetworkActionResponse, NetworkDeleteSubnetRequest>($"/networks/{id}/actions/delete_subnet", request, cancellationToken);
    }

    public async Task<NetworkActionResponse> ChangeIpRangeAsync(long id, NetworkChangeIpRangeRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<NetworkActionResponse, NetworkChangeIpRangeRequest>($"/networks/{id}/actions/change_ip_range", request, cancellationToken);
    }

    public async Task<NetworkActionResponse> AddRouteAsync(long id, NetworkAddRouteRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<NetworkActionResponse, NetworkAddRouteRequest>($"networks/{id}/actions/add_route", request, cancellationToken);
    }

    public async Task<NetworkActionResponse> DeleteRouteAsync(long id, NetworkDeleteRouteRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<NetworkActionResponse, NetworkDeleteRouteRequest>($"networks/{id}/actions/delete_route", request, cancellationToken);
    }

    public async Task<NetworkActionResponse> ChangeProtectionAsync(long id, NetworkChangeProtectionRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<NetworkActionResponse, NetworkChangeProtectionRequest>($"/networks/{id}/actions/change_protection", request, cancellationToken);
    }

    private string BuildQueryString(NetworkListOptions? options)
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