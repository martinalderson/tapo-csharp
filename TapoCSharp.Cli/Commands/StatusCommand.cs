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

            JsonNode? deviceInfo = null;
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"Getting status for {settings.Device}...", async ctx =>
                {
                    var device = await deviceService.ConnectToDeviceAsync(settings.Device);
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
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]");
            return 1;
        }
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