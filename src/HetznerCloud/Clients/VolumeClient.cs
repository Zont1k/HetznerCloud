using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HetznerCloud.Exceptions;
using HetznerCloud.Models;
using HetznerCloud.Pagination;

namespace HetznerCloud.Clients;

public class VolumeClient : IVolumeClient
{
    private readonly HetznerCloudClient _client;

    public VolumeClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<VolumeListResponse> GetAllAsync(VolumeListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<VolumeListResponse>($"/volumes{queryParams}", cancellationToken);
    }

    public async Task<VolumeResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<VolumeResponse>($"/volumes/{id}", cancellationToken);
    }

    public async Task<VolumeResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var response = await GetAllAsync(new VolumeListOptions { Page = 1, PerPage = 1 }, cancellationToken);
        var volume = response.Volumes.Find(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (volume == null)
        {
            throw new NotFoundException($"Volume with name '{name}' not found");
        }
        return new VolumeResponse { Volume = volume };
    }

    public async Task<VolumeResponse> CreateAsync(VolumeCreateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<VolumeResponse, VolumeCreateRequest>("/volumes", request, cancellationToken);
    }

    public async Task<VolumeResponse> UpdateAsync(long id, VolumeUpdateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PutAsync<VolumeResponse, VolumeUpdateRequest>($"/volumes/{id}", request, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync<HttpResponseMessage>($"/volumes/{id}", cancellationToken);
    }

    public async Task<VolumeActionResponse> AttachAsync(long id, VolumeAttachRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<VolumeActionResponse, VolumeAttachRequest>($"/volumes/{id}/actions/attach", request, cancellationToken);
    }

    public async Task<VolumeActionResponse> DetachAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<VolumeActionResponse, VolumeActionRequest>($"/volumes/{id}/actions/detach", new VolumeActionRequest { Type = "detach" }, cancellationToken);
    }

    public async Task<VolumeActionResponse> ResizeAsync(long id, VolumeResizeRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<VolumeActionResponse, VolumeResizeRequest>($"/volumes/{id}/actions/resize", request, cancellationToken);
    }

    public async Task<VolumeActionResponse> ChangeProtectionAsync(long id, VolumeChangeProtectionRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<VolumeActionResponse, VolumeChangeProtectionRequest>($"/volumes/{id}/actions/change_protection", request, cancellationToken);
    }

    private string BuildQueryString(VolumeListOptions? options)
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