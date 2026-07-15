# HetznerCloud .NET

A .NET client library for the [Hetzner Cloud API](https://docs.hetzner.cloud), inspired by the official [hcloud-go](https://github.com/hetznercloud/hcloud-go) and [hcloud-python](https://github.com/hetznercloud/hcloud-python) libraries.

[![CI](https://github.com/Zont1k/HetznerCloud/actions/workflows/ci.yml/badge.svg)](https://github.com/Zont1k/HetznerCloud/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/HetznerCloud.svg)](https://www.nuget.org/packages/HetznerCloud/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/HetznerCloud.svg)](https://www.nuget.org/packages/HetznerCloud/)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-blueviolet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

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

var servers = await client.Servers.GetAllAsync();
foreach (var server in servers.Servers)
    Console.WriteLine($"{server.Name} — {server.Status}");
```

## Supported Resources

| Client | Operations |
|--------|-----------|
| `Servers` | CRUD, power on/off, reboot, reset, rebuild, change type, protection, backup, rescue, networks, ISOs |
| `Volumes` | CRUD, attach, detach, protection |
| `LoadBalancers` | CRUD, services, targets, algorithm, protection |
| `Networks` | CRUD, subnets, routes, IP range, protection |
| `FloatingIps` | CRUD, assign, unassign, DNS ptr |
| `Firewalls` | CRUD, rules, apply/remove resources, protection |
| `SSHKeys` | CRUD |
| `Certificates` | CRUD |
| `PlacementGroups` | CRUD |
| `Images` | List, get |
| `ServerTypes` | List, get |
| `Locations` | List, get |
| `Datacenters` | List, get |
| `Actions` | List, get, wait for completion |
| `Pricing` | Get full pricing breakdown |
| `PrimaryIps` | CRUD, assign, unassign, DNS ptr, protection |
| `DnsZones` | CRUD, DNS records, zone protection |
| `StorageBoxes` | List, get, update, SSH/WebDAV/Samba toggle, password change |

## Configuration

```csharp
services.AddHetznerCloud(options =>
{
    options.ApiToken = Environment.GetEnvironmentVariable("HCLOUD_TOKEN")!;
    options.MaxRetries = 3;
    options.RetryDelay = TimeSpan.FromSeconds(1);
});
```

## Error Handling

```csharp
try
{
    var server = await client.Servers.GetByIdAsync(999999);
}
catch (NotFoundException)        { /* 404 */ }
catch (UnauthorizedException)    { /* 401 */ }
catch (RateLimitExceededException) { /* 429 */ }
catch (HetznerCloudException ex) { /* other */ }
```

## Features

- **18 resource clients** — full Hetzner Cloud API coverage
- **Dependency Injection** — `services.AddHetznerCloud()`
- **Resilience** — retry, circuit breaker, hedging via Polly
- **OpenTelemetry** — metrics and distributed tracing
- **Typed errors** — exception hierarchy for each status code
- **Fluent builders** — type-safe request construction
- **Action polling** — `WaitForAsync` / `WaitForAllAsync`
- **Pagination** — built-in page/limit/label filtering
- **Branded IDs** — `ServerId`, `VolumeId`, `PrimaryIpId`, etc.

## Requirements

- .NET 8.0 or .NET 9.0
- A [Hetzner Cloud API token](https://console.hetzner.cloud/#/api-tokens)

## License

[MIT](LICENSE)
