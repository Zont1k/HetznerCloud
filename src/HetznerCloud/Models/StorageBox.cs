using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HetznerCloud.Pagination;

namespace HetznerCloud.Models;

public class StorageBox
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("access_settings")] public StorageBoxAccessSettings AccessSettings { get; set; } = new();
    [JsonPropertyName("zone")] public string Zone { get; set; } = string.Empty;
    [JsonPropertyName("product")] public string Product { get; set; } = string.Empty;
    [JsonPropertyName("location")] public Location Location { get; set; } = new();
    [JsonPropertyName("server_ip")] public string ServerIp { get; set; } = string.Empty;
    [JsonPropertyName("server_ipv6")] public string? ServerIpv6 { get; set; }
    [JsonPropertyName("labels")] public Dictionary<string, string> Labels { get; set; } = [];
    [JsonPropertyName("created")] public DateTime Created { get; set; }
}

public class StorageBoxAccessSettings
{
    [JsonPropertyName("reachable")] public bool? Reachable { get; set; }
    [JsonPropertyName("ssh_enabled")] public bool? SshEnabled { get; set; }
    [JsonPropertyName("webdav_enabled")] public bool? WebdavEnabled { get; set; }
    [JsonPropertyName("samba_enabled")] public bool? SambaEnabled { get; set; }
    [JsonPropertyName("rental_enabled")] public bool? RentalEnabled { get; set; }
}

public class StorageBoxListResponse
{
    [JsonPropertyName("storage_boxes")] public List<StorageBox> StorageBoxes { get; set; } = [];
    [JsonPropertyName("meta")] public PaginationMeta Meta { get; set; } = new();
}

public class StorageBoxResponse
{
    [JsonPropertyName("storage_box")] public StorageBox StorageBox { get; set; } = new();
}

public class StorageBoxUpdateRequest
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("labels")] public Dictionary<string, string>? Labels { get; set; }
}

public class StorageBoxChangePasswordRequest
{
    [JsonPropertyName("password")] public string Password { get; set; } = string.Empty;
}
