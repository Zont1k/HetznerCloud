using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HetznerCloud.Models;

/// <summary>
/// Marker interface for branded ID types
/// </summary>
public interface IBrandedId
{
    long Value { get; }
}

/// <summary>
/// JSON converter for branded ID types
/// </summary>
public sealed class BrandedIdJsonConverter<T> : JsonConverter<T> where T : struct, IBrandedId
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString()!;
            if (long.TryParse(str, out var value))
            {
                return (T)Activator.CreateInstance(typeof(T), value)!;
            }
        }
        
        var longValue = reader.GetInt64();
        return (T)Activator.CreateInstance(typeof(T), longValue)!;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}

/// <summary>
/// Strongly-typed ID for Servers
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<ServerId>))]
public readonly record struct ServerId : IBrandedId
{
    public long Value { get; init; }
    public ServerId(long value) => Value = value;
    public static implicit operator long(ServerId id) => id.Value;
    public static explicit operator ServerId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is ServerId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed ID for Volumes
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<VolumeId>))]
public readonly record struct VolumeId : IBrandedId
{
    public long Value { get; init; }
    public VolumeId(long value) => Value = value;
    public static implicit operator long(VolumeId id) => id.Value;
    public static explicit operator VolumeId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is VolumeId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed ID for Load Balancers
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<LoadBalancerId>))]
public readonly record struct LoadBalancerId : IBrandedId
{
    public long Value { get; init; }
    public LoadBalancerId(long value) => Value = value;
    public static implicit operator long(LoadBalancerId id) => id.Value;
    public static explicit operator LoadBalancerId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is LoadBalancerId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed ID for Networks
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<NetworkId>))]
public readonly record struct NetworkId : IBrandedId
{
    public long Value { get; init; }
    public NetworkId(long value) => Value = value;
    public static implicit operator long(NetworkId id) => id.Value;
    public static explicit operator NetworkId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is NetworkId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed ID for Floating IPs
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<FloatingIpId>))]
public readonly record struct FloatingIpId : IBrandedId
{
    public long Value { get; init; }
    public FloatingIpId(long value) => Value = value;
    public static implicit operator long(FloatingIpId id) => id.Value;
    public static explicit operator FloatingIpId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is FloatingIpId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed ID for SSH Keys
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<SshKeyId>))]
public readonly record struct SshKeyId : IBrandedId
{
    public long Value { get; init; }
    public SshKeyId(long value) => Value = value;
    public static implicit operator long(SshKeyId id) => id.Value;
    public static explicit operator SshKeyId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is SshKeyId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed ID for Certificates
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<CertificateId>))]
public readonly record struct CertificateId : IBrandedId
{
    public long Value { get; init; }
    public CertificateId(long value) => Value = value;
    public static implicit operator long(CertificateId id) => id.Value;
    public static explicit operator CertificateId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is CertificateId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed ID for Placement Groups
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<PlacementGroupId>))]
public readonly record struct PlacementGroupId : IBrandedId
{
    public long Value { get; init; }
    public PlacementGroupId(long value) => Value = value;
    public static implicit operator long(PlacementGroupId id) => id.Value;
    public static explicit operator PlacementGroupId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is PlacementGroupId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed ID for Firewalls
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<FirewallId>))]
public readonly record struct FirewallId : IBrandedId
{
    public long Value { get; init; }
    public FirewallId(long value) => Value = value;
    public static implicit operator long(FirewallId id) => id.Value;
    public static explicit operator FirewallId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is FirewallId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed ID for ISO Images
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<IsoImageId>))]
public readonly record struct IsoImageId : IBrandedId
{
    public long Value { get; init; }
    public IsoImageId(long value) => Value = value;
    public static implicit operator long(IsoImageId id) => id.Value;
    public static explicit operator IsoImageId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is IsoImageId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed ID for Actions
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<ActionId>))]
public readonly record struct ActionId : IBrandedId
{
    public long Value { get; init; }
    public ActionId(long value) => Value = value;
    public static implicit operator long(ActionId id) => id.Value;
    public static explicit operator ActionId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is ActionId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed ID for Datacenters
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<DatacenterId>))]
public readonly record struct DatacenterId : IBrandedId
{
    public long Value { get; init; }
    public DatacenterId(long value) => Value = value;
    public static implicit operator long(DatacenterId id) => id.Value;
    public static explicit operator DatacenterId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is DatacenterId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed ID for Locations
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<LocationId>))]
public readonly record struct LocationId : IBrandedId
{
    public long Value { get; init; }
    public LocationId(long value) => Value = value;
    public static implicit operator long(LocationId id) => id.Value;
    public static explicit operator LocationId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is LocationId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed ID for Server Types
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<ServerTypeId>))]
public readonly record struct ServerTypeId : IBrandedId
{
    public long Value { get; init; }
    public ServerTypeId(long value) => Value = value;
    public static implicit operator long(ServerTypeId id) => id.Value;
    public static explicit operator ServerTypeId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is ServerTypeId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed ID for Images
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<ImageId>))]
public readonly record struct ImageId : IBrandedId
{
    public long Value { get; init; }
    public ImageId(long value) => Value = value;
    public static implicit operator long(ImageId id) => id.Value;
    public static explicit operator ImageId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is ImageId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed ID for Load Balancer Types
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<LoadBalancerTypeId>))]
public readonly record struct LoadBalancerTypeId : IBrandedId
{
    public long Value { get; init; }
    public LoadBalancerTypeId(long value) => Value = value;
    public static implicit operator long(LoadBalancerTypeId id) => id.Value;
    public static explicit operator LoadBalancerTypeId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is LoadBalancerTypeId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed ID for Primary IPs
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<PrimaryIpId>))]
public readonly record struct PrimaryIpId : IBrandedId
{
    public long Value { get; init; }
    public PrimaryIpId(long value) => Value = value;
    public static implicit operator long(PrimaryIpId id) => id.Value;
    public static explicit operator PrimaryIpId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is PrimaryIpId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed ID for DNS Zones
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<DnsZoneId>))]
public readonly record struct DnsZoneId : IBrandedId
{
    public long Value { get; init; }
    public DnsZoneId(long value) => Value = value;
    public static implicit operator long(DnsZoneId id) => id.Value;
    public static explicit operator DnsZoneId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is DnsZoneId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed ID for Storage Boxes
/// </summary>
[JsonConverter(typeof(BrandedIdJsonConverter<StorageBoxId>))]
public readonly record struct StorageBoxId : IBrandedId
{
    public long Value { get; init; }
    public StorageBoxId(long value) => Value = value;
    public static implicit operator long(StorageBoxId id) => id.Value;
    public static explicit operator StorageBoxId(long value) => new(value);
    public bool Equals(IBrandedId? other) => other is StorageBoxId id && id.Value == Value;
    public int CompareTo(IBrandedId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}