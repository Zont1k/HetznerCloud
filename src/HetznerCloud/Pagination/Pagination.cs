using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HetznerCloud.Pagination;

public class PaginationInfo
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("per_page")]
    public int PerPage { get; set; }

    [JsonPropertyName("previous_page")]
    public int? PreviousPage { get; set; }

    [JsonPropertyName("next_page")]
    public int? NextPage { get; set; }

    [JsonPropertyName("last_page")]
    public int LastPage { get; set; }

    [JsonPropertyName("total_entries")]
    public int TotalEntries { get; set; }
}

public class PaginatedResponse<T>
{
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = [];

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; set; } = new();

    public bool HasNextPage => Meta.Pagination.NextPage.HasValue;

    public bool HasPreviousPage => Meta.Pagination.PreviousPage.HasValue;
}

public class PaginationMeta
{
    [JsonPropertyName("pagination")]
    public PaginationInfo Pagination { get; set; } = new();
}

public class PaginationOptions
{
    public int Page { get; set; } = 1;

    public int PerPage { get; set; } = 25;

    public Dictionary<string, string> ToQueryParameters()
    {
        var parameters = new Dictionary<string, string>
        {
            ["page"] = Page.ToString(),
            ["per_page"] = PerPage.ToString()
        };
        return parameters;
    }
}

public class SortOptions
{
    public string? Field { get; set; }

    public SortDirection Direction { get; set; } = SortDirection.Ascending;

    public string ToQueryParameter()
    {
        if (string.IsNullOrEmpty(Field))
            return string.Empty;

        var direction = Direction == SortDirection.Ascending ? "asc" : "desc";
        return $"{Field}:{direction}";
    }
}

public enum SortDirection
{
    Ascending,
    Descending
}

public interface IPaginatedList<T>
{
    IReadOnlyList<T> Items { get; }
    PaginationInfo Pagination { get; }
    bool HasNextPage { get; }
    bool HasPreviousPage { get; }
}

public class PaginatedList<T> : IPaginatedList<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public PaginationInfo Pagination { get; set; } = new();
    public bool HasNextPage => Pagination.NextPage.HasValue;
    public bool HasPreviousPage => Pagination.PreviousPage.HasValue;
}