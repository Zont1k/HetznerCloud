// Advanced example demonstrating all major features of HetznerCloud .NET library
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HetznerCloud;
using HetznerCloud.Extensions;
using HetznerCloud.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HetznerCloud.AdvancedExample;

public class Program
{
    public static async Task Main(string[] args)
    {
        var apiToken = Environment.GetEnvironmentVariable("HCLOUD_TOKEN") 
            ?? throw new InvalidOperationException("Set HCLOUD_TOKEN environment variable");

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddHetznerCloud(apiToken, "advanced-example", "1.0.0");
                services.AddLogging(builder => builder.AddConsole());
            })
            .Build();

        var client = host.Services.GetRequiredService<HetznerCloudClient>();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("=== Hetzner Cloud .NET Library - Advanced Example ===");

            // 1. List available resources
            await ListResources(client, logger);

            // 2. Create infrastructure with proper error handling
            var server = await CreateServerWithInfrastructure(client, logger);

            // 3. Demonstrate async action handling
            await DemonstrateActionHandling(client, server, logger);

            // 4. Scale and manage resources
            await ManageResources(client, server, logger);

            // 5. Cleanup
            await Cleanup(client, server, logger);
        }
        catch (HetznerCloud.Exceptions.NotFoundException ex)
        {
            logger.LogError(ex, "Resource not found: {Message}", ex.Message);
        }
        catch (HetznerCloud.Exceptions.UnauthorizedException ex)
        {
            logger.LogError(ex, "Authentication failed: {Message}", ex.Message);
        }
        catch (HetznerCloud.Exceptions.RateLimitExceededException ex)
        {
            logger.LogWarning(ex, "Rate limited. Retry after: {RetryAfter}", ex.RetryAfter);
        }
        catch (HetznerCloud.Exceptions.ValidationException ex)
        {
            logger.LogError(ex, "Validation failed: {Errors}", string.Join(", ", ex.ValidationErrors.Select(e => $"{e.Field}: {e.Message}")));
        }
        catch (HetznerCloud.Exceptions.HetznerCloudException ex)
        {
            logger.LogError(ex, "API error ({StatusCode}): {Message}", ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error: {Message}", ex.Message);
        }
        finally
        {
            client.Dispose();
        }
    }

    private static async Task ListResources(HetznerCloudClient client, ILogger logger)
    {
        logger.LogInformation("--- Listing Available Resources ---");

        // Server Types with pricing
        var serverTypes = await client.ServerTypes.GetAllAsync();
        logger.LogInformation("Server Types: {Count}", serverTypes.ServerTypes.Count);
        foreach (var st in serverTypes.ServerTypes.Where(s => !s.Deprecated).Take(5))
        {
            var price = st.Prices.FirstOrDefault(p => p.Location == "fsn1");
            logger.LogInformation("  {Name}: {Cores}vCPU, {Memory}GB RAM, {Disk}GB {Storage} - €{Monthly}/mo",
                st.Name, st.Cores, st.Memory, st.Disk, st.StorageType, price?.PriceMonthly.Net ?? 0);
        }

        // Images
        var images = await client.Images.GetAllAsync(new ImageListOptions { Type = "system", PerPage = 10 });
        logger.LogInformation("System Images: {Count}", images.Images.Count);
        foreach (var img in images.Images.Take(3))
        {
            logger.LogInformation("  {Name} ({Flavor})", img.Name, img.OsFlavor);
        }

        // Locations
        var locations = await client.Locations.GetAllAsync();
        logger.LogInformation("Locations: {Count}", locations.Locations.Count);
        foreach (var loc in locations.Locations)
        {
            logger.LogInformation("  {Name}: {City}, {Country} (Network: {NetworkZone})",
                loc.Name, loc.City, loc.Country, loc.NetworkZone);
        }

        // Datacenters
        var datacenters = await client.Datacenters.GetAllAsync();
        logger.LogInformation("Datacenters: {Count}", datacenters.Datacenters.Count);
    }

    private static async Task<Server> CreateServerWithInfrastructure(HetznerCloudClient client, ILogger logger)
    {
        logger.LogInformation("--- Creating Server with Infrastructure ---");

        // Find required resources
        var serverTypes = await client.ServerTypes.GetAllAsync();
        var serverType = serverTypes.ServerTypes.FirstOrDefault(s => s.Name == "cpx21" && !s.Deprecated);
        
        var images = await client.Images.GetAllAsync(new ImageListOptions { Type = "system", PerPage = 20 });
        var ubuntuImage = images.Images.FirstOrDefault(i => i.Name.StartsWith("ubuntu-24.04"));
        
        var locations = await client.Locations.GetAllAsync();
        var location = locations.Locations.FirstOrDefault(l => l.Name == "fsn1");

        if (serverType == null || ubuntuImage == null || location == null)
        {
            throw new InvalidOperationException("Required resources not found");
        }

        logger.LogInformation("Creating server: {ServerType} with {Image} in {Location}",
            serverType.Name, ubuntuImage.Name, location.Name);

        // Create server with labels and SSH key
        var createResponse = await client.Servers.CreateAsync(new ServerCreateRequest
        {
            Name = $"demo-server-{DateTime.UtcNow:yyyyMMdd-HHmmss}",
            ServerType = serverType.Name,
            Image = ubuntuImage.Name,
            Location = location.Name,
            StartAfterCreate = true,
            Labels = new Dictionary<string, string>
            {
                ["environment"] = "demo",
                ["managed-by"] = "hcloud-dotnet",
                ["created"] = DateTime.UtcNow.ToString("yyyy-MM-dd")
            },
            PublicNet = new PublicNetworkConfig
            {
                EnableIpv4 = true,
                EnableIpv6 = true
            }
        });

        logger.LogInformation("Server created: {Name} (ID: {Id})", createResponse.Server.Name, createResponse.Server.Id);

        // Wait for creation action to complete
        logger.LogInformation("Waiting for server creation to complete...");
        await client.Servers.WaitForActionAsync(createResponse.Action.Id);
        
        // Also wait for any next actions
        if (createResponse.NextActions?.Count > 0)
        {
            await client.Servers.WaitForActionsAsync(createResponse.NextActions.Select(a => a.Id));
        }

        var server = await client.Servers.GetByIdAsync(createResponse.Server.Id);
        logger.LogInformation("Server ready: {Name} - Status: {Status}, IPv4: {IPv4}, IPv6: {IPv6}",
            server.Server.Name, server.Server.Status, 
            server.Server.PublicNet.Ipv4.Ip, server.Server.PublicNet.Ipv6.Ip);

        return server.Server;
    }

    private static async Task DemonstrateActionHandling(HetznerCloudClient client, Server server, ILogger logger)
    {
        logger.LogInformation("--- Demonstrating Action Handling ---");

        // Power off
        logger.LogInformation("Powering off server...");
        var powerOffResponse = await client.Servers.PowerOffAsync(server.Id);
        await client.Servers.WaitForActionAsync(powerOffResponse.Action.Id);
        logger.LogInformation("Server powered off");

        // Power on
        logger.LogInformation("Powering on server...");
        var powerOnResponse = await client.Servers.PowerOnAsync(server.Id);
        await client.Servers.WaitForActionAsync(powerOnResponse.Action.Id);
        logger.LogInformation("Server powered on");

        // Reboot
        logger.LogInformation("Rebooting server...");
        var rebootResponse = await client.Servers.RebootAsync(server.Id);
        await client.Servers.WaitForActionAsync(rebootResponse.Action.Id);
        logger.LogInformation("Server rebooted");

        // Check action history
        var actions = await client.Actions.GetAllAsync(new ActionListOptions 
        { 
            ResourceId = server.Id, 
            ResourceType = "server",
            PerPage = 10 
        });
        logger.LogInformation("Recent actions for server {ServerId}:", server.Id);
        foreach (var action in actions.Actions.Take(5))
        {
            logger.LogInformation("  {Id}: {Command} - {Status} ({Progress}%)", 
                action.Id, action.Command, action.Status, action.Progress);
        }
    }

    private static async Task ManageResources(HetznerCloudClient client, Server server, ILogger logger)
    {
        logger.LogInformation("--- Resource Management ---");

        // Create a volume and attach
        logger.LogInformation("Creating and attaching volume...");
        var volumeResponse = await client.Volumes.CreateAsync(new VolumeCreateRequest
        {
            Name = $"data-volume-{server.Id}",
            Size = 50,
            Location = server.Location.Name,
            Format = "ext4",
            Automount = true,
            Labels = new Dictionary<string, string> { ["server"] = server.Name }
        });
        await client.Volumes.WaitForActionAsync(volumeResponse.Action.Id);
        
        var attachResponse = await client.Volumes.AttachAsync(volumeResponse.Volume.Id, 
            new VolumeAttachRequest { Server = server.Id, Automount = true });
        await client.Volumes.WaitForActionAsync(attachResponse.Action.Id);
        logger.LogInformation("Volume attached: {VolumeId}", volumeResponse.Volume.Id);

        // Create a floating IP and assign
        logger.LogInformation("Creating and assigning floating IP...");
        var floatingIpResponse = await client.FloatingIps.CreateAsync(new FloatingIpCreateRequest
        {
            Type = FloatingIpType.Ipv4,
            Description = $"floating-ip-for-{server.Name}",
            HomeLocation = server.Location.Name,
            Labels = new Dictionary<string, string> { ["server"] = server.Name }
        });
        
        var assignResponse = await client.FloatingIps.AssignAsync(floatingIpResponse.FloatingIp.Id,
            new FloatingIpAssignRequest { Server = server.Id });
        await client.FloatingIps.WaitForActionAsync(assignResponse.Action.Id);
        logger.LogInformation("Floating IP assigned: {IP}", floatingIpResponse.FloatingIp.Ip);

        // Create a network and attach server
        logger.LogInformation("Creating network and attaching server...");
        var networkResponse = await client.Networks.CreateAsync(new NetworkCreateRequest
        {
            Name = $"app-network-{server.Id}",
            IpRange = "10.0.0.0/16",
            Labels = new Dictionary<string, string> { ["environment"] = "demo" },
            Subnets = new List<NetworkSubnetCreateRequest>
            {
                new() { Type = NetworkSubnetType.Cloud, NetworkZone = "eu-central", IpRange = "10.0.1.0/24" }
            }
        });
        
        var subnetAttachResponse = await client.Networks.AddSubnetAsync(networkResponse.Network.Id,
            new NetworkAddSubnetRequest 
            { 
                Subnet = new NetworkSubnetCreateRequest 
                { 
                    Type = NetworkSubnetType.Cloud, 
                    NetworkZone = "eu-central", 
                    IpRange = "10.0.2.0/24" 
                } 
            });
        await client.Networks.WaitForActionAsync(subnetAttachResponse.Actions[0].Id);

        var attachNetworkResponse = await client.Servers.AttachToNetworkAsync(server.Id,
            new ServerAttachToNetworkRequest 
            { 
                Network = networkResponse.Network.Id, 
                Ip = "10.0.1.10" 
            });
        await client.Servers.WaitForActionAsync(attachNetworkResponse.Action.Id);
        logger.LogInformation("Network attached: {NetworkId}", networkResponse.Network.Id);

        // Change protection
        logger.LogInformation("Enabling delete protection...");
        await client.Servers.ChangeProtectionAsync(server.Id, new ServerChangeProtectionRequest 
        { 
            Delete = true, 
            Rebuild = false 
        });
        logger.LogInformation("Delete protection enabled");

        // Demonstrate pagination
        logger.LogInformation("Demonstrating pagination...");
        var allServers = new List<Server>();
        var page = 1;
        while (true)
        {
            var serverPage = await client.Servers.GetAllAsync(new ServerListOptions 
            { 
                Page = page, 
                PerPage = 10,
                Sort = "created:desc"
            });
            allServers.AddRange(serverPage.Servers);
            
            if (!serverPage.HasNextPage)
                break;
                
            page++;
        }
        logger.LogInformation("Total servers across all pages: {Count}", allServers.Count);

        // List actions with filtering
        var runningActions = await client.Actions.GetAllAsync(new ActionListOptions
        {
            Status = "running",
            PerPage = 20
        });
        logger.LogInformation("Currently running actions: {Count}", runningActions.Actions.Count);
    }

    private static async Task Cleanup(HetznerCloudClient client, Server server, ILogger logger)
    {
        logger.LogInformation("--- Cleanup ---");

        // Detach and delete volume
        var volumes = await client.Volumes.GetAllAsync(new VolumeListOptions 
        { 
            LabelSelector = $"server={server.Name}" 
        });
        foreach (var volume in volumes.Volumes)
        {
            logger.LogInformation("Detaching and deleting volume {VolumeId}...", volume.Id);
            await client.Volumes.DetachAsync(volume.Id);
            await client.Volumes.DeleteAsync(volume.Id);
        }

        // Unassign and delete floating IP
        var floatingIps = await client.FloatingIps.GetAllAsync(new FloatingIpListOptions
        {
            LabelSelector = $"server={server.Name}"
        });
        foreach (var fip in floatingIps.FloatingIps)
        {
            if (fip.Server != null)
            {
                logger.LogInformation("Unassigning floating IP {IP}...", fip.Ip);
                await client.FloatingIps.UnassignAsync(fip.Id);
            }
            logger.LogInformation("Deleting floating IP {IP}...", fip.Ip);
            await client.FloatingIps.DeleteAsync(fip.Id);
        }

        // Detach and delete network
        var networks = await client.Networks.GetAllAsync(new NetworkListOptions
        {
            LabelSelector = "environment=demo"
        });
        foreach (var network in networks.Networks)
        {
            // Detach server first
            logger.LogInformation("Detaching server from network {NetworkId}...", network.Id);
            // Note: In practice, you'd use the server's detach from network action
            
            logger.LogInformation("Deleting network {NetworkId}...", network.Id);
            await client.Networks.DeleteAsync(network.Id);
        }

        // Delete server
        logger.LogInformation("Deleting server {ServerId}...", server.Id);
        await client.Servers.DeleteAsync(server.Id);
        logger.LogInformation("Cleanup complete");
    }
}