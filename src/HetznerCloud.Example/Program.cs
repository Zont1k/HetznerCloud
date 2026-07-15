using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HetznerCloud;
using HetznerCloud.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var apiToken = Environment.GetEnvironmentVariable("HCLOUD_TOKEN") ?? "your-api-token";
        
        services.AddHetznerCloud(apiToken, applicationName: "HetznerCloud-Example", applicationVersion: "1.0.0");
        
        services.AddLogging(builder => builder.AddConsole());
    })
    .Build();

var client = host.Services.GetRequiredService<HetznerCloudClient>();

try
{
    Console.WriteLine("=== Hetzner Cloud .NET Library Example ===\n");

    await ListServers(client);
    await ListServerTypes(client);
    await ListImages(client);
    await ListLocations(client);
    await ListVolumes(client);
    await ListLoadBalancers(client);
    await ListFloatingIps(client);
    await ListNetworks(client);
    await ListSshKeys(client);
    await ListPlacementGroups(client);
    await ListFirewalls(client);
    await ListCertificates(client);
    await ListIsoImages(client);
    await GetPricing(client);

    // Example: Create a server
    // await CreateServerExample(client);
}
catch (HetznerCloud.Exceptions.HetznerCloudException ex)
{
    Console.WriteLine($"API Error: {ex.Message} (Status: {ex.StatusCode}, Code: {ex.ErrorCode})");
    if (ex.Details?.Count > 0)
    {
        foreach (var detail in ex.Details)
        {
            Console.WriteLine($"  - {detail.Field}: {detail.Code} - {detail.Message}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
finally
{
    client.Dispose();
}

static async Task ListServers(HetznerCloudClient client)
{
    Console.WriteLine("--- Servers ---");
    var response = await client.Servers.GetAllAsync(new() { PerPage = 5 });
    foreach (var server in response.Servers)
    {
        Console.WriteLine($"  {server.Id}: {server.Name} ({server.Status}) - {server.ServerType.Name} in {server.Location.Name}");
    }
    Console.WriteLine($"  Total: {response.Meta.Pagination.TotalEntries}\n");
}

static async Task ListServerTypes(HetznerCloudClient client)
{
    Console.WriteLine("--- Server Types ---");
    var response = await client.ServerTypes.GetAllAsync(new() { PerPage = 10 });
    foreach (var type in response.ServerTypes)
    {
        Console.WriteLine($"  {type.Name}: {type.Cores} vCPU, {type.Memory} GB RAM, {type.Disk} GB {type.StorageType} - €{type.Prices.FirstOrDefault()?.PriceMonthly.Net}/month");
    }
    Console.WriteLine();
}

static async Task ListImages(HetznerCloudClient client)
{
    Console.WriteLine("--- Images ---");
    var response = await client.Images.GetAllAsync(new() { PerPage = 10, Type = "system" });
    foreach (var image in response.Images)
    {
        Console.WriteLine($"  {image.Id}: {image.Name} ({image.Type}) - {image.OsFlavor} {image.OsVersion ?? ""}");
    }
    Console.WriteLine($"  Total: {response.Meta.Pagination.TotalEntries}\n");
}

static async Task ListLocations(HetznerCloudClient client)
{
    Console.WriteLine("--- Locations ---");
    var response = await client.Locations.GetAllAsync();
    foreach (var location in response.Locations)
    {
        Console.WriteLine($"  {location.Name}: {location.Description} ({location.Country}) - {location.NetworkZone}");
    }
    Console.WriteLine();
}

static async Task ListVolumes(HetznerCloudClient client)
{
    Console.WriteLine("--- Volumes ---");
    var response = await client.Volumes.GetAllAsync(new() { PerPage = 5 });
    foreach (var volume in response.Volumes)
    {
        Console.WriteLine($"  {volume.Id}: {volume.Name} ({volume.Size} GB) - {volume.Format} - {volume.Location.Name}");
    }
    Console.WriteLine($"  Total: {response.Meta.Pagination.TotalEntries}\n");
}

static async Task ListLoadBalancers(HetznerCloudClient client)
{
    Console.WriteLine("--- Load Balancers ---");
    var response = await client.LoadBalancers.GetAllAsync(new() { PerPage = 5 });
    foreach (var lb in response.LoadBalancers)
    {
        Console.WriteLine($"  {lb.Id}: {lb.Name} ({lb.LoadBalancerType.Name}) - {lb.PublicNet.Enabled ? "Public" : "Private"} - {lb.Location.Name}");
    }
    Console.WriteLine($"  Total: {response.Meta.Pagination.TotalEntries}\n");
}

static async Task ListFloatingIps(HetznerCloudClient client)
{
    Console.WriteLine("--- Floating IPs ---");
    var response = await client.FloatingIps.GetAllAsync(new() { PerPage = 5 });
    foreach (var ip in response.FloatingIps)
    {
        Console.WriteLine($"  {ip.Id}: {ip.Ip} ({ip.Type}) - {ip.Description} - {ip.HomeLocation.Name} - Server: {ip.Server?.Name ?? "none"}");
    }
    Console.WriteLine($"  Total: {response.Meta.Pagination.TotalEntries}\n");
}

static async Task ListNetworks(HetznerCloudClient client)
{
    Console.WriteLine("--- Networks ---");
    var response = await client.Networks.GetAllAsync(new() { PerPage = 5 });
    foreach (var network in response.Networks)
    {
        Console.WriteLine($"  {network.Id}: {network.Name} - {network.IpRange} - {network.Subnets.Count} subnets");
    }
    Console.WriteLine($"  Total: {response.Meta.Pagination.TotalEntries}\n");
}

static async Task ListSshKeys(HetznerCloudClient client)
{
    Console.WriteLine("--- SSH Keys ---");
    var response = await client.SSHKeys.GetAllAsync(new() { PerPage = 5 });
    foreach (var key in response.SshKeys)
    {
        Console.WriteLine($"  {key.Id}: {key.Name} - {key.Fingerprint[..16]}...");
    }
    Console.WriteLine($"  Total: {response.Meta.Pagination.TotalEntries}\n");
}

static async Task ListPlacementGroups(HetznerCloudClient client)
{
    Console.WriteLine("--- Placement Groups ---");
    var response = await client.PlacementGroups.GetAllAsync(new() { PerPage = 5 });
    foreach (var pg in response.PlacementGroups)
    {
        Console.WriteLine($"  {pg.Id}: {pg.Name} ({pg.Type}) - {pg.Servers.Count} servers");
    }
    Console.WriteLine($"  Total: {response.Meta.Pagination.TotalEntries}\n");
}

static async Task ListFirewalls(HetznerCloudClient client)
{
    Console.WriteLine("--- Firewalls ---");
    var response = await client.Firewalls.GetAllAsync(new() { PerPage = 5 });
    foreach (var fw in response.Firewalls)
    {
        Console.WriteLine($"  {fw.Id}: {fw.Name} - {fw.Rules.Count} rules - {fw.AppliedTo.Count} resources");
    }
    Console.WriteLine($"  Total: {response.Meta.Pagination.TotalEntries}\n");
}

static async Task ListCertificates(HetznerCloudClient client)
{
    Console.WriteLine("--- Certificates ---");
    var response = await client.Certificates.GetAllAsync(new() { PerPage = 5 });
    foreach (var cert in response.Certificates)
    {
        Console.WriteLine($"  {cert.Id}: {cert.Name} ({cert.Type}) - {string.Join(", ", cert.DomainNames)} - Expires: {cert.NotValidAfter:yyyy-MM-dd}");
    }
    Console.WriteLine($"  Total: {response.Meta.Pagination.TotalEntries}\n");
}

static async Task ListIsoImages(HetznerCloudClient client)
{
    Console.WriteLine("--- ISO Images ---");
    var response = await client.IsoImages.GetAllAsync(new() { PerPage = 10 });
    foreach (var iso in response.Isos)
    {
        Console.WriteLine($"  {iso.Id}: {iso.Name} ({iso.Type}) - {iso.Description}");
    }
    Console.WriteLine($"  Total: {response.Meta.Pagination.TotalEntries}\n");
}

static async Task GetPricing(HetznerCloudClient client)
{
    Console.WriteLine("--- Pricing ---");
    var response = await client.Pricing.GetAsync();
    Console.WriteLine($"  Currency: {response.Pricing.Currency}");
    var samplePrices = response.Pricing.Prices.Where(p => p.Location == "fsn1").Take(5);
    foreach (var price in samplePrices)
    {
        Console.WriteLine($"  {price.Location}: Monthly €{price.PriceMonthly.Net}, Hourly €{price.PriceHourly.Net}");
    }
    Console.WriteLine();
}

static async Task CreateServerExample(HetznerCloudClient client)
{
    Console.WriteLine("--- Create Server Example ---");
    
    var serverTypes = await client.ServerTypes.GetAllAsync(new() { PerPage = 1 });
    var serverType = serverTypes.ServerTypes.FirstOrDefault(s => s.Name.Contains("cpx21"));
    
    var images = await client.Images.GetAllAsync(new() { PerPage = 1, Type = "system" });
    var image = images.Images.FirstOrDefault(i => i.Name.Contains("ubuntu-24.04"));
    
    var locations = await client.Locations.GetAllAsync();
    var location = locations.Locations.FirstOrDefault(l => l.Name == "fsn1");
    
    if (serverType == null || image == null || location == null)
    {
        Console.WriteLine("  Could not find required resources for example");
        return;
    }

    var request = new ServerCreateRequest
    {
        Name = "example-server",
        ServerType = serverType.Name,
        Image = image.Name,
        Location = location.Name,
        StartAfterCreate = true,
        Labels = new Dictionary<string, string>
        {
            ["environment"] = "development",
            ["created-by"] = "hcloud-dotnet-example"
        }
    };

    Console.WriteLine($"  Creating server: {request.Name} ({request.ServerType}) with {request.Image} in {request.Location}");
    
    var response = await client.Servers.CreateAsync(request);
    
    Console.WriteLine($"  Server created: {response.Server.Id} - {response.Server.Name}");
    Console.WriteLine($"  Action: {response.Action.Id} ({response.Action.Command}) - {response.Action.Status}");
    
    if (response.NextActions?.Count > 0)
    {
        Console.WriteLine("  Waiting for actions to complete...");
        var actions = await client.Actions.WaitForAllAsync(response.NextActions.Select(a => a.Id));
        foreach (var action in actions)
        {
            Console.WriteLine($"  Action {action.Id} ({action.Command}): {action.Status}");
        }
    }
    
    Console.WriteLine("  Server is ready!");
}