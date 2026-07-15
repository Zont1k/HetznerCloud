using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HetznerCloud.Pagination;

namespace HetznerCloud.Models;

public class Volume
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public Location Location { get; set; } = new();

    [JsonPropertyName("server")]
    public ResourceReference? Server { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    [JsonPropertyName("protection")]
    public VolumeProtection Protection { get; set; } = new();

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; } = [];

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    [JsonPropertyName("linux_device")]
    public string LinuxDevice { get; set; } = string.Empty;
}

public class VolumeProtection
{
    [JsonPropertyName("delete")]
    public bool Delete { get; set; }
}

public class VolumeListResponse
{
    [JsonPropertyName("volumes")]
    public List<Volume> Volumes { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();
}

public class VolumeResponse
{
    [JsonPropertyName("volume")]
    public Volume Volume { get; set; } = new();
}

public class VolumeCreateRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("server")]
    public long? Server { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("automount")]
    public bool Automount { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

public class VolumeUpdateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

public class VolumeActionRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class VolumeAttachRequest : VolumeActionRequest
{
    public VolumeAttachRequest()
    {
        Type = "attach";
    }

    [JsonPropertyName("server")]
    public long Server { get; set; }

    [JsonPropertyName("automount")]
    public bool Automount { get; set; }
}

public class VolumeDetachRequest : VolumeActionRequest
{
    public VolumeDetachRequest()
    {
        Type = "detach";
    }
}

public class VolumeResizeRequest : VolumeActionRequest
{
    public VolumeResizeRequest()
    {
        Type = "resize";
    }

    [JsonPropertyName("size")]
    public int Size { get; set; }
}

public class VolumeChangeProtectionRequest : VolumeActionRequest
{
    public VolumeChangeProtectionRequest()
    {
        Type = "change_protection";
    }

    [JsonPropertyName("delete")]
    public bool Delete { get; set; }
}

public class VolumeActionResponse
{
    [JsonPropertyName("action")]
    public Action Action { get; set; } = new();
}