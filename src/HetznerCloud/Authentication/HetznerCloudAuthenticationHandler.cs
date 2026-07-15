using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace HetznerCloud.Authentication;

public class HetznerCloudAuthenticationHandler : DelegatingHandler
{
    private readonly HetznerCloudClientOptions _options;

    public HetznerCloudAuthenticationHandler(IOptions<HetznerCloudClientOptions> options)
    {
        _options = options.Value;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiToken);

        var userAgent = $"{_options.ApplicationName}/{_options.ApplicationVersion} (+https://github.com/hetznercloud/hcloud-dotnet)";
        request.Headers.UserAgent.ParseAdd(userAgent);

        return base.SendAsync(request, cancellationToken);
    }
}

public static class AuthenticationHeaderValueExtensions
{
    public static void ParseAdd(this HttpHeaderValueCollection<AuthenticationHeaderValue> headers, string value)
    {
        if (AuthenticationHeaderValue.TryParse(value, out var headerValue))
        {
            headers.Add(headerValue);
        }
    }
}