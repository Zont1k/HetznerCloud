using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HetznerCloud.Pagination;

namespace HetznerCloud.Models;

public class Network
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; } = [];

    [JsonPropertyName("ip_range")]
    public string IpRange { get; set; } = string.Empty;

    [JsonPropertyName("network_zone")]
    public string NetworkZone { get; set; } = string.Empty;

    [JsonPropertyName("subnets")]
    public List<NetworkSubnet> Subnets { get; set; } = [];

    [JsonPropertyName("routes")]
    public List<NetworkRoute> Routes { get; set; } = [];

    [JsonPropertyName("servers")]
    public List<ResourceReference> Servers { get; set; } = [];

    [JsonPropertyName("load_balancers")]
    public List<ResourceReference> LoadBalancers { get; set; } = [];

    [JsonPropertyName("protection")]
    public NetworkProtection Protection { get; set; } = new();

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    [JsonPropertyName("expose_routes_to_vswitch")]
    public bool ExposeRoutesToVswitch { get; set; }
}

public class NetworkSubnet
{
    [JsonPropertyName("type")]
    public NetworkSubnetType Type { get; set; }

    [JsonPropertyName("network_zone")]
    public string NetworkZone { get; set; } = string.Empty;

    [JsonPropertyName("vswitch_id")]
    public long? VswitchId { get; set; }

    [JsonPropertyName("ip_range")]
    public string IpRange { get; set; } = string.Empty;

    [JsonPropertyName("gateway")]
    public string? Gateway { get; set; }
}

public enum NetworkSubnetType
{
    [JsonPropertyName("cloud")]
    Cloud,
    [JsonPropertyName("vswitch")]
    VSwitch
}

public class NetworkRoute
{
    [JsonPropertyName("destination")]
    public string Destination { get; set; } = string.Empty;

    [JsonPropertyName("gateway")]
    public string Gateway { get; set; } = string.Empty;
}

public class NetworkProtection
{
    [JsonPropertyName("delete")]
    public bool Delete { get; set; }
}

public class NetworkListResponse
{
    [JsonPropertyName("networks")]
    public List<Network> Networks { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class NetworkResponse
{
    [JsonPropertyName("network")]
    public Network Network { get; set; } = new();
}

public class NetworkCreateRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("ip_range")]
    public string IpRange { get; set; } = string.Empty;

    [JsonPropertyName("network_zone")]
    public string? NetworkZone { get; set; }

    [JsonPropertyName("subnets")]
    public List<NetworkSubnetCreateRequest> Subnets { get; set; } = [];

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }

    [JsonPropertyName("expose_routes_to_vswitch")]
    public bool ExposeRoutesToVswitch { get; set; }
}

public class NetworkSubnetCreateRequest
{
    [JsonPropertyName("type")]
    public NetworkSubnetType Type { get; set; }

    [JsonPropertyName("network_zone")]
    public string? NetworkZone { get; set; }

    [JsonPropertyName("ip_range")]
    public string IpRange { get; set; } = string.Empty;

    [JsonPropertyName("vswitch_id")]
    public long? VswitchId { get; set; }
}

public class NetworkUpdateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

public class NetworkActionRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class NetworkAddSubnetRequest : NetworkActionRequest
{
    public NetworkAddSubnetRequest()
    {
        Type = "add_subnet";
    }

    [JsonPropertyName("subnet")]
    public NetworkSubnetCreateRequest Subnet { get; set; } = new();
}

public class NetworkDeleteSubnetRequest : NetworkActionRequest
{
    public NetworkDeleteSubnetRequest()
    {
        Type = "delete_subnet";
    }

    [JsonPropertyName("subnet")]
    public NetworkSubnetDeleteRequest Subnet { get; set; } = new();
}

public class NetworkSubnetDeleteRequest
{
    [JsonPropertyName("type")]
    public NetworkSubnetType Type { get; set; }

    [JsonPropertyName("network_zone")]
    public string NetworkZone { get; set; } = string.Empty;

    [JsonPropertyName("ip_range")]
    public string IpRange { get; set; } = string.Empty;

    [JsonPropertyName("vswitch_id")]
    public long? VswitchId { get; set; }
}

public class NetworkChangeIpRangeRequest : NetworkActionRequest
{
    public NetworkChangeIpRangeRequest()
    {
        Type = "change_ip_range";
    }

    [JsonPropertyName("ip_range")]
    public string IpRange { get; set; } = string.Empty;
}

public class NetworkChangeProtectionRequest : NetworkActionRequest
{
    public NetworkChangeProtectionRequest()
    {
        Type = "change_protection";
    }

    [JsonPropertyName("delete")]
    public bool Delete { get; set; }
}

public class NetworkAddRouteRequest : NetworkActionRequest
{
    public NetworkAddRouteRequest()
    {
        Type = "add_route";
    }

    [JsonPropertyName("route")]
    public NetworkRoute Route { get; set; } = new();
}

public class NetworkDeleteRouteRequest : NetworkActionRequest
{
    public NetworkDeleteRouteRequest()
    {
        Type = "delete_route";
    }

    [JsonPropertyName("route")]
    public NetworkRoute Route { get; set; } = new();
}

public class NetworkActionResponse
{
    [JsonPropertyName("actions")]
    public List<Action> Actions { get; set; } = [];

    [JsonPropertyName("network")]
    public Network? Network { get; set; }
}

public class FloatingIp
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("type")]
    public FloatingIpType Type { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("blocked")]
    public bool Blocked { get; set; }

    [JsonPropertyName("dns_ptr")]
    public List<DnsPtr> DnsPtr { get; set; } = [];

    [JsonPropertyName("protection")]
    public FloatingIpProtection Protection { get; set; } = new();

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; } = [];

    [JsonPropertyName("home_location")]
    public Location HomeLocation { get; set; } = new();

    [JsonPropertyName("server")]
    public ResourceReference? Server { get; set; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }
}

public enum FloatingIpType
{
    [JsonPropertyName("ipv4")]
    Ipv4,
    [JsonPropertyName("ipv6")]
    Ipv6
}

public class FloatingIpProtection
{
    [JsonPropertyName("delete")]
    public bool Delete { get; set; }
}

public class FloatingIpListResponse
{
    [JsonPropertyName("floating_ips")]
    public List<FloatingIp> FloatingIps { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class FloatingIpResponse
{
    [JsonPropertyName("floating_ip")]
    public FloatingIp FloatingIp { get; set; } = new();
}

public class FloatingIpCreateRequest
{
    [JsonPropertyName("type")]
    public FloatingIpType Type { get; set; } = FloatingIpType.Ipv4;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("home_location")]
    public string? HomeLocation { get; set; }

    [JsonPropertyName("server")]
    public long? Server { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

public class FloatingIpUpdateRequest
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

public class FloatingIpActionRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class FloatingIpAssignRequest : FloatingIpActionRequest
{
    public FloatingIpAssignRequest()
    {
        Type = "assign";
    }

    [JsonPropertyName("server")]
    public long Server { get; set; }
}

public class FloatingIpUnassignRequest : FloatingIpActionRequest
{
    public FloatingIpUnassignRequest()
    {
        Type = "unassign";
    }
}

public class FloatingIpChangeDnsPtrRequest : FloatingIpActionRequest
{
    public FloatingIpChangeDnsPtrRequest()
    {
        Type = "change_dns_ptr";
    }

    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("dns_ptr")]
    public string DnsPtr { get; set; } = string.Empty;
}

public class FloatingIpChangeProtectionRequest : FloatingIpActionRequest
{
    public FloatingIpChangeProtectionRequest()
    {
        Type = "change_protection";
    }

    [JsonPropertyName("delete")]
    public bool Delete { get; set; }
}

public class FloatingIpActionResponse
{
    [JsonPropertyName("action")]
    public Action Action { get; set; } = new();
}