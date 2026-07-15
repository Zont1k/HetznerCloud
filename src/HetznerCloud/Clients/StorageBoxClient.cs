using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HetznerCloud.Exceptions;
using HetznerCloud.Models;
using HetznerCloud.Pagination;

namespace HetznerCloud.Clients;

public class StorageBoxClient : IStorageBoxClient
{
    private readonly HetznerCloudClient _client;

    public StorageBoxClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<StorageBoxListResponse> GetAllAsync(StorageBoxListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<StorageBoxListResponse>($"/storage_boxes{queryParams}", cancellationToken);
    }

    public async Task<StorageBoxResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<StorageBoxResponse>($"/storage_boxes/{id}", cancellationToken);
    }

    public async Task<StorageBoxResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var response = await GetAllAsync(new StorageBoxListOptions { Page = 1, PerPage = 50 }, cancellationToken);
        var storageBox = response.StorageBoxes.Find(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (storageBox == null)
        {
            throw new NotFoundException($"Storage box with name '{name}' not found");
        }
        return new StorageBoxResponse { StorageBox = storageBox };
    }

    public async Task<StorageBoxResponse> UpdateAsync(long id, StorageBoxUpdateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PutAsync<StorageBoxResponse, StorageBoxUpdateRequest>($"/storage_boxes/{id}", request, cancellationToken);
    }

    public async Task ChangePasswordAsync(long id, StorageBoxChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        await _client.PostAsync<HttpResponseMessage, StorageBoxChangePasswordRequest>($"/storage_boxes/{id}/actions/change_password", request, cancellationToken);
    }

    public async Task EnableSshAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.PostAsync<HttpResponseMessage, object>($"/storage_boxes/{id}/actions/enable_ssh", new { }, cancellationToken);
    }

    public async Task DisableSshAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.PostAsync<HttpResponseMessage, object>($"/storage_boxes/{id}/actions/disable_ssh", new { }, cancellationToken);
    }

    public async Task EnableWebdavAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.PostAsync<HttpResponseMessage, object>($"/storage_boxes/{id}/actions/enable_webdav", new { }, cancellationToken);
    }

    public async Task DisableWebdavAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.PostAsync<HttpResponseMessage, object>($"/storage_boxes/{id}/actions/disable_webdav", new { }, cancellationToken);
    }

    public async Task EnableSambaAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.PostAsync<HttpResponseMessage, object>($"/storage_boxes/{id}/actions/enable_samba", new { }, cancellationToken);
    }

    public async Task DisableSambaAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.PostAsync<HttpResponseMessage, object>($"/storage_boxes/{id}/actions/disable_samba", new { }, cancellationToken);
    }

    private string BuildQueryString(StorageBoxListOptions? options)
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
