using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HetznerCloud.Exceptions;
using HetznerCloud.Models;
using HetznerCloud.Pagination;

namespace HetznerCloud.Clients;

public class PrimaryIpClient : IPrimaryIpClient
{
    private readonly HetznerCloudClient _client;

    public PrimaryIpClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<PrimaryIpListResponse> GetAllAsync(PrimaryIpListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<PrimaryIpListResponse>($"/primary_ips{queryParams}", cancellationToken);
    }

    public async Task<PrimaryIpResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<PrimaryIpResponse>($"/primary_ips/{id}", cancellationToken);
    }

    public async Task<PrimaryIpResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var response = await GetAllAsync(new PrimaryIpListOptions { Page = 1, PerPage = 50 }, cancellationToken);
        var primaryIp = response.PrimaryIps.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (primaryIp == null)
        {
            throw new NotFoundException($"Primary IP with name '{name}' not found");
        }
        return new PrimaryIpResponse { PrimaryIp = primaryIp };
    }

    public async Task<PrimaryIpResponse> CreateAsync(PrimaryIpCreateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<PrimaryIpResponse, PrimaryIpCreateRequest>("/primary_ips", request, cancellationToken);
    }

    public async Task<PrimaryIpResponse> UpdateAsync(long id, PrimaryIpUpdateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PutAsync<PrimaryIpResponse, PrimaryIpUpdateRequest>($"/primary_ips/{id}", request, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync<HttpResponseMessage>($"/primary_ips/{id}", cancellationToken);
    }

    public async Task<PrimaryIpActionResponse> AssignAsync(long id, PrimaryIpAssignRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<PrimaryIpActionResponse, PrimaryIpAssignRequest>($"/primary_ips/{id}/actions/assign", request, cancellationToken);
    }

    public async Task<PrimaryIpActionResponse> UnassignAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<PrimaryIpActionResponse, object>($"/primary_ips/{id}/actions/unassign", new { }, cancellationToken);
    }

    public async Task<PrimaryIpActionResponse> ChangeDnsPtrAsync(long id, PrimaryIpChangeDnsPtrRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<PrimaryIpActionResponse, PrimaryIpChangeDnsPtrRequest>($"/primary_ips/{id}/actions/change_dns_ptr", request, cancellationToken);
    }

    public async Task<PrimaryIpActionResponse> ChangeProtectionAsync(long id, PrimaryIpChangeProtectionRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<PrimaryIpActionResponse, PrimaryIpChangeProtectionRequest>($"/primary_ips/{id}/actions/change_protection", request, cancellationToken);
    }

    private string BuildQueryString(PrimaryIpListOptions? options)
    {
        if (options == null) return string.Empty;
        var parts = new List<string>();
        if (options.Page > 1) parts.Add($"page={options.Page}");
        if (options.PerPage != 25) parts.Add($"per_page={options.PerPage}");
        if (!string.IsNullOrEmpty(options.Sort)) parts.Add($"sort={Uri.EscapeDataString(options.Sort)}");
        if (!string.IsNullOrEmpty(options.Name)) parts.Add($"name={Uri.EscapeDataString(options.Name)}");
        if (!string.IsNullOrEmpty(options.LabelSelector)) parts.Add($"label_selector={Uri.EscapeDataString(options.LabelSelector)}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}
