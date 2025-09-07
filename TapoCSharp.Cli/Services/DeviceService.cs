using System.Text;
using TapoCSharp;
using TapoCSharp.Cli.Models;

namespace TapoCSharp.Cli.Services;

public class DeviceService
{
    private readonly ConfigService _configService;

    public DeviceService(ConfigService configService)
    {
        _configService = configService;
    }

    public async Task<P100PlugHandler?> ConnectToDeviceAsync(string ipOrName)
    {
        var auth = await _configService.LoadAuthConfigAsync();
        if (auth == null)
        {
            throw new InvalidOperationException("Authentication not configured. Run 'tapo auth' first.");
        }

        var device = await _configService.FindDeviceAsync(ipOrName);
        var ipAddress = device?.IpAddress ?? ipOrName;

        // Validate IP address format
        if (!System.Net.IPAddress.TryParse(ipAddress, out _))
        {
            throw new ArgumentException($"Invalid IP address: {ipAddress}");
        }

        try
        {
            var client = new ApiClient(auth.Username, auth.Password);
            var plugHandler = await client.P100Async(ipAddress);
            
            // Update last seen time if this was a saved device
            if (device != null)
            {
                device.LastSeen = DateTime.UtcNow;
                await _configService.AddDeviceAsync(device);
            }

            return plugHandler;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to connect to device at {ipAddress}: {ex.Message}", ex);
        }
    }

    public async Task<(bool success, string? model, string? error)> TestDeviceConnectionAsync(string ipAddress)
    {
        try
        {
            var auth = await _configService.LoadAuthConfigAsync();
            if (auth == null)
            {
                return (false, null, "Authentication not configured");
            }

            var client = new ApiClient(auth.Username, auth.Password);
            var device = await client.P100Async(ipAddress);
            var deviceInfo = await device.GetDeviceInfoAsync();
            
            var model = deviceInfo?["model"]?.ToString();
            return (true, model, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<DeviceConfig[]> GetAllDevicesAsync()
    {
        var deviceList = await _configService.LoadDevicesAsync();
        return deviceList.Devices.ToArray();
    }

    /// <summary>
    /// Decodes the Base64 encoded nickname from device info
    /// </summary>
    public static string DecodeNickname(string? base64Nickname)
    {
        if (string.IsNullOrEmpty(base64Nickname))
            return "Unnamed Device";

        try
        {
            var bytes = Convert.FromBase64String(base64Nickname);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return base64Nickname; // Return as-is if not valid Base64
        }
    }
}