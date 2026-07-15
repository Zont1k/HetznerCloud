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

public class ActionClient : IActionClient
{
    private readonly HetznerCloudClient _client;

    public ActionClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<ActionListResponse> GetAllAsync(ActionListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<ActionListResponse>($"/actions{queryParams}", cancellationToken);
    }

    public async Task<ActionResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<ActionResponse>($"/actions/{id}", cancellationToken);
    }

    public async Task<ActionResponse> WaitForAsync(long actionId, CancellationToken cancellationToken = default)
    {
        const int pollIntervalMs = 1000;
        const int maxAttempts = 300;

        for (int i = 0; i < maxAttempts; i++)
        {
            var response = await GetByIdAsync(actionId, cancellationToken);
            if (response.Action.Status == ActionStatus.Success)
            {
                return response;
            }
            if (response.Action.Status == ActionStatus.Error)
            {
                throw new HetznerCloudException($"Action {actionId} failed: {response.Action.Error?.Message ?? "Unknown error"}", System.Net.HttpStatusCode.InternalServerError, "action_failed");
            }

            await Task.Delay(pollIntervalMs, cancellationToken);
        }

        throw new TimeoutException($"Action {actionId} did not complete within the timeout period");
    }

    public async Task<List<Models.Action>> WaitForAllAsync(IEnumerable<long> actionIds, CancellationToken cancellationToken = default)
    {
        var actions = new List<Models.Action>();
        foreach (var id in actionIds)
        {
            var response = await WaitForAsync(id, cancellationToken);
            actions.Add(response.Action);
        }
        return actions;
    }

    private string BuildQueryString(ActionListOptions? options)
    {
        if (options == null) return string.Empty;
        var parts = new List<string>();
        if (options.Page > 0) parts.Add($"page={options.Page}");
        if (options.PerPage > 0) parts.Add($"per_page={options.PerPage}");
        if (!string.IsNullOrEmpty(options.Sort)) parts.Add($"sort={Uri.EscapeDataString(options.Sort)}");
        if (!string.IsNullOrEmpty(options.Status)) parts.Add($"status={Uri.EscapeDataString(options.Status)}");
        if (options.ResourceId.HasValue) parts.Add($"resource_id={options.ResourceId.Value}");
        if (!string.IsNullOrEmpty(options.ResourceType)) parts.Add($"resource_type={Uri.EscapeDataString(options.ResourceType)}");
        if (options.StartedAfter.HasValue) parts.Add($"started_after={options.StartedAfter.Value:O}");
        if (options.StartedBefore.HasValue) parts.Add($"started_before={options.StartedBefore.Value:O}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}