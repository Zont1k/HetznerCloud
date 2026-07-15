using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HetznerCloud.Pagination;

namespace HetznerCloud.Models;

public class PrimaryIp
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public PrimaryIpType Type { get; set; }

    [JsonPropertyName("blocked")]
    public bool Blocked { get; set; }

    [JsonPropertyName("dns_ptr")]
    public List<DnsPtr> DnsPtr { get; set; } = [];

    [JsonPropertyName("protection")]
    public PrimaryIpProtection Protection { get; set; } = new();

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; } = [];

    [JsonPropertyName("assignee_type")]
    public string? AssigneeType { get; set; }

    [JsonPropertyName("assignee_id")]
    public long? AssigneeId { get; set; }

    [JsonPropertyName("home_location")]
    public Location HomeLocation { get; set; } = new();

    [JsonPropertyName("datacenter")]
    public string? Datacenter { get; set; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }
}

public enum PrimaryIpType
{
    [JsonPropertyName("ipv4")]
    Ipv4,
    [JsonPropertyName("ipv6")]
    Ipv6
}

public class PrimaryIpProtection
{
    [JsonPropertyName("delete")]
    public bool Delete { get; set; }
}

public class PrimaryIpListResponse
{
    [JsonPropertyName("primary_ips")]
    public List<PrimaryIp> PrimaryIps { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class PrimaryIpResponse
{
    [JsonPropertyName("primary_ip")]
    public PrimaryIp PrimaryIp { get; set; } = new();
}

public class PrimaryIpCreateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("type")]
    public PrimaryIpType Type { get; set; }

    [JsonPropertyName("assignee_type")]
    public string? AssigneeType { get; set; }

    [JsonPropertyName("assignee_id")]
    public long? AssigneeId { get; set; }

    [JsonPropertyName("auto_delete")]
    public bool? AutoDelete { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }

    [JsonPropertyName("home_location")]
    public string? HomeLocation { get; set; }

    [JsonPropertyName("datacenter")]
    public string? Datacenter { get; set; }
}

public class PrimaryIpUpdateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }

    [JsonPropertyName("auto_delete")]
    public bool? AutoDelete { get; set; }
}

public class PrimaryIpAssignRequest
{
    [JsonPropertyName("assignee_type")]
    public string AssigneeType { get; set; } = "server";

    [JsonPropertyName("assignee_id")]
    public long AssigneeId { get; set; }

    [JsonPropertyName("allow_delete")]
    public bool? AllowDelete { get; set; }
}

public class PrimaryIpUnassignRequest
{
    [JsonPropertyName("allow_delete")]
    public bool? AllowDelete { get; set; }
}

public class PrimaryIpChangeDnsPtrRequest
{
    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("dns_ptr")]
    public string? DnsPtr { get; set; }
}

public class PrimaryIpChangeProtectionRequest
{
    [JsonPropertyName("delete")]
    public bool Delete { get; set; }
}

public class PrimaryIpActionResponse
{
    [JsonPropertyName("action")]
    public Action Action { get; set; } = new();
}
