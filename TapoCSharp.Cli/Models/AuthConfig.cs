using System.Text.Json.Serialization;

namespace TapoCSharp.Cli.Models;

public class AuthConfig
{
    [JsonPropertyName("username")]
    public required string Username { get; set; }
    
    [JsonPropertyName("password")]
    public required string Password { get; set; }
    
    [JsonPropertyName("encrypted")]
    public bool IsEncrypted { get; set; } = false;
}