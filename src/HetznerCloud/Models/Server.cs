using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HetznerCloud.Pagination;

namespace HetznerCloud.Models;

public class Iso
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public IsoImageType Type { get; set; }

    [JsonPropertyName("deprecated")]
    public DateTime? Deprecated { get; set; }
}

public class Server
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public ServerStatus Status { get; set; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    [JsonPropertyName("public_net")]
    public PublicNetwork PublicNet { get; set; } = new();

    [JsonPropertyName("private_net")]
    public List<PrivateNetwork> PrivateNet { get; set; } = [];

    [JsonPropertyName("server_type")]
    public ServerType ServerType { get; set; } = new();

    [JsonPropertyName("image")]
    public Image Image { get; set; } = new();

    [JsonPropertyName("datacenter")]
    public Datacenter Datacenter { get; set; } = new();

    [JsonPropertyName("location")]
    public Location Location { get; set; } = new();

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; } = [];

    [JsonPropertyName("volumes")]
    public List<long> Volumes { get; set; } = [];

    [JsonPropertyName("primary_disk_size")]
    public long PrimaryDiskSize { get; set; }

    [JsonPropertyName("protection")]
    public ServerProtection Protection { get; set; } = new();

    [JsonPropertyName("rescue_enabled")]
    public bool RescueEnabled { get; set; }

    [JsonPropertyName("locked")]
    public bool Locked { get; set; }

    [JsonPropertyName("included_traffic")]
    public ulong IncludedTraffic { get; set; }

    [JsonPropertyName("outgoing_traffic")]
    public ulong OutgoingTraffic { get; set; }

    [JsonPropertyName("ingoing_traffic")]
    public ulong IngoingTraffic { get; set; }

    [JsonPropertyName("iso")]
    public Iso? Iso { get; set; }

    [JsonPropertyName("load_balancers")]
    public List<LoadBalancerReference> LoadBalancers { get; set; } = [];

    [JsonPropertyName("placement_group")]
    public PlacementGroup? PlacementGroup { get; set; }

    [JsonPropertyName("backup_window")]
    public string? BackupWindow { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];
}

public enum ServerStatus
{
    [JsonPropertyName("initializing")]
    Initializing,
    [JsonPropertyName("starting")]
    Starting,
    [JsonPropertyName("running")]
    Running,
    [JsonPropertyName("stopping")]
    Stopping,
    [JsonPropertyName("off")]
    Off,
    [JsonPropertyName("deleting")]
    Deleting,
    [JsonPropertyName("rebuilding")]
    Rebuilding,
    [JsonPropertyName("migrating")]
    Migrating,
    [JsonPropertyName("unknown")]
    Unknown
}

public class PublicNetwork
{
    [JsonPropertyName("ipv4")]
    public Ipv4Network Ipv4 { get; set; } = new();

    [JsonPropertyName("ipv6")]
    public Ipv6Network Ipv6 { get; set; } = new();

    [JsonPropertyName("floating_ips")]
    public List<long> FloatingIps { get; set; } = [];
}

public class Ipv4Network
{
    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("blocked")]
    public bool Blocked { get; set; }

    [JsonPropertyName("dns_ptr")]
    public List<DnsPtr> DnsPtr { get; set; } = [];
}

public class Ipv6Network
{
    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("blocked")]
    public bool Blocked { get; set; }

    [JsonPropertyName("dns_ptr")]
    public List<DnsPtr> DnsPtr { get; set; } = [];
}

public class PrivateNetwork
{
    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("network")]
    public long Network { get; set; }

    [JsonPropertyName("alias_ips")]
    public List<string> AliasIps { get; set; } = [];
}

public class ServerProtection
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("server_type")]
    public string ServerType { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;

    [JsonPropertyName("datacenter")]
    public string? Datacenter { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("ssh_keys")]
    public List<long>? SshKeys { get; set; }

    [JsonPropertyName("volumes")]
    public List<long>? Volumes { get; set; }

    [JsonPropertyName("networks")]
    public List<long>? Networks { get; set; }

    [JsonPropertyName("user_data")]
    public string? UserData { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }

    [JsonPropertyName("automount")]
    public bool? Automount { get; set; }

    [JsonPropertyName("placement_group")]
    public long? PlacementGroup { get; set; }

    [JsonPropertyName("public_net")]
    public PublicNetworkConfig? PublicNet { get; set; }

    [JsonPropertyName("start_after_create")]
    public bool StartAfterCreate { get; set; } = true;

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }
}

public class PublicNetworkConfig
{
    [JsonPropertyName("enable_ipv4")]
    public bool EnableIpv4 { get; set; } = true;

    [JsonPropertyName("enable_ipv6")]
    public bool EnableIpv6 { get; set; } = true;

    [JsonPropertyName("ipv4")]
    public long? Ipv4Id { get; set; }

    [JsonPropertyName("ipv6")]
    public long? Ipv6Id { get; set; }
}

public class ServerActionResponse
{
    [JsonPropertyName("action")]
    public Action Action { get; set; } = new();

    [JsonPropertyName("next_actions")]
    public List<Action> NextActions { get; set; } = [];

    [JsonPropertyName("server")]
    public Server? Server { get; set; }

    [JsonPropertyName("root_password")]
    public string? RootPassword { get; set; }
}

public class ServerActionRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("start_after_create")]
    public bool? StartAfterCreate { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("server_type")]
    public string? ServerType { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

public class ServerRebuildRequest : ServerActionRequest
{
    public ServerRebuildRequest()
    {
        Type = "rebuild";
    }

    [JsonPropertyName("user_data")]
    public string? UserData { get; set; }
}

public class ServerChangeTypeRequest : ServerActionRequest
{
    public ServerChangeTypeRequest()
    {
        Type = "change_type";
    }

    [JsonPropertyName("upgrade_disk")]
    public bool UpgradeDisk { get; set; } = true;
}

public class ServerChangeProtectionRequest : ServerActionRequest
{
    public ServerChangeProtectionRequest()
    {
        Type = "change_protection";
    }

    [JsonPropertyName("delete")]
    public bool? Delete { get; set; }

    [JsonPropertyName("rebuild")]
    public bool? Rebuild { get; set; }
}

public class ServerEnableBackupRequest : ServerActionRequest
{
    public ServerEnableBackupRequest()
    {
        Type = "enable_backup";
    }

    [JsonPropertyName("backup_window")]
    public string? BackupWindow { get; set; }
}

public class ServerDisableBackupRequest : ServerActionRequest
{
    public ServerDisableBackupRequest()
    {
        Type = "disable_backup";
    }
}

public class ServerChangeDnsPtrRequest : ServerActionRequest
{
    public ServerChangeDnsPtrRequest()
    {
        Type = "change_dns_ptr";
    }

    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("dns_ptr")]
    public string? DnsPtr { get; set; }
}

public class ServerAttachToNetworkRequest : ServerActionRequest
{
    public ServerAttachToNetworkRequest()
    {
        Type = "attach_to_network";
    }

    [JsonPropertyName("network")]
    public long Network { get; set; }

    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    [JsonPropertyName("alias_ips")]
    public List<string>? AliasIps { get; set; }
}

public class ServerDetachFromNetworkRequest : ServerActionRequest
{
    public ServerDetachFromNetworkRequest()
    {
        Type = "detach_from_network";
    }

    [JsonPropertyName("network")]
    public long Network { get; set; }
}

public class ServerCreateImageRequest : ServerActionRequest
{
    public ServerCreateImageRequest()
    {
        Type = "create_image";
    }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }
}

public class ServerReassignIpRequest : ServerActionRequest
{
    public ServerReassignIpRequest()
    {
        Type = "reassign_ip";
    }

    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;
}

public class ServerResetPasswordRequest : ServerActionRequest
{
    public ServerResetPasswordRequest()
    {
        Type = "reset_password";
    }
}

public class ServerEnableRescueRequest : ServerActionRequest
{
    public ServerEnableRescueRequest()
    {
        Type = "enable_rescue";
    }

    [JsonPropertyName("rescue_type")]
    public RescueType RescueType { get; set; } = RescueType.Linux64;

    [JsonPropertyName("ssh_keys")]
    public List<long>? SshKeys { get; set; }
}

public enum RescueType
{
    [JsonPropertyName("linux64")]
    Linux64,
    [JsonPropertyName("linux32")]
    Linux32,
    [JsonPropertyName("freebsd64")]
    FreeBSD64,
    [JsonPropertyName("freebsd32")]
    FreeBSD32,
    [JsonPropertyName("windows")]
    Windows
}

public class ServerDisableRescueRequest : ServerActionRequest
{
    public ServerDisableRescueRequest()
    {
        Type = "disable_rescue";
    }
}

public class ServerChangeAliasIpsRequest : ServerActionRequest
{
    public ServerChangeAliasIpsRequest()
    {
        Type = "change_alias_ips";
    }

    [JsonPropertyName("network")]
    public long Network { get; set; }

    [JsonPropertyName("old_ip")]
    public string OldIp { get; set; } = string.Empty;

    [JsonPropertyName("new_alias_ips")]
    public List<string> NewAliasIps { get; set; } = [];
}

public class ServerAttachIsoRequest : ServerActionRequest
{
    public ServerAttachIsoRequest()
    {
        Type = "attach_iso";
    }

    [JsonPropertyName("iso")]
    public long Iso { get; set; }
}

public class ServerDetachIsoRequest : ServerActionRequest
{
    public ServerDetachIsoRequest()
    {
        Type = "detach_iso";
    }
}

public class ServerUpdateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }
}

public class ServerCreateRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("server_type")]
    public string ServerType { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;

    [JsonPropertyName("datacenter")]
    public string? Datacenter { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("ssh_keys")]
    public List<long>? SshKeys { get; set; }

    [JsonPropertyName("volumes")]
    public List<long>? Volumes { get; set; }

    [JsonPropertyName("networks")]
    public List<long>? Networks { get; set; }

    [JsonPropertyName("user_data")]
    public string? UserData { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }

    [JsonPropertyName("automount")]
    public bool? Automount { get; set; }

    [JsonPropertyName("placement_group")]
    public long? PlacementGroup { get; set; }

    [JsonPropertyName("public_net")]
    public PublicNetworkConfig? PublicNet { get; set; }

    [JsonPropertyName("start_after_create")]
    public bool StartAfterCreate { get; set; } = true;

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }
}

public class ServerListResponse
{
    [JsonPropertyName("servers")]
    public List<Server> Servers { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class ServerResponse
{
    [JsonPropertyName("server")]
    public Server Server { get; set; } = new();
}

public class ServerCreateResponse
{
    [JsonPropertyName("action")]
    public Action Action { get; set; } = new();

    [JsonPropertyName("next_actions")]
    public List<Action> NextActions { get; set; } = [];

    [JsonPropertyName("server")]
    public Server? Server { get; set; }

    [JsonPropertyName("root_password")]
    public string? RootPassword { get; set; }
}