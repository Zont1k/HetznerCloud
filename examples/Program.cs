using System;
using System.Threading.Tasks;
using HetznerCloud;
using HetznerCloud.Extensions;
using HetznerCloud.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HetznerCloud.Example;

public class Program
{
    public static async Task Main(string[] args)
    {
        var apiToken = Environment.GetEnvironmentVariable("HCLOUD_TOKEN") ?? "your-api-token";

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddHetznerCloud(apiToken, "my-app", "1.0.0");
            })
            .Build();

        var client = host.Services.GetRequiredService<HetznerCloudClient>();

        Console.WriteLine("Hetzner Cloud .NET Library Example");
        Console.WriteLine("==================================");

        await ListServerTypes(client);
        await ListImages(client);
        await ListLocations(client);
    }

    private static async Task ListServerTypes(HetznerCloudClient client)
    {
        Console.WriteLine("\n--- Server Types ---");
        var response = await client.ServerTypes.GetAllAsync();
        foreach (var type in response.ServerTypes)
        {
            Console.WriteLine($"  {type.Name}: {type.Cores} vCPU, {type.Memory} GB RAM, {type.Disk} GB Disk ({type.StorageType})");
        }
    }

    private static async Task ListImages(HetznerCloudClient client)
    {
        Console.WriteLine("\n--- System Images ---");
        var response = await client.Images.GetAllAsync(new ImageListOptions { Type = "system", PerPage = 10 });
        foreach (var image in response.Images)
        {
            Console.WriteLine($"  {image.Name} ({image.OsFlavor} {image.OsVersion}) - {image.Description}");
        }
    }

    private static async Task ListLocations(HetznerCloudClient client)
    {
        Console.WriteLine("\n--- Locations ---");
        var response = await client.Locations.GetAllAsync();
        foreach (var location in response.Locations)
        {
            Console.WriteLine($"  {location.Name}: {location.City}, {location.Country} (Network Zone: {location.NetworkZone})");
        }
    }

    private static async Task CreateServerExample(HetznerCloudClient client)
    {
        Console.WriteLine("\n--- Create Server Example (Dry Run) ---");
        
        var serverTypes = await client.ServerTypes.GetAllAsync();
        var cpx22 = serverTypes.ServerTypes.Find(t => t.Name == "cpx22");
        
        var images = await client.Images.GetAllAsync(new ImageListOptions { Type = "system", PerPage = 10 });
        var ubuntu = images.Images.Find(i => i.Name.StartsWith("ubuntu-24.04"));
        
        var locations = await client.Locations.GetAllAsync();
        var hel1 = locations.Locations.Find(l => l.Name == "hel1");

        if (cpx22 == null || ubuntu == null || hel1 == null)
        {
            Console.WriteLine("Could not find required resources for example");
            return;
        }

        Console.WriteLine($"Would create server:");
        Console.WriteLine($"  Type: {cpx22.Name} ({cpx22.Cores} vCPU, {cpx22.Memory} GB RAM)");
        Console.WriteLine($"  Image: {ubuntu.Name}");
        Console.WriteLine($"  Location: {hel1.Name}");

        var request = new ServerCreateRequest
        {
            Name = "example-server",
            ServerType = cpx22.Name,
            Image = ubuntu.Name,
            Location = hel1.Name,
            StartAfterCreate = true,
            Labels = new Dictionary<string, string>
            {
                { "environment", "example" },
                { "created-by", "hetznercloud-dotnet" }
            }
        };

        Console.WriteLine("\nTo actually create the server, uncomment the following code:");
        Console.WriteLine("""
            var createResponse = await client.Servers.CreateAsync(request);
            Console.WriteLine($"Server created: {createResponse.Server.Name} (ID: {createResponse.Server.Id})");
            
            // Wait for the action to complete
            await client.Servers.WaitForActionAsync(createResponse.Action.Id);
            Console.WriteLine("Server is ready!");
        """);
    }
}