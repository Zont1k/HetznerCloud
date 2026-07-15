# HetznerCloud .NET

A comprehensive, production-ready .NET client library for the [Hetzner Cloud API](https://docs.hetzner.cloud), modeled after the official [hcloud-go](https://github.com/hetznercloud/hcloud-go) and [hcloud-python](https://github.com/hetznercloud/hcloud-python) libraries.

[![Build](https://img.shields.io/badge/build-passing-brightgreen)]()
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-blueviolet)]()
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Features

- **18 Resource Clients** — Full coverage of Hetzner Cloud API endpoints including Primary IPs, DNS Zones, and Storage Boxes
- **Strongly-Typed Models** — All request/response models with `System.Text.Json` serialization
- **Async/Await** — Fully asynchronous API with `CancellationToken` support
- **Dependency Injection** — Built-in `services.AddHetznerCloud()` registration
- **Resilience** — Automatic retry with exponential backoff, circuit breakers, and hedging via Polly
- **Rate Limiting** — Automatic detection and retry-after handling
- **Pagination** — Built-in support for all list operations with metadata
- **Action Polling** — `WaitForAsync` / `WaitForAllAsync` for long-running operations
- **Error Handling** — Typed exception hierarchy (`NotFoundException`, `ValidationException`, `RateLimitExceededException`, etc.)
- **Fluent Builders** — Type-safe builders for complex resource creation
- **OpenTelemetry** — Built-in metrics and distributed tracing
- **Branded IDs** — Compile-time safe ID types (`ServerId`, `VolumeId`, `PrimaryIpId`, etc.)

## Installation

```bash
dotnet add package HetznerCloud
```

## Quick Start

```csharp
using HetznerCloud;
using HetznerCloud.Models;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddHetznerCloud("your-api-token");
var provider = services.BuildServiceProvider();

var client = provider.GetRequiredService<HetznerCloudClient>();

// List all servers
var servers = await client.Servers.GetAllAsync();
foreach (var server in servers.Servers)
    Console.WriteLine($"{server.Name} — {server.Status}");

// Create a server
var create = await client.Servers.CreateAsync(new ServerCreateRequest
{
    Name = "my-server",
    ServerType = "cpx22",
    Image = "ubuntu-24.04",
    Location = "fsn1",
    StartAfterCreate = true,
    Labels = new() { ["env"] = "production" }
});

await client.Servers.WaitForActionAsync(create.Action.Id);
```

## Configuration

```csharp
services.AddHetznerCloud(options =>
{
    options.ApiToken = Environment.GetEnvironmentVariable("HCLOUD_TOKEN")!;
    options.ApplicationName = "my-app";
    options.ApplicationVersion = "1.0.0";
    options.Timeout = TimeSpan.FromSeconds(100);
    options.MaxRetries = 3;
    options.RetryDelay = TimeSpan.FromSeconds(1);
    options.ThrowOnError = true; // throw exceptions on API errors (default: true)
});
```

## API Reference

### Servers

| Method | Description |
|--------|-------------|
| `GetAllAsync(options?)` | List servers with pagination, sort, label filtering |
| `GetByIdAsync(id)` | Get a single server by ID |
| `GetByNameAsync(name)` | Get a server by name |
| `CreateAsync(request)` | Create a new server |
| `UpdateAsync(id, request)` | Update server name/labels |
| `DeleteAsync(id)` | Delete a server |
| `PowerOnAsync(id)` | Power on a server |
| `PowerOffAsync(id)` | Graceful power off |
| `RebootAsync(id)` | Graceful reboot |
| `ResetAsync(id)` | Hard reset (power cycle) |
| `RebuildAsync(id, request)` | Rebuild server (supports `user_data`) |
| `ChangeTypeAsync(id, request)` | Change server type |
| `ChangeProtectionAsync(id, request)` | Enable/disable delete/rebuild protection |
| `EnableBackupAsync(id, request)` | Enable automatic backups |
| `DisableBackupAsync(id)` | Disable automatic backups |
| `ChangeDnsPtrAsync(id, request)` | Set PTR record (reverse DNS) |
| `AttachToNetworkAsync(id, request)` | Attach server to a private network |
| `DetachFromNetworkAsync(id, request)` | Detach server from a private network |
| `CreateImageAsync(id, request)` | Create snapshot/image from server |
| `ReassignIpAsync(id, request)` | Reassign primary IP |
| `ResetPasswordAsync(id)` | Reset root password |
| `EnableRescueAsync(id, request)` | Boot into rescue mode |
| `DisableRescueAsync(id)` | Exit rescue mode |
| `ChangeAliasIpsAsync(id, request)` | Change alias IPs on network interface |
| `AttachIsoAsync(id, request)` | Attach an ISO image |
| `DetachIsoAsync(id)` | Detach an ISO image |

### Primary IPs

| Method | Description |
|--------|-------------|
| `GetAllAsync(options?)` | List primary IPs |
| `GetByIdAsync(id)` | Get by ID |
| `GetByNameAsync(name)` | Get by name |
| `CreateAsync(request)` | Create a new primary IP |
| `UpdateAsync(id, request)` | Update name/labels |
| `DeleteAsync(id)` | Delete (must be unassigned first) |
| `AssignAsync(id, request)` | Assign to a server/network |
| `UnassignAsync(id)` | Unassign from current resource |
| `ChangeDnsPtrAsync(id, request)` | Update PTR record |
| `ChangeProtectionAsync(id, request)` | Enable/disable deletion protection |

### DNS Zones

| Method | Description |
|--------|-------------|
| `GetAllAsync(options?)` | List all DNS zones |
| `GetByIdAsync(id)` | Get zone by ID |
| `CreateAsync(request)` | Create a new DNS zone |
| `UpdateAsync(id, request)` | Update zone name/TTL |
| `DeleteAsync(id)` | Delete a zone |
| `GetAllRecordsAsync(zoneId, options?)` | List records in a zone |
| `GetRecordByIdAsync(zoneId, recordId)` | Get a specific record |
| `CreateRecordAsync(zoneId, request)` | Create a DNS record |
| `UpdateRecordAsync(zoneId, recordId, request)` | Update a DNS record |
| `DeleteRecordAsync(zoneId, recordId)` | Delete a DNS record |
| `ChangeProtectionAsync(zoneId, request)` | Change zone protection |

### Storage Boxes

| Method | Description |
|--------|-------------|
| `GetAllAsync(options?)` | List storage boxes |
| `GetByIdAsync(id)` | Get by ID |
| `GetByNameAsync(name)` | Get by name |
| `UpdateAsync(id, request)` | Update name/labels |
| `ChangePasswordAsync(id, request)` | Change access password |
| `EnableSshAsync(id)` / `DisableSshAsync(id)` | Toggle SSH access |
| `EnableWebdavAsync(id)` / `DisableWebdavAsync(id)` | Toggle WebDAV access |
| `EnableSambaAsync(id)` / `DisableSambaAsync(id)` | Toggle Samba access |

### Load Balancers

| Method | Description |
|--------|-------------|
| `GetAllAsync` / `GetByIdAsync` / `GetByNameAsync` | CRUD operations |
| `CreateAsync` / `UpdateAsync` / `DeleteAsync` | |
| `AddServiceAsync` / `UpdateServiceAsync` / `DeleteServiceAsync` | Manage HTTP/TCP services |
| `AddTargetAsync` / `RemoveTargetAsync` | Manage targets |
| `ChangeAlgorithmAsync` | Change balancing algorithm |
| `ChangeTypeAsync` | Change LB type |
| `EnablePublicInterfaceAsync` / `DisablePublicInterfaceAsync` | Toggle public interface |
| `AttachToNetworkAsync` / `DetachFromNetworkAsync` | Network attachment |
| `ChangeProtectionAsync` / `ChangeIpAsync` | |

> **HTTP Services** support `idle_timeout` (30–300 seconds), sticky sessions, redirect, and certificate configuration.

### Networks

| Method | Description |
|--------|-------------|
| CRUD | `GetAllAsync` / `GetByIdAsync` / `GetByNameAsync` / `CreateAsync` / `UpdateAsync` / `DeleteAsync` |
| `AddSubnetAsync` / `DeleteSubnetAsync` | Manage subnets |
| `AddRouteAsync` / `DeleteRouteAsync` | Manage routes |
| `ChangeIpRangeAsync` | Change IP range |
| `ChangeProtectionAsync` | Enable/disable deletion protection |

### Floating IPs

| Method | Description |
|--------|-------------|
| CRUD | `GetAllAsync` / `GetByIdAsync` / `CreateAsync` / `UpdateAsync` / `DeleteAsync` |
| `AssignAsync` / `UnassignAsync` | Assign/unassign to server |
| `ChangeDnsPtrAsync` | Update PTR record |
| `ChangeProtectionAsync` | |

### Firewalls

| Method | Description |
|--------|-------------|
| CRUD | `GetAllAsync` / `GetByIdAsync` / `GetByNameAsync` / `CreateAsync` / `UpdateAsync` / `DeleteAsync` |
| `SetRulesAsync` | Replace firewall rules |
| `ApplyToResourcesAsync` / `RemoveFromResourcesAsync` | Attach/detach to servers/networks/labels |
| `ChangeProtectionAsync` | Enable/disable change/delete protection |

### Other Resources

| Client | Description |
|--------|-------------|
| `client.SSHKeys` | SSH key management (CRUD) |
| `client.Certificates` | SSL/TLS certificates (upload or managed) |
| `client.PlacementGroups` | Server placement groups for spread distribution |
| `client.IsoImages` | ISO images for rescue/install modes |
| `client.Images` | OS images, snapshots, backups, apps |
| `client.ServerTypes` | Server type information and pricing |
| `client.Locations` | Datacenter locations |
| `client.Datacenters` | Individual datacenter details |
| `client.Actions` | Track async operations (`GetAllAsync`, `WaitForAsync`) |
| `client.Pricing` | Full pricing breakdown (servers, volumes, LBs, floating IPs, images, primary IPs, networks, certificates, firewalls, storage boxes) |

## Waiting for Actions

```csharp
// Single action
var action = await client.Actions.WaitForAsync(actionId);

// Multiple actions
await client.Actions.WaitForAllAsync(new[] { action1.Id, action2.Id, action3.Id });

// Via server client
await client.Servers.WaitForActionAsync(createResponse.Action.Id);
```

## Pagination

```csharp
var response = await client.Servers.GetAllAsync(new ServerListOptions
{
    Page = 1,
    PerPage = 50,
    Sort = "created:desc",
    LabelSelector = "env=production"
});

Console.WriteLine($"Page {response.Meta.Pagination.Page} / {response.Meta.Pagination.LastPage}");
Console.WriteLine($"Total: {response.Meta.Pagination.TotalEntries}");
```

## Error Handling

```csharp
try
{
    var server = await client.Servers.GetByIdAsync(999999);
}
catch (NotFoundException ex)
{
    Console.WriteLine($"Not found: {ex.Message}");
}
catch (UnauthorizedException)
{
    Console.WriteLine("Invalid API token");
}
catch (ValidationException ex)
{
    foreach (var error in ex.ValidationErrors)
        Console.WriteLine($"  {error.Field}: {error.Message}");
}
catch (RateLimitExceededException ex)
{
    Console.WriteLine($"Rate limited. Retry after: {ex.RetryAfter}");
}
catch (ConflictException ex)
{
    Console.WriteLine($"Conflict: {ex.Message}");
}
catch (HetznerCloudException ex)
{
    Console.WriteLine($"API error ({(int)ex.StatusCode}): {ex.Message}");
}
```

## Fluent Builders

```csharp
var request = ServerCreateBuilder.Create()
    .WithName("web-server")
    .WithServerType("cpx31")
    .WithImage("ubuntu-24.04")
    .WithLocation("fsn1")
    .WithLabels(b => b.Add("env", "production"))
    .Build();
```

## OpenTelemetry

```csharp
services.AddHetznerCloud("token");
services.AddOpenTelemetry()
    .WithTracing(b => b.AddSource("HetznerCloud"));
```

## Exception Hierarchy

```
HetznerCloudException
├── NotFoundException (404)
├── UnauthorizedException (401)
├── ForbiddenException (403)
├── ValidationException (422)
├── ConflictException (409)
├── RateLimitExceededException (429)
├── ServerErrorException (500)
└── ServiceUnavailableException (503)
```

## Requirements

- .NET 8.0 or .NET 9.0
- A [Hetzner Cloud API token](https://console.hetzner.cloud/#/api-tokens)

## License

[MIT](LICENSE)

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## Related Projects

- [hcloud-go](https://github.com/hetznercloud/hcloud-go) — Official Go library
- [hcloud-python](https://github.com/hetznercloud/hcloud-python) — Official Python library
- [Hetzner Cloud API Docs](https://docs.hetzner.cloud)
- [API Changelog](https://status.hetzner.cloud/?xml)
