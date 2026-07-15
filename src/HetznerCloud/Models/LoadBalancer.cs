using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HetznerCloud.Pagination;

namespace HetznerCloud.Models;

public class LoadBalancer
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; } = [];

    [JsonPropertyName("load_balancer_type")]
    public LoadBalancerType LoadBalancerType { get; set; } = new();

    [JsonPropertyName("location")]
    public Location Location { get; set; } = new();

    [JsonPropertyName("network_zone")]
    public string NetworkZone { get; set; } = string.Empty;

    [JsonPropertyName("network")]
    public ResourceReference? Network { get; set; }

    [JsonPropertyName("public_net")]
    public LoadBalancerPublicNet PublicNet { get; set; } = new();

    [JsonPropertyName("private_net")]
    public List<LoadBalancerPrivateNet> PrivateNet { get; set; } = [];

    [JsonPropertyName("protection")]
    public LoadBalancerProtection Protection { get; set; } = new();

    [JsonPropertyName("algorithm")]
    public LoadBalancerAlgorithm Algorithm { get; set; } = new();

    [JsonPropertyName("targets")]
    public List<LoadBalancerTarget> Targets { get; set; } = [];

    [JsonPropertyName("services")]
    public List<LoadBalancerService> Services { get; set; } = [];

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }
}

public class LoadBalancerPublicNet
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("ipv4")]
    public Ipv4Address? Ipv4 { get; set; }

    [JsonPropertyName("ipv6")]
    public Ipv6Address? Ipv6 { get; set; }
}

public class Ipv4Address
{
    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("blocked")]
    public bool Blocked { get; set; }

    [JsonPropertyName("dns_ptr")]
    public List<DnsPtr> DnsPtr { get; set; } = [];
}

public class Ipv6Address
{
    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("blocked")]
    public bool Blocked { get; set; }

    [JsonPropertyName("dns_ptr")]
    public List<DnsPtr> DnsPtr { get; set; } = [];
}

public class DnsPtr
{
    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("dns_ptr")]
    public string DnsPtrValue { get; set; } = string.Empty;
}

public class LoadBalancerPrivateNet
{
    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("network")]
    public long Network { get; set; }
}

public class LoadBalancerProtection
{
    [JsonPropertyName("delete")]
    public bool Delete { get; set; }
}

public class LoadBalancerAlgorithm
{
    [JsonPropertyName("type")]
    public LoadBalancerAlgorithmType Type { get; set; }
}

public enum LoadBalancerAlgorithmType
{
    [JsonPropertyName("round_robin")]
    RoundRobin,
    [JsonPropertyName("least_connections")]
    LeastConnections
}

public class LoadBalancerTarget
{
    [JsonPropertyName("type")]
    public LoadBalancerTargetType Type { get; set; }

    [JsonPropertyName("server")]
    public ResourceReference? Server { get; set; }

    [JsonPropertyName("label_selector")]
    public ResourceReference? LabelSelector { get; set; }

    [JsonPropertyName("ip")]
    public ResourceReference? Ip { get; set; }

    [JsonPropertyName("use_private_ip")]
    public bool UsePrivateIp { get; set; }
}

public enum LoadBalancerTargetType
{
    [JsonPropertyName("server")]
    Server,
    [JsonPropertyName("label_selector")]
    LabelSelector,
    [JsonPropertyName("ip")]
    Ip
}

public class LoadBalancerService
{
    [JsonPropertyName("protocol")]
    public LoadBalancerServiceProtocol Protocol { get; set; }

    [JsonPropertyName("listen_port")]
    public int ListenPort { get; set; }

    [JsonPropertyName("destination_port")]
    public int DestinationPort { get; set; }

    [JsonPropertyName("http")]
    public LoadBalancerServiceHttp? Http { get; set; }

    [JsonPropertyName("proxyprotocol")]
    public LoadBalancerProxyProtocolOptions? ProxyProtocol { get; set; }

    [JsonPropertyName("health_check")]
    public LoadBalancerHealthCheck HealthCheck { get; set; } = new();
}

public enum LoadBalancerServiceProtocol
{
    [JsonPropertyName("http")]
    Http,
    [JsonPropertyName("https")]
    Https,
    [JsonPropertyName("tcp")]
    Tcp
}

public class LoadBalancerServiceHttp
{
    [JsonPropertyName("certificates")]
    public List<long> Certificates { get; set; } = [];

    [JsonPropertyName("redirect_http")]
    public LoadBalancerServiceHttpRedirectHttp? RedirectHttp { get; set; }

    [JsonPropertyName("sticky_sessions")]
    public LoadBalancerServiceHttpStickySessions? StickySessions { get; set; }

    [JsonPropertyName("cookie_name")]
    public string? CookieName { get; set; }

    [JsonPropertyName("cookie_lifetime")]
    public int? CookieLifetime { get; set; }

    [JsonPropertyName("idle_timeout")]
    public int? IdleTimeout { get; set; }
}

public class LoadBalancerServiceHttpRedirectHttp
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; }
}

public class LoadBalancerServiceHttpStickySessions
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("cookie_name")]
    public string? CookieName { get; set; }
}

public class LoadBalancerProxyProtocolOptions
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("version")]
    public int Version { get; set; }
}

public class LoadBalancerHealthCheck
{
    [JsonPropertyName("protocol")]
    public LoadBalancerHealthCheckProtocol Protocol { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("interval")]
    public int Interval { get; set; }

    [JsonPropertyName("timeout")]
    public int Timeout { get; set; }

    [JsonPropertyName("retries")]
    public int Retries { get; set; }

    [JsonPropertyName("http")]
    public LoadBalancerHealthCheckHttp? Http { get; set; }
}

public enum LoadBalancerHealthCheckProtocol
{
    [JsonPropertyName("http")]
    Http,
    [JsonPropertyName("tcp")]
    Tcp
}

public class LoadBalancerHealthCheckHttp
{
    [JsonPropertyName("domain")]
    public string? Domain { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("response")]
    public LoadBalancerHealthCheckHttpResponse? Response { get; set; }

    [JsonPropertyName("tls")]
    public bool Tls { get; set; }
}

public class LoadBalancerHealthCheckHttpResponse
{
    [JsonPropertyName("status_codes")]
    public List<string> StatusCodes { get; set; } = [];
}

public class LoadBalancerType
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("max_connections")]
    public int MaxConnections { get; set; }

    [JsonPropertyName("max_services")]
    public int MaxServices { get; set; }

    [JsonPropertyName("max_targets")]
    public int MaxTargets { get; set; }

    [JsonPropertyName("max_assigned_certificates")]
    public int MaxAssignedCertificates { get; set; }

    [JsonPropertyName("prices")]
    public List<Price> Prices { get; set; } = [];
}

public class LoadBalancerListResponse
{
    [JsonPropertyName("load_balancers")]
    public List<LoadBalancer> LoadBalancers { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class LoadBalancerResponse
{
    [JsonPropertyName("load_balancer")]
    public LoadBalancer LoadBalancer { get; set; } = new();
}

public class LoadBalancerTypeListResponse
{
    [JsonPropertyName("load_balancer_types")]
    public List<LoadBalancerType> LoadBalancerTypes { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class LoadBalancerTypeResponse
{
    [JsonPropertyName("load_balancer_type")]
    public LoadBalancerType LoadBalancerType { get; set; } = new();
}

public class LoadBalancerCreateRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("load_balancer_type")]
    public string LoadBalancerType { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("network_zone")]
    public string? NetworkZone { get; set; }

    [JsonPropertyName("network")]
    public long? Network { get; set; }

    [JsonPropertyName("ipv4")]
    public IpBlockRequest? Ipv4 { get; set; }

    [JsonPropertyName("ipv6")]
    public IpBlockRequest? Ipv6 { get; set; }

    [JsonPropertyName("algorithm")]
    public LoadBalancerAlgorithm Algorithm { get; set; } = new();

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }

    [JsonPropertyName("services")]
    public List<LoadBalancerServiceCreateRequest> Services { get; set; } = [];

    [JsonPropertyName("targets")]
    public List<LoadBalancerTargetCreateRequest> Targets { get; set; } = [];

    [JsonPropertyName("public_interface")]
    public bool PublicInterface { get; set; } = true;

    [JsonPropertyName("disable_private_network")]
    public bool DisablePrivateNetwork { get; set; }
}

public class IpBlockRequest
{
    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;
}

public class LoadBalancerServiceCreateRequest
{
    [JsonPropertyName("protocol")]
    public LoadBalancerServiceProtocol Protocol { get; set; }

    [JsonPropertyName("listen_port")]
    public int ListenPort { get; set; }

    [JsonPropertyName("destination_port")]
    public int DestinationPort { get; set; }

    [JsonPropertyName("http")]
    public LoadBalancerServiceHttpCreateRequest? Http { get; set; }

    [JsonPropertyName("health_check")]
    public LoadBalancerHealthCheckCreateRequest HealthCheck { get; set; } = new();
}

public class LoadBalancerServiceHttpCreateRequest
{
    [JsonPropertyName("certificates")]
    public List<long> Certificates { get; set; } = [];

    [JsonPropertyName("redirect_http")]
    public LoadBalancerServiceHttpRedirectHttpCreateRequest? RedirectHttp { get; set; }

    [JsonPropertyName("sticky_sessions")]
    public LoadBalancerServiceHttpStickySessionsCreateRequest? StickySessions { get; set; }

    [JsonPropertyName("idle_timeout")]
    public int? IdleTimeout { get; set; }
}

public class LoadBalancerServiceHttpRedirectHttpCreateRequest
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; }
}

public class LoadBalancerServiceHttpStickySessionsCreateRequest
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("cookie_name")]
    public string? CookieName { get; set; }
}

public class LoadBalancerHealthCheckCreateRequest
{
    [JsonPropertyName("protocol")]
    public LoadBalancerHealthCheckProtocol Protocol { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("interval")]
    public int Interval { get; set; }

    [JsonPropertyName("timeout")]
    public int Timeout { get; set; }

    [JsonPropertyName("retries")]
    public int Retries { get; set; }

    [JsonPropertyName("http")]
    public LoadBalancerHealthCheckHttpCreateRequest? Http { get; set; }
}

public class LoadBalancerHealthCheckHttpCreateRequest
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("domain")]
    public string? Domain { get; set; }

    [JsonPropertyName("response")]
    public LoadBalancerHealthCheckHttpResponseCreateRequest? Response { get; set; }

    [JsonPropertyName("tls")]
    public bool Tls { get; set; }
}

public class LoadBalancerHealthCheckHttpResponseCreateRequest
{
    [JsonPropertyName("status_codes")]
    public List<string> StatusCodes { get; set; } = [];
}

public class LoadBalancerTargetCreateRequest
{
    [JsonPropertyName("type")]
    public LoadBalancerTargetType Type { get; set; }

    [JsonPropertyName("server")]
    public long? Server { get; set; }

    [JsonPropertyName("label_selector")]
    public LoadBalancerTargetLabelSelectorCreateRequest? LabelSelector { get; set; }

    [JsonPropertyName("ip")]
    public LoadBalancerTargetIpCreateRequest? Ip { get; set; }

    [JsonPropertyName("use_private_ip")]
    public bool UsePrivateIp { get; set; }
}

public class LoadBalancerTargetLabelSelectorCreateRequest
{
    [JsonPropertyName("selector")]
    public string Selector { get; set; } = string.Empty;
}

public class LoadBalancerTargetIpCreateRequest
{
    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;
}

public class LoadBalancerAlgorithmCreateRequest
{
    [JsonPropertyName("type")]
    public LoadBalancerAlgorithmType Type { get; set; }
}

public class LoadBalancerUpdateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

public class LoadBalancerActionRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class LoadBalancerAddServiceRequest : LoadBalancerActionRequest
{
    public LoadBalancerAddServiceRequest()
    {
        Type = "add_service";
    }

    [JsonPropertyName("service")]
    public LoadBalancerServiceCreateRequest Service { get; set; } = new();
}

public class LoadBalancerDeleteServiceRequest : LoadBalancerActionRequest
{
    public LoadBalancerDeleteServiceRequest()
    {
        Type = "delete_service";
    }

    [JsonPropertyName("service")]
    public LoadBalancerServiceReference Service { get; set; } = new();
}

public class LoadBalancerServiceReference
{
    [JsonPropertyName("listen_port")]
    public int ListenPort { get; set; }

    [JsonPropertyName("protocol")]
    public LoadBalancerServiceProtocol Protocol { get; set; }
}

public class LoadBalancerUpdateServiceRequest : LoadBalancerActionRequest
{
    public LoadBalancerUpdateServiceRequest()
    {
        Type = "update_service";
    }

    [JsonPropertyName("service")]
    public LoadBalancerServiceReference Service { get; set; } = new();

    [JsonPropertyName("new_service")]
    public LoadBalancerServiceCreateRequest NewService { get; set; } = new();
}

public class LoadBalancerAddTargetRequest : LoadBalancerActionRequest
{
    public LoadBalancerAddTargetRequest()
    {
        Type = "add_target";
    }

    [JsonPropertyName("target")]
    public LoadBalancerTargetCreateRequest Target { get; set; } = new();
}

public class LoadBalancerRemoveTargetRequest : LoadBalancerActionRequest
{
    public LoadBalancerRemoveTargetRequest()
    {
        Type = "remove_target";
    }

    [JsonPropertyName("target")]
    public LoadBalancerTargetRemoveRequest Target { get; set; } = new();
}

public class LoadBalancerTargetRemoveRequest
{
    [JsonPropertyName("type")]
    public LoadBalancerTargetType Type { get; set; }

    [JsonPropertyName("server")]
    public long? Server { get; set; }

    [JsonPropertyName("label_selector")]
    public LoadBalancerTargetLabelSelectorCreateRequest? LabelSelector { get; set; }

    [JsonPropertyName("ip")]
    public LoadBalancerTargetIpCreateRequest? Ip { get; set; }
}

public class LoadBalancerChangeAlgorithmRequest : LoadBalancerActionRequest
{
    public LoadBalancerChangeAlgorithmRequest()
    {
        Type = "change_algorithm";
    }

    [JsonPropertyName("algorithm")]
    public LoadBalancerAlgorithmCreateRequest Algorithm { get; set; } = new();
}

public class LoadBalancerChangeTypeRequest : LoadBalancerActionRequest
{
    public LoadBalancerChangeTypeRequest()
    {
        Type = "change_type";
    }

    [JsonPropertyName("load_balancer_type")]
    public string LoadBalancerType { get; set; } = string.Empty;
}

public class LoadBalancerChangeProtectionRequest : LoadBalancerActionRequest
{
    public LoadBalancerChangeProtectionRequest()
    {
        Type = "change_protection";
    }

    [JsonPropertyName("delete")]
    public bool Delete { get; set; }
}

public class LoadBalancerEnablePublicInterfaceRequest : LoadBalancerActionRequest
{
    public LoadBalancerEnablePublicInterfaceRequest()
    {
        Type = "enable_public_interface";
    }

    [JsonPropertyName("ipv4")]
    public long? Ipv4 { get; set; }

    [JsonPropertyName("ipv6")]
    public long? Ipv6 { get; set; }
}

public class LoadBalancerDisablePublicInterfaceRequest : LoadBalancerActionRequest
{
    public LoadBalancerDisablePublicInterfaceRequest()
    {
        Type = "disable_public_interface";
    }
}

public class LoadBalancerAttachToNetworkRequest : LoadBalancerActionRequest
{
    public LoadBalancerAttachToNetworkRequest()
    {
        Type = "attach_to_network";
    }

    [JsonPropertyName("network")]
    public long Network { get; set; }

    [JsonPropertyName("ip")]
    public string? Ip { get; set; }
}

public class LoadBalancerDetachFromNetworkRequest : LoadBalancerActionRequest
{
    public LoadBalancerDetachFromNetworkRequest()
    {
        Type = "detach_from_network";
    }

    [JsonPropertyName("network")]
    public long Network { get; set; }
}

public class LoadBalancerChangeIpRequest : LoadBalancerActionRequest
{
    public LoadBalancerChangeIpRequest()
    {
        Type = "change_ip";
    }

    [JsonPropertyName("ipv4")]
    public IpBlockRequest? Ipv4 { get; set; }

    [JsonPropertyName("ipv6")]
    public IpBlockRequest? Ipv6 { get; set; }
}

public class LoadBalancerActionResponse
{
    [JsonPropertyName("actions")]
    public List<Action> Actions { get; set; } = [];

    [JsonPropertyName("load_balancer")]
    public LoadBalancer? LoadBalancer { get; set; }
}