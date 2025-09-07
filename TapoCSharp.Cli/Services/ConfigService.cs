using System.Text.Json;
using TapoCSharp.Cli.Models;

namespace TapoCSharp.Cli.Services;

public class ConfigService
{
    private static readonly string ConfigDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".tapo");
    private static readonly string AuthFile = Path.Combine(ConfigDirectory, "auth.json");
    private static readonly string DevicesFile = Path.Combine(ConfigDirectory, "devices.json");

    public ConfigService()
    {
        EnsureConfigDirectoryExists();
    }

    private void EnsureConfigDirectoryExists()
    {
        if (!Directory.Exists(ConfigDirectory))
        {
            Directory.CreateDirectory(ConfigDirectory);
            // Set restrictive permissions (Linux/macOS)
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                File.SetUnixFileMode(ConfigDirectory, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            }
        }
    }

    public async Task SaveAuthConfigAsync(AuthConfig config)
    {
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(AuthFile, json);
        
        // Set restrictive permissions
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            File.SetUnixFileMode(AuthFile, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    public async Task<AuthConfig?> LoadAuthConfigAsync()
    {
        if (!File.Exists(AuthFile))
            return null;

        var json = await File.ReadAllTextAsync(AuthFile);
        return JsonSerializer.Deserialize<AuthConfig>(json);
    }

    public async Task<DeviceList> LoadDevicesAsync()
    {
        if (!File.Exists(DevicesFile))
            return new DeviceList();

        var json = await File.ReadAllTextAsync(DevicesFile);
        return JsonSerializer.Deserialize<DeviceList>(json) ?? new DeviceList();
    }

    public async Task SaveDevicesAsync(DeviceList deviceList)
    {
        var json = JsonSerializer.Serialize(deviceList, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(DevicesFile, json);
        
        // Set restrictive permissions
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            File.SetUnixFileMode(DevicesFile, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    public async Task<DeviceConfig?> FindDeviceAsync(string ipOrName)
    {
        var devices = await LoadDevicesAsync();
        return devices.Devices.FirstOrDefault(d => 
            d.IpAddress.Equals(ipOrName, StringComparison.OrdinalIgnoreCase) ||
            d.Name.Equals(ipOrName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task AddDeviceAsync(DeviceConfig device)
    {
        var devices = await LoadDevicesAsync();
        
        // Remove existing device with same IP or name
        devices.Devices.RemoveAll(d => 
            d.IpAddress.Equals(device.IpAddress, StringComparison.OrdinalIgnoreCase) ||
            d.Name.Equals(device.Name, StringComparison.OrdinalIgnoreCase));
        
        devices.Devices.Add(device);
        await SaveDevicesAsync(devices);
    }

    public async Task RemoveDeviceAsync(string ipOrName)
    {
        var devices = await LoadDevicesAsync();
        devices.Devices.RemoveAll(d => 
            d.IpAddress.Equals(ipOrName, StringComparison.OrdinalIgnoreCase) ||
            d.Name.Equals(ipOrName, StringComparison.OrdinalIgnoreCase));
        
        await SaveDevicesAsync(devices);
    }

}