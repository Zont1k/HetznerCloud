using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HetznerCloud.Pagination;

namespace HetznerCloud.Models;

public class DnsZone
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("ttl")]
    public long? Ttl { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; } = [];

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    [JsonPropertyName("modified")]
    public DateTime? Modified { get; set; }

    [JsonPropertyName("zone")]
    public string Zone { get; set; } = string.Empty;

    [JsonPropertyName("primary_name_server")]
    public string? PrimaryNameServer { get; set; }

    [JsonPropertyName("nameservers")]
    public List<string>? Nameservers { get; set; }

    [JsonPropertyName("protection")]
    public DnsZoneProtection Protection { get; set; } = new();
}

public class DnsZoneProtection
{
    [JsonPropertyName("change")]
    public bool Change { get; set; }

    [JsonPropertyName("delete")]
    public bool Delete { get; set; }
}

public class DnsZoneListResponse
{
    [JsonPropertyName("dns_zones")]
    public List<DnsZone> DnsZones { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class DnsZoneResponse
{
    [JsonPropertyName("dns_zone")]
    public DnsZone DnsZone { get; set; } = new();
}

public class DnsZoneCreateRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("ttl")]
    public long? Ttl { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

public class DnsZoneUpdateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("ttl")]
    public long? Ttl { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

public class DnsZoneRecord
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("type")]
    public DnsRecordType Type { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("ttl")]
    public long? Ttl { get; set; }
}

public enum DnsRecordType
{
    [JsonPropertyName("A")]
    A,
    [JsonPropertyName("AAAA")]
    Aaaa,
    [JsonPropertyName("CNAME")]
    Cname,
    [JsonPropertyName("MX")]
    Mx,
    [JsonPropertyName("NS")]
    Ns,
    [JsonPropertyName("TXT")]
    Txt,
    [JsonPropertyName("SRV")]
    Srv,
    [JsonPropertyName("CAA")]
    Caa,
    [JsonPropertyName("RP")]
    Rp,
    [JsonPropertyName("SOA")]
    Soa,
    [JsonPropertyName("HFSDB")]
    Hfsdb,
    [JsonPropertyName("PTR")]
    Ptr
}

public class DnsZoneRecordListResponse
{
    [JsonPropertyName("records")]
    public List<DnsZoneRecord> Records { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class DnsZoneRecordResponse
{
    [JsonPropertyName("record")]
    public DnsZoneRecord Record { get; set; } = new();
}

public class DnsZoneRecordCreateRequest
{
    [JsonPropertyName("type")]
    public DnsRecordType Type { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("ttl")]
    public long? Ttl { get; set; }
}

public class DnsZoneRecordUpdateRequest
{
    [JsonPropertyName("type")]
    public DnsRecordType? Type { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("ttl")]
    public long? Ttl { get; set; }
}

public class DnsZoneProtectionChangeRequest
{
    [JsonPropertyName("change")]
    public bool Change { get; set; }
}
