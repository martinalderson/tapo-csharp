using System.Text.Json.Serialization;

namespace TapoCSharp.Cli.Models;

public class DeviceConfig
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    [JsonPropertyName("ip")]
    public required string IpAddress { get; set; }
    
    [JsonPropertyName("model")]
    public string? Model { get; set; }
    
    [JsonPropertyName("added")]
    public DateTime Added { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("last_seen")]
    public DateTime? LastSeen { get; set; }
}

public class DeviceList
{
    [JsonPropertyName("devices")]
    public List<DeviceConfig> Devices { get; set; } = new();
}