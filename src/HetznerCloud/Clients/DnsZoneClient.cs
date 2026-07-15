using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HetznerCloud.Exceptions;
using HetznerCloud.Models;
using HetznerCloud.Pagination;

namespace HetznerCloud.Clients;

public class DnsZoneClient : IDnsZoneClient
{
    private readonly HetznerCloudClient _client;

    public DnsZoneClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<DnsZoneListResponse> GetAllAsync(DnsZoneListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<DnsZoneListResponse>($"/dns_zones{queryParams}", cancellationToken);
    }

    public async Task<DnsZoneResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<DnsZoneResponse>($"/dns_zones/{id}", cancellationToken);
    }

    public async Task<DnsZoneResponse> CreateAsync(DnsZoneCreateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<DnsZoneResponse, DnsZoneCreateRequest>("/dns_zones", request, cancellationToken);
    }

    public async Task<DnsZoneResponse> UpdateAsync(long id, DnsZoneUpdateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PutAsync<DnsZoneResponse, DnsZoneUpdateRequest>($"/dns_zones/{id}", request, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync<HttpResponseMessage>($"/dns_zones/{id}", cancellationToken);
    }

    public async Task<DnsZoneRecordListResponse> GetAllRecordsAsync(long zoneId, DnsZoneRecordListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildRecordQueryString(options);
        return await _client.GetAsync<DnsZoneRecordListResponse>($"/dns_zones/{zoneId}/records{queryParams}", cancellationToken);
    }

    public async Task<DnsZoneRecordResponse> GetRecordByIdAsync(long zoneId, long recordId, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<DnsZoneRecordResponse>($"/dns_zones/{zoneId}/records/{recordId}", cancellationToken);
    }

    public async Task<DnsZoneRecordResponse> CreateRecordAsync(long zoneId, DnsZoneRecordCreateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<DnsZoneRecordResponse, DnsZoneRecordCreateRequest>($"/dns_zones/{zoneId}/records", request, cancellationToken);
    }

    public async Task<DnsZoneRecordResponse> UpdateRecordAsync(long zoneId, long recordId, DnsZoneRecordUpdateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PutAsync<DnsZoneRecordResponse, DnsZoneRecordUpdateRequest>($"/dns_zones/{zoneId}/records/{recordId}", request, cancellationToken);
    }

    public async Task DeleteRecordAsync(long zoneId, long recordId, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync<HttpResponseMessage>($"/dns_zones/{zoneId}/records/{recordId}", cancellationToken);
    }

    public async Task ChangeProtectionAsync(long zoneId, DnsZoneProtectionChangeRequest request, CancellationToken cancellationToken = default)
    {
        await _client.PostAsync<HttpResponseMessage, DnsZoneProtectionChangeRequest>($"/dns_zones/{zoneId}/actions/change_protection", request, cancellationToken);
    }

    private string BuildQueryString(DnsZoneListOptions? options)
    {
        if (options == null) return string.Empty;
        var parts = new List<string>();
        if (options.Page > 1) parts.Add($"page={options.Page}");
        if (options.PerPage != 25) parts.Add($"per_page={options.PerPage}");
        if (!string.IsNullOrEmpty(options.Sort)) parts.Add($"sort={Uri.EscapeDataString(options.Sort)}");
        if (!string.IsNullOrEmpty(options.LabelSelector)) parts.Add($"label_selector={Uri.EscapeDataString(options.LabelSelector)}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }

    private string BuildRecordQueryString(DnsZoneRecordListOptions? options)
    {
        if (options == null) return string.Empty;
        var parts = new List<string>();
        if (options.Page > 1) parts.Add($"page={options.Page}");
        if (options.PerPage != 25) parts.Add($"per_page={options.PerPage}");
        if (!string.IsNullOrEmpty(options.Sort)) parts.Add($"sort={Uri.EscapeDataString(options.Sort)}");
        if (!string.IsNullOrEmpty(options.LabelSelector)) parts.Add($"label_selector={Uri.EscapeDataString(options.LabelSelector)}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}
