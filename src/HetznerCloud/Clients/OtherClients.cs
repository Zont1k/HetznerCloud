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

public class SshKeyClient : ISSHKeyClient
{
    private readonly HetznerCloudClient _client;

    public SshKeyClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<SshKeyListResponse> GetAllAsync(SshKeyListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<SshKeyListResponse>($"/ssh_keys{queryParams}", cancellationToken);
    }

    public async Task<SshKeyResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<SshKeyResponse>($"/ssh_keys/{id}", cancellationToken);
    }

    public async Task<SshKeyResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var response = await GetAllAsync(new SshKeyListOptions { Page = 1, PerPage = 1 }, cancellationToken);
        var key = response.SshKeys.Find(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (key == null)
        {
            throw new NotFoundException($"SSH key with name '{name}' not found");
        }
        return new SshKeyResponse { SshKey = key };
    }

    public async Task<SshKeyResponse> CreateAsync(SshKeyCreateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<SshKeyResponse, SshKeyCreateRequest>("/ssh_keys", request, cancellationToken);
    }

    public async Task<SshKeyResponse> UpdateAsync(long id, SshKeyUpdateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PutAsync<SshKeyResponse, SshKeyUpdateRequest>($"/ssh_keys/{id}", request, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync<HttpResponseMessage>($"/ssh_keys/{id}", cancellationToken);
    }

    private string BuildQueryString(SshKeyListOptions? options)
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

public class CertificateClient : ICertificateClient
{
    private readonly HetznerCloudClient _client;

    public CertificateClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<CertificateListResponse> GetAllAsync(CertificateListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<CertificateListResponse>($"/certificates{queryParams}", cancellationToken);
    }

    public async Task<CertificateResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<CertificateResponse>($"/certificates/{id}", cancellationToken);
    }

    public async Task<CertificateResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var response = await GetAllAsync(new CertificateListOptions { Page = 1, PerPage = 1 }, cancellationToken);
        var cert = response.Certificates.Find(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (cert == null)
        {
            throw new NotFoundException($"Certificate with name '{name}' not found");
        }
        return new CertificateResponse { Certificate = cert };
    }

    public async Task<CertificateResponse> CreateAsync(CertificateCreateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<CertificateResponse, CertificateCreateRequest>("/certificates", request, cancellationToken);
    }

    public async Task<CertificateResponse> UpdateAsync(long id, CertificateUpdateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PutAsync<CertificateResponse, CertificateUpdateRequest>($"/certificates/{id}", request, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync<HttpResponseMessage>($"/certificates/{id}", cancellationToken);
    }

    private string BuildQueryString(CertificateListOptions? options)
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

public class PlacementGroupClient : IPlacementGroupClient
{
    private readonly HetznerCloudClient _client;

    public PlacementGroupClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<PlacementGroupListResponse> GetAllAsync(PlacementGroupListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<PlacementGroupListResponse>($"/placement_groups{queryParams}", cancellationToken);
    }

    public async Task<PlacementGroupResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<PlacementGroupResponse>($"/placement_groups/{id}", cancellationToken);
    }

    public async Task<PlacementGroupResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var response = await GetAllAsync(new PlacementGroupListOptions { Page = 1, PerPage = 1 }, cancellationToken);
        var pg = response.PlacementGroups.Find(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (pg == null)
        {
            throw new NotFoundException($"Placement group with name '{name}' not found");
        }
        return new PlacementGroupResponse { PlacementGroup = pg };
    }

    public async Task<PlacementGroupResponse> CreateAsync(PlacementGroupCreateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<PlacementGroupResponse, PlacementGroupCreateRequest>("/placement_groups", request, cancellationToken);
    }

    public async Task<PlacementGroupResponse> UpdateAsync(long id, PlacementGroupUpdateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PutAsync<PlacementGroupResponse, PlacementGroupUpdateRequest>($"/placement_groups/{id}", request, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync<HttpResponseMessage>($"/placement_groups/{id}", cancellationToken);
    }

    private string BuildQueryString(PlacementGroupListOptions? options)
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

public class FirewallClient : IFirewallClient
{
    private readonly HetznerCloudClient _client;

    public FirewallClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<FirewallListResponse> GetAllAsync(FirewallListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<FirewallListResponse>($"/firewalls{queryParams}", cancellationToken);
    }

    public async Task<FirewallResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<FirewallResponse>($"/firewalls/{id}", cancellationToken);
    }

    public async Task<FirewallResponse> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var response = await GetAllAsync(new FirewallListOptions { Page = 1, PerPage = 1 }, cancellationToken);
        var fw = response.Firewalls.Find(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (fw == null)
        {
            throw new NotFoundException($"Firewall with name '{name}' not found");
        }
        return new FirewallResponse { Firewall = fw };
    }

    public async Task<FirewallResponse> CreateAsync(FirewallCreateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<FirewallResponse, FirewallCreateRequest>("/firewalls", request, cancellationToken);
    }

    public async Task<FirewallResponse> UpdateAsync(long id, FirewallUpdateRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PutAsync<FirewallResponse, FirewallUpdateRequest>($"/firewalls/{id}", request, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync<HttpResponseMessage>($"/firewalls/{id}", cancellationToken);
    }

    public async Task<FirewallActionResponse> SetRulesAsync(long id, FirewallSetRulesRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<FirewallActionResponse, FirewallSetRulesRequest>($"/firewalls/{id}/actions/set_rules", request, cancellationToken);
    }

    public async Task<FirewallActionResponse> ApplyToResourcesAsync(long id, FirewallApplyToResourcesRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<FirewallActionResponse, FirewallApplyToResourcesRequest>($"/firewalls/{id}/actions/apply_to_resources", request, cancellationToken);
    }

    public async Task<FirewallActionResponse> RemoveFromResourcesAsync(long id, FirewallRemoveFromResourcesRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<FirewallActionResponse, FirewallRemoveFromResourcesRequest>($"/firewalls/{id}/actions/remove_from_resources", request, cancellationToken);
    }

    public async Task<FirewallActionResponse> ChangeProtectionAsync(long id, FirewallChangeProtectionRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<FirewallActionResponse, FirewallChangeProtectionRequest>($"firewalls/{id}/actions/change_protection", request, cancellationToken);
    }

    private string BuildQueryString(FirewallListOptions? options)
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

public class IsoImageClient : IIsoImageClient
{
    private readonly HetznerCloudClient _client;

    public IsoImageClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<IsoImageListResponse> GetAllAsync(IsoImageListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildQueryString(options);
        return await _client.GetAsync<IsoImageListResponse>($"/isos{queryParams}", cancellationToken);
    }

    public async Task<IsoImageResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<IsoImageResponse>($"isos/{id}", cancellationToken);
    }

    private string BuildQueryString(IsoImageListOptions? options)
    {
        if (options == null) return string.Empty;
        var parts = new List<string>();
        if (options.Page > 0) parts.Add($"page={options.Page}");
        if (options.PerPage > 0) parts.Add($"per_page={options.PerPage}");
        if (!string.IsNullOrEmpty(options.Sort)) parts.Add($"sort={Uri.EscapeDataString(options.Sort)}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}

public class PricingClient : IPricingClient
{
    private readonly HetznerCloudClient _client;

    public PricingClient(HetznerCloudClient client)
    {
        _client = client;
    }

    public async Task<PricingListResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<PricingListResponse>("/pricing", cancellationToken);
    }
}