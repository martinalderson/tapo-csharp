using System.Net;
using System.Net.Sockets;
using Spectre.Console;

namespace TapoCSharp.Cli.Services;

public class NetworkScanService
{
    private readonly DeviceService _deviceService;

    public NetworkScanService(DeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    /// <summary>
    /// Scans entire /24 subnet for Tapo devices
    /// </summary>
    public async Task<List<(string ip, string model)>> ScanSubnetAsync(string knownDeviceIp, IProgress<string>? progress = null)
    {
        var foundDevices = new List<(string ip, string model)>();
        
        // Parse the known IP to get the subnet
        if (!IPAddress.TryParse(knownDeviceIp, out var ipAddress))
        {
            throw new ArgumentException($"Invalid IP address: {knownDeviceIp}");
        }

        var ipBytes = ipAddress.GetAddressBytes();
        var subnet = $"{ipBytes[0]}.{ipBytes[1]}.{ipBytes[2]}";
        
        progress?.Report($"Scanning entire subnet {subnet}.0/24 for Tapo devices...");
        
        // First pass: scan for hosts with port 80 open
        var hostsWithPort80 = new List<string>();
        var portScanTasks = new List<Task>();
        var portScanSemaphore = new SemaphoreSlim(50); // More aggressive for port scanning
        
        for (int hostNum = 1; hostNum <= 254; hostNum++)
        {
            int currentHost = hostNum; // Capture for closure
            portScanTasks.Add(Task.Run(async () =>
            {
                await portScanSemaphore.WaitAsync();
                try
                {
                    var testIp = $"{subnet}.{currentHost}";
                    progress?.Report($"Port scanning {testIp}:80...");
                    
                    if (await IsPortOpenAsync(testIp, 80))
                    {
                        lock (hostsWithPort80)
                        {
                            hostsWithPort80.Add(testIp);
                        }
                        progress?.Report($"✓ Port 80 open on {testIp}");
                    }
                }
                catch
                {
                    // Ignore errors during scanning
                }
                finally
                {
                    portScanSemaphore.Release();
                }
            }));
        }
        
        await Task.WhenAll(portScanTasks);
        progress?.Report($"Found {hostsWithPort80.Count} hosts with port 80 open, testing for Tapo devices...");
        
        // Second pass: test Tapo connectivity on hosts with port 80 open
        var tapoTestTasks = new List<Task>();
        var tapoTestSemaphore = new SemaphoreSlim(5); // Conservative for actual Tapo connections
        
        foreach (var ip in hostsWithPort80)
        {
            tapoTestTasks.Add(Task.Run(async () =>
            {
                await tapoTestSemaphore.WaitAsync();
                try
                {
                    progress?.Report($"Testing Tapo connection to {ip}...");
                    
                    var (success, model, _) = await _deviceService.TestDeviceConnectionAsync(ip);
                    if (success && !string.IsNullOrEmpty(model))
                    {
                        lock (foundDevices)
                        {
                            foundDevices.Add((ip, model));
                        }
                        progress?.Report($"✓ Found {model} at {ip}");
                    }
                }
                catch
                {
                    // Ignore errors during scanning
                }
                finally
                {
                    tapoTestSemaphore.Release();
                }
            }));
        }
        
        await Task.WhenAll(tapoTestTasks);
        
        return foundDevices.OrderBy(d => IPAddress.Parse(d.ip).GetAddressBytes()[3]).ToList();
    }

    private async Task<bool> IsPortOpenAsync(string ipAddress, int port)
    {
        try
        {
            using var tcpClient = new TcpClient();
            var connectTask = tcpClient.ConnectAsync(ipAddress, port);
            var timeoutTask = Task.Delay(500); // 500ms timeout
            
            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
            
            if (completedTask == connectTask && tcpClient.Connected)
            {
                return true;
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }
}