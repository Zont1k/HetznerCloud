using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HetznerCloud.Exceptions;
using HetznerCloud.Models;
using HetznerCloud.Pagination;

namespace HetznerCloud.Clients;

public class LoadBalancerClient : ILoadBalancerClient
{
    private readonly HetznerCloudClient _client;

    public LoadBalancerClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<LoadBalancerListResponse> GetAllAsync(LoadBalancerListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<LoadBalancerListResponse>($"/load_balancers{queryParams}", cancellationToken);
    }

    public async Task<LoadBalancerResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<LoadBalancerResponse>($"/load_balancers/{id}", cancellationToken);
    }

    public async Task<LoadBalancerResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var response = await GetAllAsync(new LoadBalancerListOptions { Page = 1, PerPage = 1 }, cancellationToken);
        var lb = response.LoadBalancers.Find(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (lb == null)
        {
            throw new NotFoundException($"Load balancer with name '{name}' not found");
        }
        return new LoadBalancerResponse { LoadBalancer = lb };
    }

    public async Task<LoadBalancerResponse> CreateAsync(LoadBalancerCreateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<LoadBalancerResponse, LoadBalancerCreateRequest>("/load_balancers", request, cancellationToken);
    }

    public async Task<LoadBalancerResponse> UpdateAsync(long id, LoadBalancerUpdateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PutAsync<LoadBalancerResponse, LoadBalancerUpdateRequest>($"/load_balancers/{id}", request, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync<HttpResponseMessage>($"/load_balancers/{id}", cancellationToken);
    }

    public async Task<LoadBalancerTypeListResponse> GetTypesAsync(LoadBalancerTypeListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<LoadBalancerTypeListResponse>($"/load_balancer_types{queryParams}", cancellationToken);
    }

    public async Task<LoadBalancerTypeResponse> GetTypeByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<LoadBalancerTypeResponse>($"/load_balancer_types/{id}", cancellationToken);
    }

    public async Task<LoadBalancerTypeResponse> GetTypeByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var response = await GetTypesAsync(new LoadBalancerTypeListOptions { Page = 1, PerPage = 1 }, cancellationToken);
        var type = response.LoadBalancerTypes.Find(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (type == null)
        {
            throw new NotFoundException($"Load balancer type with name '{name}' not found");
        }
        return new LoadBalancerTypeResponse { LoadBalancerType = type };
    }

    public async Task<LoadBalancerActionResponse> AddServiceAsync(long id, LoadBalancerAddServiceRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<LoadBalancerActionResponse, LoadBalancerAddServiceRequest>($"/load_balancers/{id}/actions/add_service", request, cancellationToken);
    }

    public async Task<LoadBalancerActionResponse> UpdateServiceAsync(long id, LoadBalancerUpdateServiceRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<LoadBalancerActionResponse, LoadBalancerUpdateServiceRequest>($"load_balancers/{id}/actions/update_service", request, cancellationToken);
    }

    public async Task<LoadBalancerActionResponse> DeleteServiceAsync(long id, LoadBalancerDeleteServiceRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<LoadBalancerActionResponse, LoadBalancerDeleteServiceRequest>($"/load_balancers/{id}/actions/delete_service", request, cancellationToken);
    }

    public async Task<LoadBalancerActionResponse> AddTargetAsync(long id, LoadBalancerAddTargetRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<LoadBalancerActionResponse, LoadBalancerAddTargetRequest>($"/load_balancers/{id}/actions/add_target", request, cancellationToken);
    }

    public async Task<LoadBalancerActionResponse> RemoveTargetAsync(long id, LoadBalancerRemoveTargetRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<LoadBalancerActionResponse, LoadBalancerRemoveTargetRequest>($"/load_balancers/{id}/actions/remove_target", request, cancellationToken);
    }

    public async Task<LoadBalancerActionResponse> ChangeAlgorithmAsync(long id, LoadBalancerChangeAlgorithmRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<LoadBalancerActionResponse, LoadBalancerChangeAlgorithmRequest>($"/load_balancers/{id}/actions/change_algorithm", request, cancellationToken);
    }

    public async Task<LoadBalancerActionResponse> ChangeTypeAsync(long id, LoadBalancerChangeTypeRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<LoadBalancerActionResponse, LoadBalancerChangeTypeRequest>($"/load_balancers/{id}/actions/change_type", request, cancellationToken);
    }

    public async Task<LoadBalancerActionResponse> EnablePublicInterfaceAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<LoadBalancerActionResponse, LoadBalancerActionRequest>($"/load_balancers/{id}/actions/enable_public_interface", new LoadBalancerActionRequest { Type = "enable_public_interface" }, cancellationToken);
    }

    public async Task<LoadBalancerActionResponse> DisablePublicInterfaceAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<LoadBalancerActionResponse, LoadBalancerActionRequest>($"/load_balancers/{id}/actions/disable_public_interface", new LoadBalancerActionRequest { Type = "disable_public_interface" }, cancellationToken);
    }

    public async Task<LoadBalancerActionResponse> AttachToNetworkAsync(long id, LoadBalancerAttachToNetworkRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<LoadBalancerActionResponse, LoadBalancerAttachToNetworkRequest>($"/load_balancers/{id}/actions/attach_to_network", request, cancellationToken);
    }

    public async Task<LoadBalancerActionResponse> DetachFromNetworkAsync(long id, LoadBalancerDetachFromNetworkRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<LoadBalancerActionResponse, LoadBalancerDetachFromNetworkRequest>($"/load_balancers/{id}/actions/detach_from_network", request, cancellationToken);
    }

    public async Task<LoadBalancerActionResponse> ChangeProtectionAsync(long id, LoadBalancerChangeProtectionRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<LoadBalancerActionResponse, LoadBalancerChangeProtectionRequest>($"/load_balancers/{id}/actions/change_protection", request, cancellationToken);
    }

    public async Task<LoadBalancerActionResponse> ChangeIpAsync(long id, LoadBalancerChangeIpRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<LoadBalancerActionResponse, LoadBalancerChangeIpRequest>($"/load_balancers/{id}/actions/change_ip", request, cancellationToken);
    }

    private string BuildQueryString(LoadBalancerListOptions? options)
    {
        if (options == null) return string.Empty;
        var parts = new List<string>();
        if (options.Page > 0) parts.Add($"page={options.Page}");
        if (options.PerPage > 0) parts.Add($"per_page={options.PerPage}");
        if (!string.IsNullOrEmpty(options.Sort)) parts.Add($"sort={Uri.EscapeDataString(options.Sort)}");
        if (!string.IsNullOrEmpty(options.LabelSelector)) parts.Add($"label_selector={Uri.EscapeDataString(options.LabelSelector)}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }

    private string BuildQueryString(LoadBalancerTypeListOptions? options)
    {
        if (options == null) return string.Empty;
        var parts = new List<string>();
        if (options.Page > 0) parts.Add($"page={options.Page}");
        if (options.PerPage > 0) parts.Add($"per_page={options.PerPage}");
        if (!string.IsNullOrEmpty(options.Sort)) parts.Add($"sort={Uri.EscapeDataString(options.Sort)}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}