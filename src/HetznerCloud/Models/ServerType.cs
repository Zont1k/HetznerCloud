using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HetznerCloud.Pagination;

namespace HetznerCloud.Models;

public class ServerType
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("cores")]
    public int Cores { get; set; }

    [JsonPropertyName("memory")]
    public double Memory { get; set; }

    [JsonPropertyName("disk")]
    public int Disk { get; set; }

    [JsonPropertyName("storage_type")]
    public string StorageType { get; set; } = string.Empty;

    [JsonPropertyName("cpu_type")]
    public string CpuType { get; set; } = string.Empty;

    [JsonPropertyName("architecture")]
    public ServerArchitecture Architecture { get; set; }

    [JsonPropertyName("included_traffic")]
    public ulong IncludedTraffic { get; set; }

    [JsonPropertyName("prices")]
    public List<Price> Prices { get; set; } = [];

    [JsonPropertyName("deprecated")]
    public bool Deprecated { get; set; }

    [JsonPropertyName("available")]
    public bool Available { get; set; }
}

public enum ServerArchitecture
{
    [JsonPropertyName("x86")]
    X86,
    [JsonPropertyName("arm")]
    Arm
}

public class ServerTypeListResponse
{
    [JsonPropertyName("server_types")]
    public List<ServerType> ServerTypes { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class ServerTypeResponse
{
    [JsonPropertyName("server_type")]
    public ServerType ServerType { get; set; } = new();
}

public class Image
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("type")]
    public ImageType Type { get; set; }

    [JsonPropertyName("status")]
    public ImageStatus Status { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("image_size")]
    public double ImageSize { get; set; }

    [JsonPropertyName("disk_size")]
    public int DiskSize { get; set; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    [JsonPropertyName("created_from")]
    public ResourceReference? CreatedFrom { get; set; }

    [JsonPropertyName("bound_to")]
    public long? BoundTo { get; set; }

    [JsonPropertyName("os_flavor")]
    public string OsFlavor { get; set; } = string.Empty;

    [JsonPropertyName("os_version")]
    public string? OsVersion { get; set; }

    [JsonPropertyName("rapid_deploy")]
    public bool RapidDeploy { get; set; }

    [JsonPropertyName("architecture")]
    public ImageArchitecture Architecture { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; } = [];

    [JsonPropertyName("deprecated")]
    public DateTime? Deprecated { get; set; }

    [JsonPropertyName("protection")]
    public ImageProtection Protection { get; set; } = new();
}

public enum ImageType
{
    [JsonPropertyName("system")]
    System,
    [JsonPropertyName("snapshot")]
    Snapshot,
    [JsonPropertyName("backup")]
    Backup,
    [JsonPropertyName("app")]
    App
}

public enum ImageStatus
{
    [JsonPropertyName("available")]
    Available,
    [JsonPropertyName("creating")]
    Creating,
    [JsonPropertyName("deleted")]
    Deleted
}

public enum ImageArchitecture
{
    [JsonPropertyName("x86")]
    X86,
    [JsonPropertyName("arm")]
    Arm
}

public class ImageProtection
{
    [JsonPropertyName("delete")]
    public bool Delete { get; set; }
}

public class ImageListResponse
{
    [JsonPropertyName("images")]
    public List<Image> Images { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class ImageResponse
{
    [JsonPropertyName("image")]
    public Image Image { get; set; } = new();
}

public class ImageCreateRequest
{
    [JsonPropertyName("type")]
    public ImageType Type { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

public class ImageUpdateRequest
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }

    [JsonPropertyName("type")]
    public ImageType? Type { get; set; }
}

public class ImageActionRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class ImageChangeProtectionRequest : ImageActionRequest
{
    public ImageChangeProtectionRequest()
    {
        Type = "change_protection";
    }

    [JsonPropertyName("delete")]
    public bool? Delete { get; set; }
}

public class ImageActionResponse
{
    [JsonPropertyName("action")]
    public Action Action { get; set; } = new();

    [JsonPropertyName("image")]
    public Image? Image { get; set; }
}

public class Location
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("network_zone")]
    public string NetworkZone { get; set; } = string.Empty;
}

public class LocationListResponse
{
    [JsonPropertyName("locations")]
    public List<Location> Locations { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class LocationResponse
{
    [JsonPropertyName("location")]
    public Location Location { get; set; } = new();
}

public class Datacenter
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public Location Location { get; set; } = new();

    [JsonPropertyName("server_types")]
    public ServerTypeAvailability ServerTypes { get; set; } = new();
}

public class ServerTypeAvailability
{
    [JsonPropertyName("available")]
    public List<long> Available { get; set; } = [];

    [JsonPropertyName("available_for_migration")]
    public List<long> AvailableForMigration { get; set; } = [];

    [JsonPropertyName("supported")]
    public List<long> Supported { get; set; } = [];
}

public class DatacenterListResponse
{
    [JsonPropertyName("datacenters")]
    public List<Datacenter> Datacenters { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class DatacenterResponse
{
    [JsonPropertyName("datacenter")]
    public Datacenter Datacenter { get; set; } = new();
}

public class Price
{
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("price_monthly")]
    public PriceDetails PriceMonthly { get; set; } = new();

    [JsonPropertyName("price_hourly")]
    public PriceDetails PriceHourly { get; set; } = new();

    [JsonPropertyName("net")]
    public bool Net { get; set; }
}

public class PriceDetails
{
    [JsonPropertyName("gross")]
    public decimal Gross { get; set; }

    [JsonPropertyName("net")]
    public decimal Net { get; set; }
}

public class PricingListResponse
{
    [JsonPropertyName("pricing")]
    public Pricing Pricing { get; set; } = new();
}

public class Pricing
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("vat_rate")]
    public decimal? VatRate { get; set; }

    [JsonPropertyName("image")]
    public List<ImagePricing>? Image { get; set; }

    [JsonPropertyName("server_types")]
    public List<ServerTypePricing>? ServerTypes { get; set; }

    [JsonPropertyName("server_network_traffic")]
    public List<ServerNetworkTrafficPricing>? ServerNetworkTraffic { get; set; }

    [JsonPropertyName("primary_ips")]
    public List<PrimaryIpPricing>? PrimaryIps { get; set; }

    [JsonPropertyName("floating_ips")]
    public List<FloatingIpPricing>? FloatingIps { get; set; }

    [JsonPropertyName("volumes")]
    public List<VolumePricing>? Volumes { get; set; }

    [JsonPropertyName("load_balancers")]
    public List<LoadBalancerPricing>? LoadBalancers { get; set; }

    [JsonPropertyName("networks")]
    public List<NetworkPricing>? Networks { get; set; }

    [JsonPropertyName("certificates")]
    public List<CertificatePricing>? Certificates { get; set; }

    [JsonPropertyName("firewalls")]
    public List<FirewallPricing>? Firewalls { get; set; }

    [JsonPropertyName("storage_boxes")]
    public List<StorageBoxPricing>? StorageBoxes { get; set; }

    [JsonPropertyName("labels")]
    public List<LabelPricing>? Labels { get; set; }
}

public class ImagePricing
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("prices")]
    public List<Price> Prices { get; set; } = [];
}

public class ServerTypePricing
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("prices")]
    public List<Price> Prices { get; set; } = [];
}

public class ServerNetworkTrafficPricing
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("prices")]
    public List<Price> Prices { get; set; } = [];
}

public class PrimaryIpPricing
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("prices")]
    public List<Price> Prices { get; set; } = [];
}

public class FloatingIpPricing
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("prices")]
    public List<Price> Prices { get; set; } = [];
}

public class VolumePricing
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("prices")]
    public List<Price> Prices { get; set; } = [];
}

public class LoadBalancerPricing
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("prices")]
    public List<Price> Prices { get; set; } = [];
}

public class NetworkPricing
{
    [JsonPropertyName("prices")]
    public List<Price> Prices { get; set; } = [];
}

public class CertificatePricing
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("prices")]
    public List<Price> Prices { get; set; } = [];
}

public class FirewallPricing
{
    [JsonPropertyName("prices")]
    public List<Price> Prices { get; set; } = [];
}

public class StorageBoxPricing
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("prices")]
    public List<Price> Prices { get; set; } = [];
}

public class LabelPricing
{
    [JsonPropertyName("labels")]
    public decimal Labels { get; set; }
}