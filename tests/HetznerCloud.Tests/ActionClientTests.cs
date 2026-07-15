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

public class ActionClientTests
{
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly HetznerCloudClientOptions _options;
    private readonly ILogger<HetznerCloudClient> _logger;

    public ActionClientTests()
    {
        _httpHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_httpHandlerMock.Object);
        _options = new HetznerCloudClientOptions
        {
            ApiToken = "test-token",
            BaseUrl = "https://api.hetzner.cloud/v1",
            MaxRetries = 0
        };
        _logger = Mock.Of<ILogger<HetznerCloudClient>>();
    }

    private HetznerCloudClient CreateClient() => new(_httpClient, Options.Create(_options), _logger);

    [Fact]
    public async Task WaitForAsync_Success_ReturnsCompletedAction()
    {
        var actionId = 123L;
        var runningAction = new Models.Action { Id = actionId, Status = ActionStatus.Running, Progress = 50 };
        var completedAction = new Models.Action { Id = actionId, Status = ActionStatus.Success, Progress = 100 };

        var runningResponse = new ActionResponse { Action = runningAction };
        var completedResponse = new ActionResponse { Action = completedAction };

        var callCount = 0;
        _httpHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(CreateResponse(runningResponse))
            .ReturnsAsync(CreateResponse(completedResponse));

        var client = CreateClient();
        var result = await client.Actions.WaitForAsync(actionId, CancellationToken.None);

        Assert.Equal(actionId, result.Action.Id);
        Assert.Equal(ActionStatus.Success, result.Action.Status);
    }

    [Fact]
    public async Task WaitForAsync_Error_ThrowsHetznerCloudException()
    {
        var actionId = 456L;
        var errorAction = new Models.Action 
        { 
            Id = actionId, 
            Status = ActionStatus.Error, 
            Error = new ActionError { Code = "server_not_found", Message = "Server not found" } 
        };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(CreateResponse(new ActionResponse { Action = errorAction }));

        var client = CreateClient();
        var ex = await Assert.ThrowsAsync<HetznerCloudException>(() => 
            client.Actions.WaitForAsync(actionId, CancellationToken.None));

        Assert.Equal("action_failed", ex.ErrorCode);
        Assert.Contains("Server not found", ex.Message);
    }

    [Fact]
    public async Task WaitForAllAsync_MultipleActions_WaitsForAll()
    {
        var actionIds = new[] { 1L, 2L, 3L };
        var completedAction = new Models.Action { Status = ActionStatus.Success, Progress = 100 };

        var callCount = 0;
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(() =>
            {
                callCount++;
                return Task.FromResult(CreateResponse(new ActionResponse { Action = completedAction }));
            });

        var client = CreateClient();
        var results = await client.Actions.WaitForAllAsync(actionIds, CancellationToken.None);

        Assert.Equal(3, results.Count);
        Assert.All(results, a => Assert.Equal(ActionStatus.Success, a.Status));
        Assert.Equal(3, callCount);
    }

    private HttpResponseMessage CreateResponse(object content)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(content), System.Text.Encoding.UTF8, "application/json")
        };
    }
}

public class ServerClientTests
{
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly HetznerCloudClientOptions _options;
    private readonly ILogger<HetznerCloudClient> _logger;

    public ServerClientTests()
    {
        _httpHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_httpHandlerMock.Object);
        _options = new HetznerCloudClientOptions { ApiToken = "test-token", BaseUrl = "https://api.hetzner.cloud/v1", MaxRetries = 0 };
        _logger = Mock.Of<ILogger<HetznerCloudClient>>();
    }

    private HetznerCloudClient CreateClient() => new(_httpClient, Options.Create(_options), _logger);

    [Fact]
    public async Task GetAllAsync_ReturnsServers()
    {
        var servers = new List<Server>
        {
            new() { Id = 1, Name = "server-1", Status = ServerStatus.Running },
            new() { Id = 2, Name = "server-2", Status = ServerStatus.Off }
        };
        var response = new ServerListResponse { Servers = servers, Meta = new PaginationMeta { Pagination = new PaginationInfo { Page = 1, PerPage = 25, TotalEntries = 2 } } };

        SetupGetResponse("/servers", response);

        var client = CreateClient();
        var result = await client.Servers.GetAllAsync();

        Assert.Equal(2, result.Servers.Count);
        Assert.Equal("server-1", result.Servers[0].Name);
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreateResponse()
    {
        var createResponse = new ServerCreateResponse
        {
            Server = new Server { Id = 100, Name = "new-server" },
            Action = new Models.Action { Id = 200, Status = ActionStatus.Running }
        };

        SetupPostResponse("/servers", createResponse, HttpStatusCode.Created);

        var client = CreateClient();
        var request = new ServerCreateRequest { Name = "new-server", ServerType = "cx11", Image = "ubuntu-24.04" };
        var result = await client.Servers.CreateAsync(request);

        Assert.Equal(100, result.Server.Id);
        Assert.Equal("new-server", result.Server.Name);
        Assert.Equal(200, result.Action.Id);
    }

    [Fact]
    public async Task PowerOnAsync_ReturnsActionResponse()
    {
        var actionResponse = new ServerActionResponse
        {
            Action = new Models.Action { Id = 300, Command = "poweron", Status = ActionStatus.Running }
        };

        SetupPostResponse("/servers/123/actions/poweron", actionResponse);

        var client = CreateClient();
        var result = await client.Servers.PowerOnAsync(123);

        Assert.Equal(300, result.Action.Id);
        Assert.Equal("poweron", result.Action.Command);
    }

    private void SetupGetResponse(string path, object response)
    {
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.AbsolutePath.Contains(path)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(response), System.Text.Encoding.UTF8, "application/json")
            });
    }

    private void SetupPostResponse(string path, object response, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri!.AbsolutePath.Contains(path)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(JsonSerializer.Serialize(response), System.Text.Encoding.UTF8, "application/json")
            });
    }
}

public class RetryPolicyTests
{
    [Fact]
    public async Task RetryPolicy_RetriesOnTransientErrors()
    {
        var httpHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(httpHandlerMock.Object);
        
        var options = new HetznerCloudClientOptions
        {
            ApiToken = "test-token",
            BaseUrl = "https://api.hetzner.cloud/v1",
            MaxRetries = 3,
            RetryDelay = TimeSpan.FromMilliseconds(10)
        };
        
        var logger = Mock.Of<ILogger<HetznerCloudClient>>();
        var client = new HetznerCloudClient(httpClient, Options.Create(options), logger);

        var attempt = 0;
        httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(() =>
            {
                attempt++;
                if (attempt < 3)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"server_types":[],"meta":{"pagination":{"page":1,"per_page":25,"total_entries":0}}}""", 
                        System.Text.Encoding.UTF8, "application/json")
                });
            });

        var result = await client.ServerTypes.GetAllAsync();
        
        Assert.Equal(3, attempt);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task RetryPolicy_ThrowsAfterMaxRetries()
    {
        var httpHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(httpHandlerMock.Object);
        
        var options = new HetznerCloudClientOptions
        {
            ApiToken = "test-token",
            BaseUrl = "https://api.hetzner.cloud/v1",
            MaxRetries = 2,
            RetryDelay = TimeSpan.FromMilliseconds(10)
        };
        
        var logger = Mock.Of<ILogger<HetznerCloudClient>>();
        var client = new HetznerCloudClient(httpClient, Options.Create(options), logger);

        httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        await Assert.ThrowsAsync<ServerErrorException>(() => client.ServerTypes.GetAllAsync());
    }
}