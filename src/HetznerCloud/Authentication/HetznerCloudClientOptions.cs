using System;
using Microsoft.Extensions.Options;

namespace HetznerCloud.Authentication;

public class HetznerCloudClientOptions
{
    public const string SectionName = "HetznerCloud";

    public string ApiToken { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.hetzner.cloud/v1";

    public string ApplicationName { get; set; } = "HetznerCloud-Dotnet";

    public string ApplicationVersion { get; set; } = "1.0.0";

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

    public int MaxRetries { get; set; } = 3;

    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    public bool ThrowOnError { get; set; } = true;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiToken))
        {
            throw new ArgumentException("API token is required", nameof(ApiToken));
        }

        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Invalid base URL", nameof(BaseUrl));
        }
    }
}

public class HetznerCloudClientOptionsValidator : IValidateOptions<HetznerCloudClientOptions>
{
    public ValidateOptionsResult Validate(string? name, HetznerCloudClientOptions options)
    {
        try
        {
            options.Validate();
            return ValidateOptionsResult.Success;
        }
        catch (Exception ex)
        {
            return ValidateOptionsResult.Fail(ex.Message);
        }
    }
}