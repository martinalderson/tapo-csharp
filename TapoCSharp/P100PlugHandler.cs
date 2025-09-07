using System.Text.Json;
using System.Text.Json.Nodes;

namespace TapoCSharp;

/// <summary>
/// Handler for P100 and P105 smart plugs.
/// </summary>
public class P100PlugHandler
{
    private readonly string _username;
    private readonly string _password;
    private readonly string _ipAddress;
    private readonly HttpClient _httpClient;
    private readonly TapoProtocol _protocol;

    internal P100PlugHandler(string username, string password, string ipAddress, HttpClient httpClient)
    {
        _username = username;
        _password = password;
        _ipAddress = ipAddress;
        _httpClient = httpClient;
        _protocol = new TapoProtocol(httpClient);
    }

    /// <summary>
    /// Authenticates with the device.
    /// </summary>
    internal async Task LoginAsync()
    {
        await _protocol.LoginAsync($"http://{_ipAddress}/app", _username, _password);
    }

    /// <summary>
    /// Turns the device on.
    /// </summary>
    public async Task OnAsync()
    {
        var deviceInfo = new { device_on = true };
        await _protocol.SetDeviceInfoAsync(deviceInfo);
    }

    /// <summary>
    /// Turns the device off.
    /// </summary>
    public async Task OffAsync()
    {
        var deviceInfo = new { device_on = false };
        await _protocol.SetDeviceInfoAsync(deviceInfo);
    }

    /// <summary>
    /// Gets device information.
    /// </summary>
    /// <returns>Device information as JSON</returns>
    public async Task<JsonNode> GetDeviceInfoAsync()
    {
        return await _protocol.GetDeviceInfoAsync();
    }

    /// <summary>
    /// Refreshes the authentication session.
    /// </summary>
    public async Task RefreshSessionAsync()
    {
        await LoginAsync();
    }
}