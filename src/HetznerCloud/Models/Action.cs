using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HetznerCloud.Pagination;

namespace HetznerCloud.Models;

public class Action
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public ActionStatus Status { get; set; }

    [JsonPropertyName("progress")]
    public int Progress { get; set; }

    [JsonPropertyName("started")]
    public DateTime Started { get; set; }

    [JsonPropertyName("finished")]
    public DateTime? Finished { get; set; }

    [JsonPropertyName("resources")]
    public List<ActionResource> Resources { get; set; } = [];

    [JsonPropertyName("error")]
    public ActionError? Error { get; set; }
}

public enum ActionStatus
{
    [JsonPropertyName("running")]
    Running,
    [JsonPropertyName("success")]
    Success,
    [JsonPropertyName("error")]
    Error
}

public class ActionResource
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class ActionError
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public class ActionListResponse
{
    [JsonPropertyName("actions")]
    public List<Action> Actions { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class ActionResponse
{
    [JsonPropertyName("action")]
    public Action Action { get; set; } = new();
}

public class ActionListOptions
{
    public int Page { get; set; } = 1;
    public int PerPage { get; set; } = 25;
    public string? Sort { get; set; }
    public string? Status { get; set; }
    public long? ResourceId { get; set; }
    public string? ResourceType { get; set; }
    public DateTime? StartedAfter { get; set; }
    public DateTime? StartedBefore { get; set; }
}

public class SshKey
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("fingerprint")]
    public string Fingerprint { get; set; } = string.Empty;

    [JsonPropertyName("public_key")]
    public string PublicKey { get; set; } = string.Empty;

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; } = [];

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }
}

public class SshKeyListResponse
{
    [JsonPropertyName("ssh_keys")]
    public List<SshKey> SshKeys { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class SshKeyResponse
{
    [JsonPropertyName("ssh_key")]
    public SshKey SshKey { get; set; } = new();
}

public class SshKeyCreateRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("public_key")]
    public string PublicKey { get; set; } = string.Empty;

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

public class SshKeyUpdateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

public class Certificate
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; } = [];

    [JsonPropertyName("type")]
    public CertificateType Type { get; set; }

    [JsonPropertyName("certificate")]
    public string CertificateData { get; set; } = string.Empty;

    [JsonPropertyName("domain_names")]
    public List<string> DomainNames { get; set; } = [];

    [JsonPropertyName("fingerprint")]
    public string Fingerprint { get; set; } = string.Empty;

    [JsonPropertyName("issued")]
    public DateTime Issued { get; set; }

    [JsonPropertyName("not_valid_before")]
    public DateTime NotValidBefore { get; set; }

    [JsonPropertyName("not_valid_after")]
    public DateTime NotValidAfter { get; set; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }
}

public enum CertificateType
{
    [JsonPropertyName("uploaded")]
    Uploaded,
    [JsonPropertyName("managed")]
    Managed
}

public class CertificateListResponse
{
    [JsonPropertyName("certificates")]
    public List<Certificate> Certificates { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class CertificateResponse
{
    [JsonPropertyName("certificate")]
    public Certificate Certificate { get; set; } = new();
}

public class CertificateCreateRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public CertificateType Type { get; set; }

    [JsonPropertyName("certificate")]
    public string? Certificate { get; set; }

    [JsonPropertyName("private_key")]
    public string? PrivateKey { get; set; }

    [JsonPropertyName("domain_names")]
    public List<string>? DomainNames { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

public class CertificateUpdateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

public class PlacementGroup
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; } = [];

    [JsonPropertyName("type")]
    public PlacementGroupType Type { get; set; }

    [JsonPropertyName("servers")]
    public List<ResourceReference> Servers { get; set; } = [];

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }
}

public enum PlacementGroupType
{
    [JsonPropertyName("spread")]
    Spread
}

public class PlacementGroupListResponse
{
    [JsonPropertyName("placement_groups")]
    public List<PlacementGroup> PlacementGroups { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class PlacementGroupResponse
{
    [JsonPropertyName("placement_group")]
    public PlacementGroup PlacementGroup { get; set; } = new();
}

public class PlacementGroupCreateRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public PlacementGroupType Type { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

public class PlacementGroupUpdateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

public class Firewall
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; } = [];

    [JsonPropertyName("rules")]
    public List<FirewallRule> Rules { get; set; } = [];

    [JsonPropertyName("applied_to")]
    public List<FirewallAppliedTo> AppliedTo { get; set; } = [];

    [JsonPropertyName("protection")]
    public FirewallProtection Protection { get; set; } = new();

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }
}

public class FirewallProtection
{
    [JsonPropertyName("change")]
    public bool Change { get; set; }

    [JsonPropertyName("delete")]
    public bool Delete { get; set; }
}

public class FirewallRule
{
    [JsonPropertyName("direction")]
    public FirewallRuleDirection Direction { get; set; }

    [JsonPropertyName("protocol")]
    public FirewallRuleProtocol Protocol { get; set; }

    [JsonPropertyName("port")]
    public string? Port { get; set; }

    [JsonPropertyName("source_ips")]
    public List<string> SourceIps { get; set; } = [];

    [JsonPropertyName("destination_ips")]
    public List<string> DestinationIps { get; set; } = [];

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public enum FirewallRuleDirection
{
    [JsonPropertyName("in")]
    In,
    [JsonPropertyName("out")]
    Out
}

public enum FirewallRuleProtocol
{
    [JsonPropertyName("tcp")]
    Tcp,
    [JsonPropertyName("udp")]
    Udp,
    [JsonPropertyName("icmp")]
    Icmp,
    [JsonPropertyName("gre")]
    Gre,
    [JsonPropertyName("esp")]
    Esp
}

public class FirewallAppliedTo
{
    [JsonPropertyName("type")]
    public FirewallAppliedToType Type { get; set; }

    [JsonPropertyName("server")]
    public ResourceReference? Server { get; set; }

    [JsonPropertyName("label_selector")]
    public ResourceReference? LabelSelector { get; set; }
}

public enum FirewallAppliedToType
{
    [JsonPropertyName("server")]
    Server,
    [JsonPropertyName("label_selector")]
    LabelSelector
}

public class FirewallListResponse
{
    [JsonPropertyName("firewalls")]
    public List<Firewall> Firewalls { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class FirewallResponse
{
    [JsonPropertyName("firewall")]
    public Firewall Firewall { get; set; } = new();
}

public class FirewallCreateRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("rules")]
    public List<FirewallRule> Rules { get; set; } = [];

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }

    [JsonPropertyName("apply_to")]
    public List<FirewallApplyToRequest>? ApplyTo { get; set; }
}

public class FirewallApplyToRequest
{
    [JsonPropertyName("type")]
    public FirewallAppliedToType Type { get; set; }

    [JsonPropertyName("server")]
    public long? Server { get; set; }

    [JsonPropertyName("label_selector")]
    public FirewallLabelSelectorRequest? LabelSelector { get; set; }
}

public class FirewallLabelSelectorRequest
{
    [JsonPropertyName("selector")]
    public string Selector { get; set; } = string.Empty;
}

public class FirewallUpdateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

public class FirewallSetRulesRequest
{
    [JsonPropertyName("rules")]
    public List<FirewallRule> Rules { get; set; } = [];
}

public class FirewallApplyToResourcesRequest
{
    [JsonPropertyName("apply_to")]
    public List<FirewallApplyToRequest> ApplyTo { get; set; } = [];
}

public class FirewallRemoveFromResourcesRequest
{
    [JsonPropertyName("remove_from")]
    public List<FirewallApplyToRequest> RemoveFrom { get; set; } = [];
}

public class FirewallActionResponse
{
    [JsonPropertyName("actions")]
    public List<Action> Actions { get; set; } = [];
}

public class FirewallChangeProtectionRequest
{
    [JsonPropertyName("change")]
    public bool? Change { get; set; }

    [JsonPropertyName("delete")]
    public bool? Delete { get; set; }
}

public class IsoImage
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

public enum IsoImageType
{
    [JsonPropertyName("public")]
    Public,
    [JsonPropertyName("private")]
    Private
}

public class IsoImageResponse
{
    [JsonPropertyName("iso")]
    public IsoImage Iso { get; set; } = new();
}

public class IsoImageListResponse
{
    [JsonPropertyName("isos")]
    public List<IsoImage> Isos { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class ResourceReference
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class LoadBalancerReference
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class ServerReference
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}