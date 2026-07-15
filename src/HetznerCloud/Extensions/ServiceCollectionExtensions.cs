using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using HetznerCloud.Authentication;

namespace HetznerCloud.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHetznerCloud(
        this IServiceCollection services,
        Action<HetznerCloudClientOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddHttpClient<HetznerCloudClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<HetznerCloudClientOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = options.Timeout;
            })
            .AddHttpMessageHandler<HetznerCloudAuthenticationHandler>();

        services.AddTransient<HetznerCloudAuthenticationHandler>();
        services.AddSingleton<IValidateOptions<HetznerCloudClientOptions>, HetznerCloudClientOptionsValidator>();

        return services;
    }

    public static IServiceCollection AddHetznerCloud(
        this IServiceCollection services,
        string apiToken,
        string? baseUrl = null,
        string? applicationName = null,
        string? applicationVersion = null)
    {
        return services.AddHetznerCloud(options =>
        {
            options.ApiToken = apiToken;
            options.BaseUrl = baseUrl ?? "https://api.hetzner.cloud/v1";
            options.ApplicationName = applicationName ?? "HetznerCloud-Dotnet";
            options.ApplicationVersion = applicationVersion ?? "1.0.0";
        });
    }
}