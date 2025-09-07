using System.Text.Json;

namespace TapoCSharp;

/// <summary>
/// Tapo API Client for controlling TP-Link Tapo devices.
/// </summary>
public class ApiClient
{
    private readonly string _username;
    private readonly string _password;
    private readonly HttpClient _httpClient;
    private readonly TimeSpan _timeout;

    /// <summary>
    /// Creates a new instance of ApiClient.
    /// </summary>
    /// <param name="username">Tapo username (email)</param>
    /// <param name="password">Tapo password</param>
    /// <param name="timeout">Connection timeout (default: 30 seconds)</param>
    public ApiClient(string username, string password, TimeSpan? timeout = null)
    {
        _username = username ?? throw new ArgumentNullException(nameof(username));
        _password = password ?? throw new ArgumentNullException(nameof(password));
        _timeout = timeout ?? TimeSpan.FromSeconds(30);
        
        var handler = new HttpClientHandler()
        {
            UseCookies = false // Disable automatic cookie handling
        };
        
        _httpClient = new HttpClient(handler)
        { 
            Timeout = _timeout 
        };
    }

    /// <summary>
    /// Creates a P100 plug handler for the specified IP address.
    /// </summary>
    /// <param name="ipAddress">Device IP address</param>
    /// <returns>Authenticated P100PlugHandler</returns>
    public async Task<P100PlugHandler> P100Async(string ipAddress)
    {
        var handler = new P100PlugHandler(_username, _password, ipAddress, _httpClient);
        await handler.LoginAsync();
        return handler;
    }

    /// <summary>
    /// Releases resources used by the ApiClient.
    /// </summary>
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}