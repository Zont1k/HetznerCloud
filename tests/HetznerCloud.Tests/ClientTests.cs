using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HetznerCloud;
using HetznerCloud.Authentication;
using HetznerCloud.Exceptions;
using HetznerCloud.Models;
using HetznerCloud.Pagination;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using Models = HetznerCloud.Models;

namespace HetznerCloud.Tests;

public class HetznerCloudClientTests
{
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly HetznerCloudClientOptions _options;
    private readonly ILogger<HetznerCloudClient> _logger;

    public HetznerCloudClientTests()
    {
        _httpHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_httpHandlerMock.Object);
        _options = new HetznerCloudClientOptions
        {
            ApiToken = "test-token",
            BaseUrl = "https://api.hetzner.cloud/v1",
            ApplicationName = "test-app",
            ApplicationVersion = "1.0.0",
            Timeout = TimeSpan.FromSeconds(30),
            MaxRetries = 0,
            ThrowOnError = true
        };
        _logger = Mock.Of<ILogger<HetznerCloudClient>>();
    }

    private HetznerCloudClient CreateClient() => new(_httpClient, Options.Create(_options), _logger);

    [Fact]
    public void Constructor_WithValidOptions_CreatesClient()
    {
        var client = CreateClient();
        
        Assert.NotNull(client.Servers);
        Assert.NotNull(client.ServerTypes);
        Assert.NotNull(client.Images);
        Assert.NotNull(client.Locations);
        Assert.NotNull(client.Datacenters);
        Assert.NotNull(client.Volumes);
        Assert.NotNull(client.LoadBalancers);
        Assert.NotNull(client.FloatingIps);
        Assert.NotNull(client.Networks);
        Assert.NotNull(client.Actions);
        Assert.NotNull(client.SSHKeys);
        Assert.NotNull(client.Certificates);
        Assert.NotNull(client.PlacementGroups);
        Assert.NotNull(client.Firewalls);
        Assert.NotNull(client.IsoImages);
        Assert.NotNull(client.Pricing);
    }

    [Fact]
    public void Constructor_WithNullApiToken_ThrowsArgumentException()
    {
        var invalidOptions = new HetznerCloudClientOptions
        {
            ApiToken = "",
            BaseUrl = "https://api.hetzner.cloud/v1"
        };

        var ex = Assert.Throws<ArgumentException>(() => 
            new HetznerCloudClient(_httpClient, Options.Create(invalidOptions), _logger));
        
        Assert.Contains("API token is required", ex.Message);
    }

    [Fact]
    public void Constructor_WithInvalidBaseUrl_ThrowsArgumentException()
    {
        var invalidOptions = new HetznerCloudClientOptions
        {
            ApiToken = "token",
            BaseUrl = "not-a-valid-url"
        };

        var ex = Assert.Throws<ArgumentException>(() => 
            new HetznerCloudClient(_httpClient, Options.Create(invalidOptions), _logger));
        
        Assert.Contains("Invalid base URL", ex.Message);
    }

    [Fact]
    public async Task GetAsync_Success_ReturnsDeserializedObject()
    {
        var expectedServer = new Server { Id = 123, Name = "test-server", Status = ServerStatus.Running };
        var response = new ServerResponse { Server = expectedServer };
        var json = JsonSerializer.Serialize(response);

        SetupHttpResponse(HttpStatusCode.OK, json);

        var client = CreateClient();
        var result = await client.GetAsync<ServerResponse>("/servers/123");

        Assert.NotNull(result);
        Assert.Equal(123, result.Server.Id);
        Assert.Equal("test-server", result.Server.Name);
    }

    [Fact]
    public async Task GetAsync_NotFound_ThrowsNotFoundException()
    {
        SetupHttpResponse(HttpStatusCode.NotFound, """{"error": {"code": "not_found", "message": "Server not found"}}""");

        var client = CreateClient();
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => 
            client.GetAsync<ServerResponse>("/servers/999"));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Equal("not_found", ex.ErrorCode);
    }

    [Fact]
    public async Task GetAsync_Unauthorized_ThrowsUnauthorizedException()
    {
        SetupHttpResponse(HttpStatusCode.Unauthorized, """{"error": {"code": "unauthorized", "message": "Invalid token"}}""");

        var client = CreateClient();
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => 
            client.GetAsync<ServerResponse>("/servers/123"));

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.Equal("unauthorized", ex.ErrorCode);
    }

    [Fact]
    public async Task GetAsync_RateLimited_ThrowsRateLimitExceededException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent("""{"error": {"code": "rate_limit_exceeded", "message": "Rate limit exceeded"}}"""),
            Headers = { RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(60)) }
        };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var client = CreateClient();
        var ex = await Assert.ThrowsAsync<RateLimitExceededException>(() => 
            client.GetAsync<ServerResponse>("/servers/123"));

        Assert.Equal(HttpStatusCode.TooManyRequests, ex.StatusCode);
        Assert.Equal("rate_limit_exceeded", ex.ErrorCode);
        Assert.Equal(TimeSpan.FromSeconds(60), ex.RetryAfter);
    }

    [Fact]
    public async Task PostAsync_Success_ReturnsDeserializedObject()
    {
        var createResponse = new ServerCreateResponse 
        { 
            Server = new Server { Id = 456, Name = "new-server" },
            Action = new Models.Action { Id = 789, Status = ActionStatus.Success }
        };
        var json = JsonSerializer.Serialize(createResponse);

        SetupHttpResponse(HttpStatusCode.Created, json, HttpMethod.Post);

        var client = CreateClient();
        var request = new ServerCreateRequest { Name = "new-server", ServerType = "cx11", Image = "ubuntu-24.04" };
        var result = await client.PostAsync<ServerCreateResponse, ServerCreateRequest>("/servers", request);

        Assert.NotNull(result);
        Assert.Equal(456, result.Server.Id);
        Assert.Equal("new-server", result.Server.Name);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content, HttpMethod? method = null)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
        };

        var methodMatcher = method ?? HttpMethod.Get;
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == methodMatcher),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }
}

public class PaginationTests
{
    [Fact]
    public void PaginationOptions_ToQueryParameters_ReturnsCorrectValues()
    {
        var options = new PaginationOptions { Page = 2, PerPage = 50 };
        var parameters = options.ToQueryParameters();

        Assert.Equal("2", parameters["page"]);
        Assert.Equal("50", parameters["per_page"]);
    }

    [Fact]
    public void SortOptions_ToQueryParameter_ReturnsFormattedString()
    {
        var sort = new SortOptions { Field = "created", Direction = SortDirection.Descending };
        Assert.Equal("created:desc", sort.ToQueryParameter());
    }

    [Fact]
    public void SortOptions_WithNullField_ReturnsEmptyString()
    {
        var sort = new SortOptions { Field = null };
        Assert.Equal(string.Empty, sort.ToQueryParameter());
    }
}

public class ExceptionTests
{
    [Fact]
    public void HetznerCloudException_StoresAllProperties()
    {
        var details = new List<ErrorDetail> { new() { Field = "name", Code = "invalid", Message = "Name is required" } };
        var ex = new HetznerCloudException("Validation failed", HttpStatusCode.UnprocessableEntity, "validation_error", details);

        Assert.Equal("Validation failed", ex.Message);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, ex.StatusCode);
        Assert.Equal("validation_error", ex.ErrorCode);
        Assert.Single(ex.Details);
        Assert.Equal("name", ex.Details[0].Field);
    }

    [Fact]
    public void ValidationException_StoresValidationErrors()
    {
        var errors = new List<ErrorDetail> { new() { Field = "email", Code = "invalid", Message = "Invalid email" } };
        var ex = new ValidationException("Invalid input", errors);

        Assert.Equal("Invalid input", ex.Message);
        Assert.Single(ex.ValidationErrors);
    }

    [Fact]
    public void RateLimitExceededException_StoresRetryAfter()
    {
        var retryAfter = TimeSpan.FromSeconds(120);
        var ex = new RateLimitExceededException("Rate limited", retryAfter);

        Assert.Equal(retryAfter, ex.RetryAfter);
    }
}

public class SerializationTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    [Fact]
    public void Server_SerializesCorrectly()
    {
        var server = new Server
        {
            Id = 1,
            Name = "test",
            Status = ServerStatus.Running,
            Labels = new Dictionary<string, string> { ["env"] = "prod" },
            ServerType = new ServerType { Name = "cx11", Cores = 1, Memory = 2 }
        };

        var json = JsonSerializer.Serialize(server, _options);
        var deserialized = JsonSerializer.Deserialize<Server>(json, _options);

        Assert.Equal(server.Id, deserialized.Id);
        Assert.Equal(server.Name, deserialized.Name);
        Assert.Equal(server.Status, deserialized.Status);
        Assert.Equal(server.Labels["env"], deserialized.Labels["env"]);
    }

    [Fact]
    public void ServerCreateRequest_SerializesWithoutNulls()
    {
        var request = new ServerCreateRequest
        {
            Name = "test",
            ServerType = "cx11",
            Image = "ubuntu-24.04",
            Location = "fsn1",
            Labels = new Dictionary<string, string> { ["env"] = "test" }
        };

        var json = JsonSerializer.Serialize(request, _options);
        
        Assert.Contains("test", json);
        Assert.Contains("cx11", json);
        Assert.Contains("ubuntu-24.04", json);
        Assert.Contains("fsn1", json);
        Assert.Contains("env", json);
    }

    [Fact]
    public void Enum_SerializesWithJsonPropertyName()
    {
        var status = ServerStatus.Running;
        var json = JsonSerializer.Serialize(status, _options);
        // JsonStringEnumConverter uses camelCase by default
        Assert.Equal("\"running\"", json.ToLowerInvariant());

        var deserialized = JsonSerializer.Deserialize<ServerStatus>("\"running\"", _options);
        Assert.Equal(ServerStatus.Running, deserialized);
    }
}

public class ModelValidationTests
{
    [Fact]
    public void ServerCreateRequest_WithRequiredFields_IsValid()
    {
        var request = new ServerCreateRequest
        {
            Name = "valid-name",
            ServerType = "cx11",
            Image = "ubuntu-24.04"
        };

        Assert.Equal("valid-name", request.Name);
        Assert.Equal("cx11", request.ServerType);
        Assert.Equal("ubuntu-24.04", request.Image);
        Assert.True(request.StartAfterCreate);
    }

    [Fact]
    public void VolumeCreateRequest_WithSizeAndLocation_IsValid()
    {
        var request = new VolumeCreateRequest
        {
            Name = "my-volume",
            Size = 100,
            Location = "fsn1",
            Format = "ext4",
            Automount = true
        };

        Assert.Equal(100, request.Size);
        Assert.Equal("fsn1", request.Location);
        Assert.Equal("ext4", request.Format);
        Assert.True(request.Automount);
    }

    [Fact]
    public void LoadBalancerCreateRequest_WithServicesAndTargets_IsValid()
    {
        var request = new LoadBalancerCreateRequest
        {
            Name = "lb-01",
            LoadBalancerType = "lb11",
            Location = "fsn1",
            Services = new List<LoadBalancerServiceCreateRequest>
            {
                new()
                {
                    Protocol = LoadBalancerServiceProtocol.Http,
                    ListenPort = 80,
                    DestinationPort = 8080,
                    HealthCheck = new LoadBalancerHealthCheckCreateRequest
                    {
                        Protocol = LoadBalancerHealthCheckProtocol.Http,
                        Port = 8080,
                        Interval = 15,
                        Timeout = 10,
                        Retries = 3
                    }
                }
            },
            Targets = new List<LoadBalancerTargetCreateRequest>
            {
                new()
                {
                    Type = LoadBalancerTargetType.Server,
                    Server = 12345,
                    UsePrivateIp = true
                }
            }
        };

        Assert.Single(request.Services);
        Assert.Single(request.Targets);
        Assert.Equal(LoadBalancerServiceProtocol.Http, request.Services[0].Protocol);
        Assert.Equal(LoadBalancerTargetType.Server, request.Targets[0].Type);
    }
}