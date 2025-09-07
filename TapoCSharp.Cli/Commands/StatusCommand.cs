using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using Spectre.Console;
using Spectre.Console.Cli;
using TapoCSharp.Cli.Services;
using TapoCSharp.Cli.Settings;

namespace TapoCSharp.Cli.Commands;

[Description("Show device status and information")]
public class StatusCommand : AsyncCommand<DeviceCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, DeviceCommandSettings settings)
    {
        try
        {
            var configService = new ConfigService();
            var deviceService = new DeviceService(configService);

            if (string.IsNullOrEmpty(settings.Device))
            {
                // Show status for all devices
                return await ShowAllDevicesStatusAsync(deviceService);
            }
            else
            {
                // Show status for single device
                return await ShowSingleDeviceStatusAsync(deviceService, settings.Device);
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]");
            return 1;
        }
    }

    private async Task<int> ShowSingleDeviceStatusAsync(DeviceService deviceService, string deviceName)
    {
        JsonNode? deviceInfo = null;
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync($"Getting status for {deviceName}...", async ctx =>
            {
                var device = await deviceService.ConnectToDeviceAsync(deviceName);
                if (device != null)
                    deviceInfo = await device.GetDeviceInfoAsync();
            });

        if (deviceInfo == null)
        {
            AnsiConsole.MarkupLine("[red]✗ Failed to retrieve device information[/]");
            return 1;
        }

        // Create a panel with device information
        var nickname = deviceInfo["nickname"]?.ToString() ?? "Device";
        var deviceOn = deviceInfo["device_on"]?.GetValue<bool>() ?? false;
        
        var panel = new Panel(CreateDeviceInfoTable(deviceInfo))
            .Header($" {nickname} ")
            .Border(BoxBorder.Rounded)
            .BorderColor(deviceOn ? Color.Green : Color.Red);

        AnsiConsole.Write(panel);
        return 0;
    }

    private async Task<int> ShowAllDevicesStatusAsync(DeviceService deviceService)
    {
        var devices = await deviceService.GetAllDevicesAsync();
        
        if (!devices.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No devices configured. Run 'tapo auth' to set up devices.[/]");
            return 1;
        }

        AnsiConsole.MarkupLine($"[blue]Getting status for {devices.Length} device(s)...[/]");
        AnsiConsole.WriteLine();

        var deviceStatuses = new List<(string name, JsonNode? info, Exception? error)>();
        
        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Checking devices...", maxValue: devices.Length);
                var semaphore = new SemaphoreSlim(3); // Limit concurrent connections
                
                var tasks = devices.Select(async deviceConfig =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var device = await deviceService.ConnectToDeviceAsync(deviceConfig.Name);
                        var info = device != null ? await device.GetDeviceInfoAsync() : null;
                        lock (deviceStatuses)
                        {
                            deviceStatuses.Add((deviceConfig.Name, info, null));
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (deviceStatuses)
                        {
                            deviceStatuses.Add((deviceConfig.Name, null, ex));
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                        task.Increment(1);
                    }
                });
                
                await Task.WhenAll(tasks);
            });

        // Sort by device name for consistent output
        deviceStatuses.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));

        // Create summary table
        var summaryTable = new Table();
        summaryTable.AddColumn("Device");
        summaryTable.AddColumn("Status");
        summaryTable.AddColumn("Model");
        summaryTable.AddColumn("IP Address");
        
        foreach (var (name, info, error) in deviceStatuses)
        {
            if (error != null)
            {
                summaryTable.AddRow(name, "[red]Offline[/]", "[dim]Error[/]", "[dim]N/A[/]");
            }
            else if (info != null)
            {
                var deviceOn = info["device_on"]?.GetValue<bool>() ?? false;
                var status = deviceOn ? "[green]On[/]" : "[yellow]Off[/]";
                var model = info["model"]?.ToString() ?? "Unknown";
                var ip = info["ip"]?.ToString() ?? "Unknown";
                
                summaryTable.AddRow(name, status, model, ip);
            }
            else
            {
                summaryTable.AddRow(name, "[red]Failed[/]", "[dim]N/A[/]", "[dim]N/A[/]");
            }
        }

        AnsiConsole.Write(summaryTable);
        return 0;
    }

    private static Table CreateDeviceInfoTable(JsonNode deviceInfo)
    {
        var table = new Table();
        table.AddColumn("Property");
        table.AddColumn("Value");
        table.Border = TableBorder.None;
        table.ShowHeaders = false;

        // Power state with color
        var deviceOn = deviceInfo["device_on"]?.GetValue<bool>() ?? false;
        var powerState = deviceOn ? "[green]On[/]" : "[red]Off[/]";
        table.AddRow("[bold]Power State[/]", powerState);

        // Basic info
        var model = deviceInfo["model"]?.ToString() ?? "Unknown";
        var ip = deviceInfo["ip"]?.ToString() ?? "Unknown";
        var mac = deviceInfo["mac"]?.ToString() ?? "Unknown";
        table.AddRow("Model", model);
        table.AddRow("IP Address", ip);
        table.AddRow("MAC Address", mac);
        
        // Firmware info
        var firmware = deviceInfo["fw_ver"]?.ToString() ?? "Unknown";
        var hardware = deviceInfo["hw_ver"]?.ToString() ?? "Unknown";
        table.AddRow("Firmware", firmware);
        table.AddRow("Hardware", hardware);

        // Network info
        var ssid = deviceInfo["ssid"]?.ToString();
        if (!string.IsNullOrEmpty(ssid))
        {
            table.AddRow("Wi-Fi SSID", ssid);
        }
        
        if (deviceInfo["rssi"]?.GetValue<int?>() is int rssi)
        {
            var signalColor = rssi > -50 ? "green" : rssi > -70 ? "yellow" : "red";
            table.AddRow("Signal Strength", $"[{signalColor}]{rssi} dBm[/]");
        }
        
        if (deviceInfo["signal_level"]?.GetValue<int?>() is int signalLevel)
        {
            var bars = new string('█', Math.Max(1, signalLevel));
            table.AddRow("Signal Level", $"{bars} ({signalLevel}/3)");
        }

        // Runtime info
        if (deviceInfo["on_time"]?.GetValue<int?>() is int onTime && onTime > 0)
        {
            var timeSpan = TimeSpan.FromSeconds(onTime);
            table.AddRow("On Time", FormatTimeSpan(timeSpan));
        }

        // Location info
        var region = deviceInfo["region"]?.ToString();
        if (!string.IsNullOrEmpty(region))
        {
            table.AddRow("Region", region);
        }

        return table;
    }

    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays >= 1)
            return $"{timeSpan.Days}d {timeSpan.Hours}h {timeSpan.Minutes}m";
        if (timeSpan.TotalHours >= 1)
            return $"{timeSpan.Hours}h {timeSpan.Minutes}m";
        return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
    }
}