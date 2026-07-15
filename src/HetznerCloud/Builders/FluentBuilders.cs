using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using HetznerCloud.Core;
using HetznerCloud.Models;

namespace HetznerCloud.Builders;

/// <summary>
/// Fluent builder for creating servers with compile-time validation
/// </summary>
public sealed class ServerCreateBuilder
{
    private string _name = string.Empty;
    private string _serverType = string.Empty;
    private string _image = string.Empty;
    private string? _location;
    private string? _datacenter;
    private readonly List<long> _sshKeys = [];
    private readonly List<long> _volumes = [];
    private readonly List<long> _networks = [];
    private string? _userData;
    private readonly Dictionary<string, string> _labels = [];
    private bool? _automount;
    private long? _placementGroup;
    private PublicNetworkConfigBuilder? _publicNet;
    private bool _startAfterCreate = true;
    private readonly List<string> _tags = [];

    public ServerCreateBuilder WithName(string name)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        return this;
    }

    public ServerCreateBuilder WithServerType(string serverType)
    {
        _serverType = serverType ?? throw new ArgumentNullException(nameof(serverType));
        return this;
    }

    public ServerCreateBuilder WithServerType(ServerTypeId serverTypeId)
    {
        _serverType = serverTypeId.Value.ToString();
        return this;
    }

    public ServerCreateBuilder WithImage(string image)
    {
        _image = image ?? throw new ArgumentNullException(nameof(image));
        return this;
    }

    public ServerCreateBuilder WithImage(ImageId imageId)
    {
        _image = imageId.Value.ToString();
        return this;
    }

    public ServerCreateBuilder WithLocation(string location)
    {
        _location = location;
        _datacenter = null; // Mutually exclusive
        return this;
    }

    public ServerCreateBuilder WithLocation(LocationId locationId)
    {
        _location = locationId.Value.ToString();
        _datacenter = null;
        return this;
    }

    public ServerCreateBuilder WithDatacenter(string datacenter)
    {
        _datacenter = datacenter;
        _location = null; // Mutually exclusive
        return this;
    }

    public ServerCreateBuilder WithDatacenter(DatacenterId datacenterId)
    {
        _datacenter = datacenterId.Value.ToString();
        _location = null;
        return this;
    }

    public ServerCreateBuilder AddSshKey(long sshKeyId)
    {
        _sshKeys.Add(sshKeyId);
        return this;
    }

    public ServerCreateBuilder AddSshKey(SshKeyId sshKeyId)
    {
        _sshKeys.Add(sshKeyId.Value);
        return this;
    }

    public ServerCreateBuilder WithSshKeys(params long[] sshKeyIds)
    {
        _sshKeys.AddRange(sshKeyIds);
        return this;
    }

    public ServerCreateBuilder WithSshKeys(params SshKeyId[] sshKeyIds)
    {
        _sshKeys.AddRange(sshKeyIds.Select(k => k.Value));
        return this;
    }

    public ServerCreateBuilder AddVolume(long volumeId)
    {
        _volumes.Add(volumeId);
        return this;
    }

    public ServerCreateBuilder AddVolume(VolumeId volumeId)
    {
        _volumes.Add(volumeId.Value);
        return this;
    }

    public ServerCreateBuilder WithVolumes(params long[] volumeIds)
    {
        _volumes.AddRange(volumeIds);
        return this;
    }

    public ServerCreateBuilder AddNetwork(long networkId)
    {
        _networks.Add(networkId);
        return this;
    }

    public ServerCreateBuilder AddNetwork(NetworkId networkId)
    {
        _networks.Add(networkId.Value);
        return this;
    }

    public ServerCreateBuilder WithNetworks(params long[] networkIds)
    {
        _networks.AddRange(networkIds);
        return this;
    }

    public ServerCreateBuilder WithUserData(string userData)
    {
        _userData = userData;
        return this;
    }

    public ServerCreateBuilder AddLabel(string key, string value)
    {
        _labels[key] = value;
        return this;
    }

    public ServerCreateBuilder WithLabels(Dictionary<string, string> labels)
    {
        foreach (var kvp in labels) _labels[kvp.Key] = kvp.Value;
        return this;
    }

    public ServerCreateBuilder WithAutomount(bool automount)
    {
        _automount = automount;
        return this;
    }

    public ServerCreateBuilder WithPlacementGroup(long placementGroupId)
    {
        _placementGroup = placementGroupId;
        return this;
    }

    public ServerCreateBuilder WithPlacementGroup(PlacementGroupId placementGroupId)
    {
        _placementGroup = placementGroupId.Value;
        return this;
    }

    public ServerCreateBuilder WithPublicNetwork(Action<PublicNetworkConfigBuilder> configure)
    {
        var builder = new PublicNetworkConfigBuilder();
        configure(builder);
        _publicNet = builder;
        return this;
    }

    public ServerCreateBuilder WithStartAfterCreate(bool startAfterCreate)
    {
        _startAfterCreate = startAfterCreate;
        return this;
    }

    public ServerCreateBuilder AddTag(string tag)
    {
        _tags.Add(tag);
        return this;
    }

    public ServerCreateBuilder WithTags(params string[] tags)
    {
        _tags.AddRange(tags);
        return this;
    }

    public ServerCreateRequest Build()
    {
        Validate();

        return new ServerCreateRequest
        {
            Name = _name,
            ServerType = _serverType,
            Image = _image,
            Location = _location,
            Datacenter = _datacenter,
            SshKeys = _sshKeys.Count > 0 ? _sshKeys : null,
            Volumes = _volumes.Count > 0 ? _volumes : null,
            Networks = _networks.Count > 0 ? _networks : null,
            UserData = _userData,
            Labels = _labels.Count > 0 ? _labels : null,
            Automount = _automount,
            PlacementGroup = _placementGroup,
            PublicNet = _publicNet?.Build(),
            StartAfterCreate = _startAfterCreate,
            Tags = _tags.Count > 0 ? _tags : null
        };
    }

    public static implicit operator ServerCreateRequest(ServerCreateBuilder builder) => builder.Build();

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_name))
            throw new InvalidOperationException("Server name is required");
        if (string.IsNullOrWhiteSpace(_serverType))
            throw new InvalidOperationException("Server type is required");
        if (string.IsNullOrWhiteSpace(_image))
            throw new InvalidOperationException("Image is required");
        if (_location is null && _datacenter is null)
            throw new InvalidOperationException("Either Location or Datacenter is required");
    }
}

/// <summary>
/// Builder for public network configuration
/// </summary>
public sealed class PublicNetworkConfigBuilder
{
    private bool _enableIpv4 = true;
    private bool _enableIpv6 = true;
    private long? _ipv4Id;
    private long? _ipv6Id;

    public PublicNetworkConfigBuilder EnableIpv4(bool enable = true)
    {
        _enableIpv4 = enable;
        return this;
    }

    public PublicNetworkConfigBuilder EnableIpv6(bool enable = true)
    {
        _enableIpv6 = enable;
        return this;
    }

    public PublicNetworkConfigBuilder WithIpv4(long ipv4Id)
    {
        _ipv4Id = ipv4Id;
        return this;
    }

    public PublicNetworkConfigBuilder WithIpv4(FloatingIpId ipv4Id)
    {
        _ipv4Id = ipv4Id.Value;
        return this;
    }

    public PublicNetworkConfigBuilder WithIpv6(long ipv6Id)
    {
        _ipv6Id = ipv6Id;
        return this;
    }

    public PublicNetworkConfigBuilder WithIpv6(FloatingIpId ipv6Id)
    {
        _ipv6Id = ipv6Id.Value;
        return this;
    }

internal PublicNetworkConfig Build()
        {
            return new PublicNetworkConfig
            {
                EnableIpv4 = _enableIpv4,
                EnableIpv6 = _enableIpv6,
                Ipv4Id = _ipv4Id,
                Ipv6Id = _ipv6Id
            };
        }
    }

/// <summary>
/// Builder for creating volumes
/// </summary>
public sealed class VolumeCreateBuilder
{
    private string _name = string.Empty;
    private int _size;
    private string? _location;
    private long? _server;
    private string? _format;
    private bool _automount;
    private readonly Dictionary<string, string> _labels = [];

    public VolumeCreateBuilder WithName(string name)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        return this;
    }

    public VolumeCreateBuilder WithSize(int sizeGb)
    {
        if (sizeGb < 10 || sizeGb > 10000)
            throw new ArgumentOutOfRangeException(nameof(sizeGb), "Size must be between 10 and 10000 GB");
        _size = sizeGb;
        return this;
    }

    public VolumeCreateBuilder WithLocation(string location)
    {
        _location = location;
        return this;
    }

    public VolumeCreateBuilder WithLocation(LocationId locationId)
    {
        _location = locationId.Value.ToString();
        return this;
    }

    public VolumeCreateBuilder WithServer(long serverId)
    {
        _server = serverId;
        return this;
    }

    public VolumeCreateBuilder WithServer(ServerId serverId)
    {
        _server = serverId.Value;
        return this;
    }

    public VolumeCreateBuilder WithFormat(string format)
    {
        _format = format;
        return this;
    }

    public VolumeCreateBuilder WithAutomount(bool automount = true)
    {
        _automount = automount;
        return this;
    }

    public VolumeCreateBuilder AddLabel(string key, string value)
    {
        _labels[key] = value;
        return this;
    }

    public VolumeCreateBuilder WithLabels(Dictionary<string, string> labels)
    {
        foreach (var kvp in labels) _labels[kvp.Key] = kvp.Value;
        return this;
    }

    public VolumeCreateRequest Build()
    {
        if (string.IsNullOrWhiteSpace(_name))
            throw new InvalidOperationException("Volume name is required");
        if (_size == 0)
            throw new InvalidOperationException("Volume size is required");

        return new VolumeCreateRequest
        {
            Name = _name,
            Size = _size,
            Location = _location,
            Server = _server,
            Format = _format,
            Automount = _automount,
            Labels = _labels.Count > 0 ? _labels : null
        };
    }

    public static implicit operator VolumeCreateRequest(VolumeCreateBuilder builder) => builder.Build();
}

/// <summary>
/// Builder for creating load balancers
/// </summary>
public sealed class LoadBalancerCreateBuilder
{
    private string _name = string.Empty;
    private string _loadBalancerType = string.Empty;
    private string? _location;
    private string? _networkZone;
    private long? _network;
    private long? _ipv4;
    private long? _ipv6;
    private readonly LoadBalancerAlgorithmBuilder _algorithm = new();
    private readonly Dictionary<string, string> _labels = [];
    private readonly List<LoadBalancerServiceCreateBuilder> _services = [];
    private readonly List<LoadBalancerTargetCreateBuilder> _targets = [];
    private bool _publicInterface = true;
    private bool _disablePrivateNetwork;

    public LoadBalancerCreateBuilder WithName(string name)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        return this;
    }

    public LoadBalancerCreateBuilder WithType(string loadBalancerType)
    {
        _loadBalancerType = loadBalancerType ?? throw new ArgumentNullException(nameof(loadBalancerType));
        return this;
    }

    public LoadBalancerCreateBuilder WithType(LoadBalancerTypeId typeId)
    {
        _loadBalancerType = typeId.Value.ToString();
        return this;
    }

    public LoadBalancerCreateBuilder WithLocation(string location)
    {
        _location = location;
        return this;
    }

    public LoadBalancerCreateBuilder WithLocation(LocationId locationId)
    {
        _location = locationId.Value.ToString();
        return this;
    }

    public LoadBalancerCreateBuilder WithNetworkZone(string networkZone)
    {
        _networkZone = networkZone;
        return this;
    }

    public LoadBalancerCreateBuilder WithNetwork(long networkId)
    {
        _network = networkId;
        return this;
    }

    public LoadBalancerCreateBuilder WithNetwork(NetworkId networkId)
    {
        _network = networkId.Value;
        return this;
    }

    public LoadBalancerCreateBuilder WithIpv4(long ipv4Id)
    {
        _ipv4 = ipv4Id;
        return this;
    }

    public LoadBalancerCreateBuilder WithIpv4(FloatingIpId ipv4Id)
    {
        _ipv4 = ipv4Id.Value;
        return this;
    }

    public LoadBalancerCreateBuilder WithIpv6(long ipv6Id)
    {
        _ipv6 = ipv6Id;
        return this;
    }

    public LoadBalancerCreateBuilder WithIpv6(FloatingIpId ipv6Id)
    {
        _ipv6 = ipv6Id.Value;
        return this;
    }

    public LoadBalancerCreateBuilder WithAlgorithm(Action<LoadBalancerAlgorithmBuilder> configure)
    {
        configure(_algorithm);
        return this;
    }

    public LoadBalancerCreateBuilder AddLabel(string key, string value)
    {
        _labels[key] = value;
        return this;
    }

    public LoadBalancerCreateBuilder WithLabels(Dictionary<string, string> labels)
    {
        foreach (var kvp in labels) _labels[kvp.Key] = kvp.Value;
        return this;
    }

    public LoadBalancerCreateBuilder AddService(Action<LoadBalancerServiceCreateBuilder> configure)
    {
        var builder = new LoadBalancerServiceCreateBuilder();
        configure(builder);
        _services.Add(builder);
        return this;
    }

    public LoadBalancerCreateBuilder AddTarget(Action<LoadBalancerTargetCreateBuilder> configure)
    {
        var builder = new LoadBalancerTargetCreateBuilder();
        configure(builder);
        _targets.Add(builder);
        return this;
    }

    public LoadBalancerCreateBuilder WithPublicInterface(bool enabled = true)
    {
        _publicInterface = enabled;
        return this;
    }

    public LoadBalancerCreateBuilder WithPrivateNetworkDisabled(bool disabled = true)
    {
        _disablePrivateNetwork = disabled;
        return this;
    }

    public LoadBalancerCreateRequest Build()
    {
        if (string.IsNullOrWhiteSpace(_name))
            throw new InvalidOperationException("Load balancer name is required");
        if (string.IsNullOrWhiteSpace(_loadBalancerType))
            throw new InvalidOperationException("Load balancer type is required");
        if (_location is null)
            throw new InvalidOperationException("Location is required");

        return new LoadBalancerCreateRequest
        {
            Name = _name,
            LoadBalancerType = _loadBalancerType,
            Location = _location,
            NetworkZone = _networkZone,
            Network = _network,
            Ipv4 = _ipv4.HasValue ? new IpBlockRequest { Ip = _ipv4.Value.ToString() } : null,
            Ipv6 = _ipv6.HasValue ? new IpBlockRequest { Ip = _ipv6.Value.ToString() } : null,
            Algorithm = _algorithm.Build(),
            Labels = _labels.Count > 0 ? _labels : null,
            Services = _services.Count > 0 ? _services.Select(s => s.Build()).ToList() : [],
            Targets = _targets.Count > 0 ? _targets.Select(t => t.Build()).ToList() : [],
            PublicInterface = _publicInterface,
            DisablePrivateNetwork = _disablePrivateNetwork
        };
    }

    public static implicit operator LoadBalancerCreateRequest(LoadBalancerCreateBuilder builder) => builder.Build();
}

/// <summary>
/// Builder for load balancer algorithm
/// </summary>
public sealed class LoadBalancerAlgorithmBuilder
{
    private LoadBalancerAlgorithmType _type = LoadBalancerAlgorithmType.RoundRobin;

    public LoadBalancerAlgorithmBuilder RoundRobin() { _type = LoadBalancerAlgorithmType.RoundRobin; return this; }
    public LoadBalancerAlgorithmBuilder LeastConnections() { _type = LoadBalancerAlgorithmType.LeastConnections; return this; }

    internal LoadBalancerAlgorithm Build() => new() { Type = _type };
}

/// <summary>
/// Builder for load balancer service
/// </summary>
public sealed class LoadBalancerServiceCreateBuilder
{
    private LoadBalancerServiceProtocol _protocol = LoadBalancerServiceProtocol.Http;
    private int _listenPort = 80;
    private int _destinationPort = 8080;
    private LoadBalancerServiceHttpBuilder? _http;
    private LoadBalancerHealthCheckBuilder _healthCheck = new();

    public LoadBalancerServiceCreateBuilder WithProtocol(LoadBalancerServiceProtocol protocol)
    {
        _protocol = protocol;
        return this;
    }

    public LoadBalancerServiceCreateBuilder WithListenPort(int port)
    {
        if (port < 1 || port > 65535)
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535");
        _listenPort = port;
        return this;
    }

    public LoadBalancerServiceCreateBuilder WithDestinationPort(int port)
    {
        if (port < 1 || port > 65535)
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535");
        _destinationPort = port;
        return this;
    }

    public LoadBalancerServiceCreateBuilder WithHttp(Action<LoadBalancerServiceHttpBuilder> configure)
    {
        var builder = new LoadBalancerServiceHttpBuilder();
        configure(builder);
        _http = builder;
        return this;
    }

    public LoadBalancerServiceCreateBuilder WithHealthCheck(Action<LoadBalancerHealthCheckBuilder> configure)
    {
        configure(_healthCheck);
        return this;
    }

    internal LoadBalancerServiceCreateRequest Build()
    {
        return new LoadBalancerServiceCreateRequest
        {
            Protocol = _protocol,
            ListenPort = _listenPort,
            DestinationPort = _destinationPort,
            Http = _http?.Build(),
            HealthCheck = _healthCheck.Build()
        };
    }
}

/// <summary>
/// Builder for HTTP service options
/// </summary>
public sealed class LoadBalancerServiceHttpBuilder
{
    private readonly List<long> _certificates = [];
    private LoadBalancerServiceHttpRedirectHttpCreateRequestBuilder? _redirectHttp;
    private LoadBalancerServiceHttpStickySessionsCreateRequestBuilder? _stickySessions;

    public LoadBalancerServiceHttpBuilder AddCertificate(long certificateId)
    {
        _certificates.Add(certificateId);
        return this;
    }

    public LoadBalancerServiceHttpBuilder AddCertificate(CertificateId certificateId)
    {
        _certificates.Add(certificateId.Value);
        return this;
    }

    public LoadBalancerServiceHttpBuilder WithRedirectHttp(Action<LoadBalancerServiceHttpRedirectHttpCreateRequestBuilder> configure)
    {
        var builder = new LoadBalancerServiceHttpRedirectHttpCreateRequestBuilder();
        configure(builder);
        _redirectHttp = builder;
        return this;
    }

    public LoadBalancerServiceHttpBuilder WithStickySessions(Action<LoadBalancerServiceHttpStickySessionsCreateRequestBuilder> configure)
    {
        var builder = new LoadBalancerServiceHttpStickySessionsCreateRequestBuilder();
        configure(builder);
        _stickySessions = builder;
        return this;
    }

    internal LoadBalancerServiceHttpCreateRequest Build()
    {
        return new LoadBalancerServiceHttpCreateRequest
        {
            Certificates = _certificates,
            RedirectHttp = _redirectHttp?.Build(),
            StickySessions = _stickySessions?.Build()
        };
    }
}

/// <summary>
/// Builder for HTTP redirect
/// </summary>
public sealed class LoadBalancerServiceHttpRedirectHttpCreateRequestBuilder
{
    private bool _enabled;
    private int _statusCode = 301;

    public LoadBalancerServiceHttpRedirectHttpCreateRequestBuilder Enable() { _enabled = true; return this; }
    public LoadBalancerServiceHttpRedirectHttpCreateRequestBuilder Disable() { _enabled = false; return this; }
    public LoadBalancerServiceHttpRedirectHttpCreateRequestBuilder WithStatusCode(int statusCode) { _statusCode = statusCode; return this; }

    internal LoadBalancerServiceHttpRedirectHttpCreateRequest Build() => new() { Enabled = _enabled, StatusCode = _statusCode };
}

/// <summary>
/// Builder for sticky sessions
/// </summary>
public sealed class LoadBalancerServiceHttpStickySessionsCreateRequestBuilder
{
    private bool _enabled;
    private string? _cookieName;

    public LoadBalancerServiceHttpStickySessionsCreateRequestBuilder Enable() { _enabled = true; return this; }
    public LoadBalancerServiceHttpStickySessionsCreateRequestBuilder Disable() { _enabled = false; return this; }
    public LoadBalancerServiceHttpStickySessionsCreateRequestBuilder WithCookieName(string cookieName) { _cookieName = cookieName; return this; }

    internal LoadBalancerServiceHttpStickySessionsCreateRequest Build() => new() { Enabled = _enabled, CookieName = _cookieName };
}

/// <summary>
/// Builder for proxy protocol
/// </summary>

/// <summary>
/// Builder for health check
/// </summary>
public sealed class LoadBalancerHealthCheckBuilder
{
    private LoadBalancerHealthCheckProtocol _protocol = LoadBalancerHealthCheckProtocol.Http;
    private int _port = 80;
    private int _interval = 15;
    private int _timeout = 10;
    private int _retries = 3;
    private LoadBalancerHealthCheckHttpCreateRequestBuilder? _http;

    public LoadBalancerHealthCheckBuilder WithProtocol(LoadBalancerHealthCheckProtocol protocol) { _protocol = protocol; return this; }
    public LoadBalancerHealthCheckBuilder WithPort(int port) { _port = port; return this; }
    public LoadBalancerHealthCheckBuilder WithInterval(int seconds) { _interval = seconds; return this; }
    public LoadBalancerHealthCheckBuilder WithTimeout(int seconds) { _timeout = seconds; return this; }
    public LoadBalancerHealthCheckBuilder WithRetries(int retries) { _retries = retries; return this; }

    public LoadBalancerHealthCheckBuilder WithHttp(Action<LoadBalancerHealthCheckHttpCreateRequestBuilder> configure)
    {
        var builder = new LoadBalancerHealthCheckHttpCreateRequestBuilder();
        configure(builder);
        _http = builder;
        return this;
    }

    internal LoadBalancerHealthCheckCreateRequest Build()
    {
        return new LoadBalancerHealthCheckCreateRequest
        {
            Protocol = _protocol,
            Port = _port,
            Interval = _interval,
            Timeout = _timeout,
            Retries = _retries,
            Http = _http?.Build()
        };
    }
}

/// <summary>
/// Builder for HTTP health check
/// </summary>
public sealed class LoadBalancerHealthCheckHttpCreateRequestBuilder
{
    private string _path = "/";
    private string? _domain;
    private LoadBalancerHealthCheckHttpResponseCreateRequestBuilder? _response;
    private bool _tls;

    public LoadBalancerHealthCheckHttpCreateRequestBuilder WithPath(string path) { _path = path; return this; }
    public LoadBalancerHealthCheckHttpCreateRequestBuilder WithDomain(string domain) { _domain = domain; return this; }
    public LoadBalancerHealthCheckHttpCreateRequestBuilder WithTls(bool tls = true) { _tls = tls; return this; }
    public LoadBalancerHealthCheckHttpCreateRequestBuilder WithResponse(Action<LoadBalancerHealthCheckHttpResponseCreateRequestBuilder> configure)
    {
        var builder = new LoadBalancerHealthCheckHttpResponseCreateRequestBuilder();
        configure(builder);
        _response = builder;
        return this;
    }

    internal LoadBalancerHealthCheckHttpCreateRequest Build()
    {
        return new LoadBalancerHealthCheckHttpCreateRequest
        {
            Path = _path,
            Domain = _domain,
            Response = _response?.Build(),
            Tls = _tls
        };
    }
}

/// <summary>
/// Builder for health check response
/// </summary>
public sealed class LoadBalancerHealthCheckHttpResponseCreateRequestBuilder
{
    private readonly List<string> _statusCodes = ["2??", "3??"];

    public LoadBalancerHealthCheckHttpResponseCreateRequestBuilder WithStatusCodes(params string[] codes)
    {
        _statusCodes.Clear();
        _statusCodes.AddRange(codes);
        return this;
    }

    internal LoadBalancerHealthCheckHttpResponseCreateRequest Build() => new() { StatusCodes = _statusCodes };
}

/// <summary>
/// Builder for health check response
/// </summary>
public sealed class LoadBalancerHealthCheckHttpResponseBuilder
{
    private readonly List<string> _statusCodes = ["2??", "3??"];

    public LoadBalancerHealthCheckHttpResponseBuilder WithStatusCodes(params string[] codes)
    {
        _statusCodes.Clear();
        _statusCodes.AddRange(codes);
        return this;
    }

    internal LoadBalancerHealthCheckHttpResponse Build() => new() { StatusCodes = _statusCodes };
}

/// <summary>
/// Builder for load balancer target
/// </summary>
public sealed class LoadBalancerTargetCreateBuilder
{
    private LoadBalancerTargetType _type = LoadBalancerTargetType.Server;
    private long? _server;
    private LoadBalancerTargetLabelSelectorBuilder? _labelSelector;
    private LoadBalancerTargetIpBuilder? _ip;
    private bool _usePrivateIp;

    public LoadBalancerTargetCreateBuilder AsServer(long serverId) { _type = LoadBalancerTargetType.Server; _server = serverId; return this; }
    public LoadBalancerTargetCreateBuilder AsServer(ServerId serverId) { _type = LoadBalancerTargetType.Server; _server = serverId.Value; return this; }

    public LoadBalancerTargetCreateBuilder AsLabelSelector(Action<LoadBalancerTargetLabelSelectorBuilder> configure)
    {
        var builder = new LoadBalancerTargetLabelSelectorBuilder();
        configure(builder);
        _labelSelector = builder;
        _type = LoadBalancerTargetType.LabelSelector;
        return this;
    }

    public LoadBalancerTargetCreateBuilder AsIp(Action<LoadBalancerTargetIpBuilder> configure)
    {
        var builder = new LoadBalancerTargetIpBuilder();
        configure(builder);
        _ip = builder;
        _type = LoadBalancerTargetType.Ip;
        return this;
    }


    internal LoadBalancerTargetCreateRequest Build()
    {
        return new LoadBalancerTargetCreateRequest
        {
            Type = _type,
            Server = _server,
            LabelSelector = _labelSelector?.Build(),
            Ip = _ip?.Build(),
            UsePrivateIp = _usePrivateIp
        };
    }

    public LoadBalancerTargetCreateBuilder UsePrivateIp(bool usePrivateIp = true)
    {
        _usePrivateIp = usePrivateIp;
        return this;
    }

}

/// <summary>
/// Builder for label selector target
/// </summary>
public sealed class LoadBalancerTargetLabelSelectorBuilder
{
    private string _selector = string.Empty;

    public LoadBalancerTargetLabelSelectorBuilder WithSelector(string selector)
    {
        _selector = selector ?? throw new ArgumentNullException(nameof(selector));
        return this;
    }

    internal LoadBalancerTargetLabelSelectorCreateRequest Build() => new() { Selector = _selector };
}

/// <summary>
/// Builder for IP target
/// </summary>
public sealed class LoadBalancerTargetIpBuilder
{
    private string _ip = string.Empty;

    public LoadBalancerTargetIpBuilder WithIp(string ip)
    {
        _ip = ip ?? throw new ArgumentNullException(nameof(ip));
        return this;
    }

    internal LoadBalancerTargetIpCreateRequest Build() => new() { Ip = _ip };
}

/// <summary>
/// Builder for creating networks
/// </summary>
public sealed class NetworkCreateBuilder
{
    private string _name = string.Empty;
    private string _ipRange = string.Empty;
    private string? _networkZone;
    private readonly List<NetworkSubnetBuilder> _subnets = [];
    private readonly Dictionary<string, string> _labels = [];
    private bool _exposeRoutesToVSwitch;

    public NetworkCreateBuilder WithName(string name)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        return this;
    }

    public NetworkCreateBuilder WithIpRange(string ipRange)
    {
        _ipRange = ipRange ?? throw new ArgumentNullException(nameof(ipRange));
        return this;
    }

    public NetworkCreateBuilder WithNetworkZone(string networkZone)
    {
        _networkZone = networkZone;
        return this;
    }

    public NetworkCreateBuilder AddSubnet(Action<NetworkSubnetBuilder> configure)
    {
        var builder = new NetworkSubnetBuilder();
        configure(builder);
        _subnets.Add(builder);
        return this;
    }

    public NetworkCreateBuilder AddLabel(string key, string value)
    {
        _labels[key] = value;
        return this;
    }

    public NetworkCreateBuilder WithExposeRoutesToVSwitch(bool expose = true)
    {
        _exposeRoutesToVSwitch = expose;
        return this;
    }

public NetworkCreateRequest Build()
        {
            if (string.IsNullOrWhiteSpace(_name))
                throw new InvalidOperationException("Network name is required");
            if (string.IsNullOrWhiteSpace(_ipRange))
                throw new InvalidOperationException("IP range is required");

            return new NetworkCreateRequest
            {
                Name = _name,
                IpRange = _ipRange,
                NetworkZone = _networkZone,
                Subnets = _subnets.Count > 0 ? _subnets.Select(s => s.Build()).ToList() : [],
                Labels = _labels.Count > 0 ? _labels : null,
                ExposeRoutesToVswitch = _exposeRoutesToVSwitch
            };
        }

    public static implicit operator NetworkCreateRequest(NetworkCreateBuilder builder) => builder.Build();
}

/// <summary>
/// Builder for network subnet
/// </summary>
public sealed class NetworkSubnetBuilder
{
    private NetworkSubnetType _type = NetworkSubnetType.Cloud;
    private string? _networkZone;
    private string _ipRange = string.Empty;
    private long? _vswitchId;

    public NetworkSubnetBuilder AsCloud()
    {
        _type = NetworkSubnetType.Cloud;
        return this;
    }

    public NetworkSubnetBuilder AsVSwitch()
    {
        _type = NetworkSubnetType.VSwitch;
        return this;
    }

    public NetworkSubnetBuilder WithNetworkZone(string networkZone)
    {
        _networkZone = networkZone;
        return this;
    }

    public NetworkSubnetBuilder WithIpRange(string ipRange)
    {
        _ipRange = ipRange ?? throw new ArgumentNullException(nameof(ipRange));
        return this;
    }

public NetworkSubnetBuilder WithVSwitchId(long vswitchId)
        {
            _vswitchId = vswitchId;
            return this;
        }

    internal NetworkSubnetCreateRequest Build()
        {
            if (string.IsNullOrWhiteSpace(_ipRange))
                throw new InvalidOperationException("IP range is required for subnet");

            return new NetworkSubnetCreateRequest
            {
                Type = _type,
                NetworkZone = _networkZone,
                IpRange = _ipRange,
                VswitchId = _vswitchId
            };
        }
    }
