using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HetznerCloud.Models;

public class Label
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

public class LabelSelector
{
    [JsonPropertyName("selector")]
    public string Selector { get; set; } = string.Empty;
}

public class LabelSelectorRequest
{
    [JsonPropertyName("label_selector")]
    public string? LabelSelector { get; set; }
}

public class ResourceReferenceResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class LabelSelectorFilter
{
    [JsonPropertyName("label_selector")]
    public string? LabelSelector { get; set; }
}

public class PaginationRequest
{
    public int Page { get; set; } = 1;
    public int PerPage { get; set; } = 25;
    public string? Sort { get; set; }
    public string? LabelSelector { get; set; }
}