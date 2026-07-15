using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using HetznerCloud.Models;
using HetznerCloud.Serialization;

namespace HetznerCloud.Benchmarks;

/// <summary>
/// Benchmarks for JSON serialization performance
/// </summary>
[MemoryDiagnoser]
[SimpleJob(iterationCount: 10, warmupCount: 3)]
public class SerializationBenchmarks
{
    private readonly JsonSerializerOptions _defaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    private readonly JsonSerializerOptions _contextOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        TypeInfoResolver = HetznerCloudJsonContext.Default
    };

    private ServerListResponse _serverListResponse = null!;
    private VolumeListResponse _volumeListResponse = null!;
    private LoadBalancerListResponse _lbListResponse = null!;
    private string _serverListJson = string.Empty;
    private string _volumeListJson = string.Empty;
    private string _lbListJson = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        // Create test data
        _serverListResponse = CreateServerListResponse(100);
        _volumeListResponse = CreateVolumeListResponse(50);
        _lbListResponse = CreateLoadBalancerListResponse(20);

        // Serialize to JSON strings for deserialization benchmarks
        _serverListJson = JsonSerializer.Serialize(_serverListResponse, _defaultOptions);
        _volumeListJson = JsonSerializer.Serialize(_volumeListResponse, _defaultOptions);
        _lbListJson = JsonSerializer.Serialize(_lbListResponse, _defaultOptions);
    }

    [Benchmark(Baseline = true)]
    public string Serialize_ServerList_DefaultOptions()
    {
        return JsonSerializer.Serialize(_serverListResponse, _defaultOptions);
    }

    [Benchmark]
    public string Serialize_ServerList_ContextOptions()
    {
        return JsonSerializer.Serialize(_serverListResponse, _contextOptions);
    }

    [Benchmark]
    public ServerListResponse Deserialize_ServerList_DefaultOptions()
    {
        return JsonSerializer.Deserialize<ServerListResponse>(_serverListJson, _defaultOptions)!;
    }

    [Benchmark]
    public ServerListResponse Deserialize_ServerList_ContextOptions()
    {
        return JsonSerializer.Deserialize<ServerListResponse>(_serverListJson, _contextOptions)!;
    }

    [Benchmark]
    public string Serialize_VolumeList_DefaultOptions()
    {
        return JsonSerializer.Serialize(_volumeListResponse, _defaultOptions);
    }

    [Benchmark]
    public string Serialize_VolumeList_ContextOptions()
    {
        return JsonSerializer.Serialize(_volumeListResponse, _contextOptions);
    }

    [Benchmark]
    public VolumeListResponse Deserialize_VolumeList_DefaultOptions()
    {
        return JsonSerializer.Deserialize<VolumeListResponse>(_volumeListJson, _defaultOptions)!;
    }

    [Benchmark]
    public VolumeListResponse Deserialize_VolumeList_ContextOptions()
    {
        return JsonSerializer.Deserialize<VolumeListResponse>(_volumeListJson, _contextOptions)!;
    }

    [Benchmark]
    public string Serialize_LBList_DefaultOptions()
    {
        return JsonSerializer.Serialize(_lbListResponse, _defaultOptions);
    }

    [Benchmark]
    public string Serialize_LBList_ContextOptions()
    {
        return JsonSerializer.Serialize(_lbListResponse, _contextOptions);
    }

    [Benchmark]
    public LoadBalancerListResponse Deserialize_LBList_DefaultOptions()
    {
        return JsonSerializer.Deserialize<LoadBalancerListResponse>(_lbListJson, _defaultOptions)!;
    }

    [Benchmark]
    public LoadBalancerListResponse Deserialize_LBList_ContextOptions()
    {
        return JsonSerializer.Deserialize<LoadBalancerListResponse>(_lbListJson, _contextOptions)!;
    }

    [Benchmark]
    public async Task<string> SerializeAsync_ServerList_DefaultOptions()
    {
        using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, _serverListResponse, _defaultOptions);
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    [Benchmark]
    public async Task<string> SerializeAsync_ServerList_ContextOptions()
    {
        using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, _serverListResponse, _contextOptions);
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    [Benchmark]
    public async Task<ServerListResponse> DeserializeAsync_ServerList_DefaultOptions()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_serverListJson));
        return await JsonSerializer.DeserializeAsync<ServerListResponse>(stream, _defaultOptions)!;
    }

    [Benchmark]
    public async Task<ServerListResponse> DeserializeAsync_ServerList_ContextOptions()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_serverListJson));
        return await JsonSerializer.DeserializeAsync<ServerListResponse>(stream, _contextOptions)!;
    }

    private ServerListResponse CreateServerListResponse(int count)
    {
        var servers = new List<Server>();
        for (int i = 0; i < count; i++)
        {
            servers.Add(new Server
            {
                Id = i + 1,
                Name = $"server-{i}",
                Status = i % 3 == 0 ? ServerStatus.Running : ServerStatus.Off,
                Created = DateTime.UtcNow.AddDays(-i),
                PublicNet = new PublicNetwork
                {
                    Ipv4 = new Ipv4Network { Ip = $"192.168.{i % 255}.{i % 255}", Blocked = false },
                    Ipv6 = new Ipv6Network { Ip = $"2a01:4f8:{i:x}:1::1", Blocked = false },
                    FloatingIps = new List<long> { 1000 + i }
                },
                ServerType = new ServerType
                {
                    Id = i % 10,
                    Name = $"cx{i % 10 + 1}1",
                    Cores = (i % 4 + 1) * 2,
                    Memory = (i % 4 + 1) * 4.0,
                    Disk = (i % 4 + 1) * 20,
                    StorageType = "ssd",
                    CpuType = "shared",
                    Architecture = Architecture.X86,
                    IncludedTraffic = 20_000_000_000
                },
                Location = new Location
                {
                    Id = i % 5 + 1,
                    Name = $"fsn{i % 5 + 1}",
                    Description = $"Falkenstein DC{i % 5 + 1}",
                    Country = "DE",
                    City = "Falkenstein",
                    NetworkZone = "eu-central"
                },
                Labels = new Dictionary<string, string>
                {
                    ["environment"] = i % 3 == 0 ? "prod" : "staging",
                    ["team"] = $"team-{i % 4}"
                },
                Protection = new ServerProtection { Delete = i % 5 == 0, Rebuild = false }
            });
        }

        return new ServerListResponse
        {
            Servers = servers,
            Meta = new PaginationMeta
            {
                Pagination = new PaginationInfo
                {
                    Page = 1,
                    PerPage = count,
                    TotalEntries = count,
                    LastPage = 1
                }
            }
        };
    }

    private VolumeListResponse CreateVolumeListResponse(int count)
    {
        var volumes = new List<Volume>();
        for (int i = 0; i < count; i++)
        {
            volumes.Add(new Volume
            {
                Id = i + 1,
                Name = $"volume-{i}",
                Description = $"Volume {i} for testing",
                Location = new Location
                {
                    Id = i % 5 + 1,
                    Name = $"fsn{i % 5 + 1}",
                    Country = "DE",
                    City = "Falkenstein"
                },
                Size = (i % 5 + 1) * 100,
                Format = i % 2 == 0 ? "ext4" : "xfs",
                Protection = new VolumeProtection { Delete = i % 3 == 0 },
                Labels = new Dictionary<string, string> { ["env"] = "test" },
                Created = DateTime.UtcNow.AddDays(-i)
            });
        }

        return new VolumeListResponse
        {
            Volumes = volumes,
            Meta = new PaginationMeta
            {
                Pagination = new PaginationInfo
                {
                    Page = 1,
                    PerPage = count,
                    TotalEntries = count,
                    LastPage = 1
                }
            }
        };
    }

    private LoadBalancerListResponse CreateLoadBalancerListResponse(int count)
    {
        var lbs = new List<LoadBalancer>();
        for (int i = 0; i < count; i++)
        {
            lbs.Add(new LoadBalancer
            {
                Id = i + 1,
                Name = $"lb-{i}",
                Labels = new Dictionary<string, string> { ["env"] = "prod" },
                LoadBalancerType = new LoadBalancerType
                {
                    Id = i % 3 + 1,
                    Name = $"lb{i % 3 + 1}",
                    Description = $"Load Balancer Type {i % 3 + 1}",
                    MaxConnections = 20000,
                    MaxServices = 5,
                    MaxTargets = 100
                },
                Location = new Location
                {
                    Id = i % 5 + 1,
                    Name = $"fsn{i % 5 + 1}",
                    Country = "DE",
                    City = "Falkenstein"
                },
                NetworkZone = "eu-central",
                PublicNet = new LoadBalancerPublicNet
                {
                    Enabled = true,
                    Ipv4 = new LoadBalancerPublicNetIpv4 { Ip = $"1.2.{i % 255}.{i % 255}", Blocked = false },
                    Ipv6 = new LoadBalancerPublicNetIpv6 { Ip = $"2a01:4f8::{i:x}::1", Blocked = false }
                },
                Algorithm = new LoadBalancerAlgorithm { Type = LoadBalancerAlgorithmType.RoundRobin },
                Services = new List<LoadBalancerService>
                {
                    new()
                    {
                        Protocol = LoadBalancerServiceProtocol.Http,
                        ListenPort = 80,
                        DestinationPort = 8080,
                        HealthCheck = new LoadBalancerHealthCheck
                        {
                            Protocol = LoadBalancerHealthCheckProtocol.Http,
                            Port = 8080,
                            Interval = 15,
                            Timeout = 10,
                            Retries = 3,
                            Http = new LoadBalancerHealthCheckHttp { Path = "/health", StatusCodes = ["2??"] }
                        }
                    }
                },
                Targets = new List<LoadBalancerTarget>
                {
                    new()
                    {
                        Type = LoadBalancerTargetType.Server,
                        Server = new ResourceReference { Id = i + 1, Type = "server" }
                    }
                }
            });
        }

        return new LoadBalancerListResponse
        {
            LoadBalancers = lbs,
            Meta = new PaginationMeta
            {
                Pagination = new PaginationInfo
                {
                    Page = 1,
                    PerPage = count,
                    TotalEntries = count,
                    LastPage = 1
                }
            }
        };
    }
}

/// <summary>
/// Benchmarks for branded ID operations
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class BrandedIdBenchmarks
{
    private readonly long[] _rawIds = new long[1000];
    private readonly ServerId[] _serverIds = new ServerId[1000];
    private readonly VolumeId[] _volumeIds = new VolumeId[1000];

    [GlobalSetup]
    public void Setup()
    {
        var rand = new Random(42);
        for (int i = 0; i < 1000; i++)
        {
            _rawIds[i] = rand.NextInt64(1, long.MaxValue);
            _serverIds[i] = new ServerId(_rawIds[i]);
            _volumeIds[i] = new VolumeId(_rawIds[i]);
        }
    }

    [Benchmark]
    public void CreateServerIds()
    {
        for (int i = 0; i < 1000; i++)
        {
            var id = new ServerId(_rawIds[i]);
        }
    }

    [Benchmark]
    public void ImplicitToLong()
    {
        long sum = 0;
        for (int i = 0; i < 1000; i++)
        {
            sum += _serverIds[i];
        }
    }

    [Benchmark]
    public void ExplicitFromLong()
    {
        for (int i = 0; i < 1000; i++)
        {
            var id = (ServerId)_rawIds[i];
        }
    }

    [Benchmark]
    public void CompareServerIds()
    {
        int count = 0;
        for (int i = 0; i < 999; i++)
        {
            if (_serverIds[i] == _serverIds[i + 1]) count++;
        }
    }

    [Benchmark]
    public void ToString()
    {
        for (int i = 0; i < 1000; i++)
        {
            var s = _serverIds[i].ToString();
        }
    }

    [Benchmark]
    public void SerializeServerIds()
    {
        var options = new JsonSerializerOptions { WriteIndented = false };
        for (int i = 0; i < 1000; i++)
        {
            var json = JsonSerializer.Serialize(_serverIds[i], options);
        }
    }

    [Benchmark]
    public void DeserializeServerIds()
    {
        var options = new JsonSerializerOptions();
        for (int i = 0; i < 1000; i++)
        {
            var json = JsonSerializer.Serialize(_serverIds[i]);
            var id = JsonSerializer.Deserialize<ServerId>(json);
        }
    }
}

/// <summary>
/// Benchmarks for Result/Option pattern operations
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class ResultOptionBenchmarks
{
    private const int Iterations = 10000;

    [Benchmark]
    public int Result_Success_Map()
    {
        var result = Result<int, string>.Success(42);
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var r = result.Map(x => x * 2);
            if (r.IsSuccess) sum += r.Value;
        }
        return sum;
    }

    [Benchmark]
    public int Result_Failure_Map()
    {
        var result = Result<int, string>.Failure("error");
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var r = result.Map(x => x * 2);
            if (r.IsFailure) sum++;
        }
        return sum;
    }

    [Benchmark]
    public int Option_Some_Map()
    {
        var opt = Option<int>.Some(42);
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var o = opt.Map(x => x * 2);
            if (o.HasValue) sum += o.Value;
        }
        return sum;
    }

    [Benchmark]
    public int Option_None_Map()
    {
        var opt = Option<int>.None;
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var o = opt.Map(x => x * 2);
            if (o.IsNone) sum++;
        }
        return sum;
    }

    [Benchmark]
    public int Result_Bind()
    {
        var result = Result<int, string>.Success(10);
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var r = result.Bind(x => Result<int, string>.Success(x + 5));
            if (r.IsSuccess) sum += r.Value;
        }
        return sum;
    }

    [Benchmark]
    public int Result_Match()
    {
        var result = Result<int, string>.Success(42);
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            sum += result.Match(x => x, _ => 0);
        }
        return sum;
    }
}