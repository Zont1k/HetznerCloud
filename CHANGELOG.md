# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2026-07-15

### Added
- **Primary IPs** — Full CRUD + assign/unassign/change_dns_ptr/change_protection (`client.PrimaryIps`)
- **DNS Zones** — Full CRUD + DNS records management + zone protection (`client.DnsZones`)
- **Storage Boxes** — List/get/update + SSH/WebDAV/Samba toggle + password change (`client.StorageBoxes`)
- **Load Balancer UpdateService** — Wire `LoadBalancerUpdateServiceRequest` to `UpdateServiceAsync`
- **Load Balancer HTTP idle_timeout** — Configurable idle timeout (30–300s) on HTTP services
- **Server Rebuild user_data** — Pass `user_data` during server rebuild
- **Network Route Actions** — `AddRouteAsync` / `DeleteRouteAsync` on networks
- **Firewall Protection** — `FirewallProtection` model + `ChangeProtectionAsync`
- **ISO GetByIdAsync** — Fetch single ISO by ID
- **Structured Pricing** — Full pricing breakdown: server_types, volumes, LBs, floating_ips, images, primary_ips, networks, certificates, firewalls, storage_boxes, labels
- **Branded ID types** — `PrimaryIpId`, `DnsZoneId`, `StorageBoxId` with JSON converters
- **Comprehensive README** — Full API reference, examples, exception hierarchy

### Fixed
- **Duplicate model definitions removed** — `ServerUpdateRequest`, `LoadBalancerUpdateRequest`, `NetworkUpdateRequest`, `FirewallUpdateRequest` now defined once in model files
- **Duplicate `Build()` method** in `LoadBalancerTargetCreateBuilder` — removed
- **`ProxyProtocol` reference** — removed from `LoadBalancerServiceCreateBuilder` (property doesn't exist on request model)
- **`CookieName` reference** — removed from `LoadBalancerServiceHttpBuilder` (belongs on `StickySessions`, not HTTP create request)
- **XML comment `<T,E>` syntax** — escaped to `&lt;T,E&gt;` in `Result.cs`
- **`CallerArgumentExpression("this")`** — replaced with standard parameter in `Option<T>.GetValueOrThrow`
- **Nullable dereference warnings** — fixed in `Resilience.cs` (null-conditional on `_telemetry`)
- **`ServerCreateImageRequest` hiding base members** — removed duplicate `Description`/`Labels` properties
- **`FirewallAppliedTo.LabelSelector` typing** — corrected to use proper selector object
- **LoadBalancerClient.cs** — Wired `UpdateServiceAsync` method
- **NetworkClient.cs** — Wired `AddRouteAsync` / `DeleteRouteAsync` methods
- **FirewallClient.cs** — Wired `ChangeProtectionAsync` method
- **IsoImageClient.cs** — Added `GetByIdAsync` method
- **PrimaryIpClient.cs** — Fixed `UnassignAsync` signature to match interface
- **DnsZoneClient.cs** — Fixed `BuildQueryString` using correct options type for records

### Changed
- **`Pricing` model** — Replaced flat `List<Price>` with structured sub-objects (`ImagePricing`, `ServerTypePricing`, `VolumePricing`, etc.)
- **`Result<IDisposable>`** → **`Result<IDisposable, Exception>`** in Telemetry (correct type parameter count)
- **`Firewall` model** — Added `Protection` property with `change` and `delete` fields

## [1.0.0] - 2024-07-14

### Added
- Initial release of Hetzner Cloud .NET library
- Full API coverage for all Hetzner Cloud resources:
  - **Servers**: Create, manage, power actions, rebuild, rescue mode, ISO mounting, backups, DNS PTR
  - **Server Types**: List with pricing and availability
  - **Images**: System images, snapshots, backups, apps with filtering
  - **Volumes**: Block storage with attach/detach/resize
  - **Load Balancers**: Services, targets, algorithms, health checks, SSL certificates
  - **Networks**: Private networks, subnets, routes, VSwitch integration
  - **Floating IPs**: IPv4/IPv6 assignment, DNS PTR, protection
  - **SSH Keys**: Key management for server access
  - **Certificates**: Uploaded and managed SSL/TLS certificates
  - **Placement Groups**: Spread placement for high availability
  - **Firewalls**: Rules, resource attachment, label selectors
  - **ISO Images**: Public and private ISO images for rescue/install
  - **Actions**: Async operation tracking with polling helpers
  - **Pricing**: Current pricing for all resources
  - **Locations & Datacenters**: Available locations with network zones
- **Dependency Injection**: `services.AddHetznerCloud()` extension
- **Authentication**: Bearer token with custom User-Agent
- **Retry Policies**: Automatic retry with exponential backoff (Polly)
- **Rate Limit Handling**: Automatic retry-after for 429 responses
- **Pagination**: Built-in support for all list operations
- **Strongly-typed Models**: Request/response DTOs with JSON serialization
- **Error Handling**: Typed exceptions for all HTTP status codes
- **Target Frameworks**: .NET 8.0 and .NET 9.0
- **Source Link**: Debugging support with embedded source

### Security
- All API communication over HTTPS
- Token never logged or exposed in exceptions
- Input validation on all request models
