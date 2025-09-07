using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using TapoCSharp.Cli.Models;
using TapoCSharp.Cli.Services;
using TapoCSharp.Cli.Settings;

namespace TapoCSharp.Cli.Commands;

[Description("Manage Tapo devices")]
public class DevicesCommand : Command<GlobalSettings>
{
    public override int Execute(CommandContext context, GlobalSettings settings)
    {
        // This is the parent command - show help
        AnsiConsole.MarkupLine("[yellow]Use one of the subcommands:[/]");
        AnsiConsole.MarkupLine("  [blue]tapo devices ls[/]     - List devices");
        AnsiConsole.MarkupLine("  [blue]tapo devices add[/]    - Add a device");
        AnsiConsole.MarkupLine("  [blue]tapo devices rm[/]     - Remove a device");
        return 0;
    }
}

[Description("List all configured devices")]
public class ListDevicesCommand : AsyncCommand<GlobalSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings)
    {
        try
        {
            var configService = new ConfigService();
            var deviceService = new DeviceService(configService);
            var devices = await deviceService.GetAllDevicesAsync();

            if (devices.Length == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No devices configured.[/]");
                AnsiConsole.MarkupLine("Add devices with: [blue]tapo devices add <ip>[/]");
                return 0;
            }

            var table = new Table();
            table.AddColumn("Name");
            table.AddColumn("IP Address");
            table.AddColumn("Model");
            table.AddColumn("Added");
            table.AddColumn("Last Seen");

            foreach (var device in devices.OrderBy(d => d.Name))
            {
                var lastSeen = device.LastSeen?.ToString("yyyy-MM-dd HH:mm") ?? "[dim]Never[/]";
                table.AddRow(
                    device.Name,
                    device.IpAddress,
                    device.Model ?? "[dim]Unknown[/]",
                    device.Added.ToString("yyyy-MM-dd"),
                    lastSeen
                );
            }

            AnsiConsole.Write(table);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]");
            return 1;
        }
    }
}

[Description("Add a new device")]
public class AddDeviceCommand : AsyncCommand<AddDeviceSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, AddDeviceSettings settings)
    {
        try
        {
            var configService = new ConfigService();
            var deviceService = new DeviceService(configService);

            // Test connection first
            string? model = null;
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"Testing connection to {settings.IpAddress}...", async ctx =>
                {
                    var (success, deviceModel, error) = await deviceService.TestDeviceConnectionAsync(settings.IpAddress);
                    if (!success)
                    {
                        throw new InvalidOperationException($"Cannot connect to device: {error}");
                    }
                    model = deviceModel;
                });

            AnsiConsole.MarkupLine($"[green]✓[/] Successfully connected to {model ?? "device"}");

            // Get device name
            var deviceName = settings.Name;
            if (string.IsNullOrEmpty(deviceName))
            {
                deviceName = AnsiConsole.Ask<string>("Enter a name for this device:");
            }

            // Check if device already exists
            var existingDevice = await configService.FindDeviceAsync(settings.IpAddress);
            if (existingDevice != null)
            {
                var overwrite = AnsiConsole.Confirm($"Device with IP {settings.IpAddress} already exists as '{existingDevice.Name}'. Update?");
                if (!overwrite)
                {
                    AnsiConsole.MarkupLine("[yellow]Device add cancelled.[/]");
                    return 0;
                }
            }

            // Add device
            var device = new DeviceConfig
            {
                Name = deviceName,
                IpAddress = settings.IpAddress,
                Model = model,
                Added = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow
            };

            await configService.AddDeviceAsync(device);

            AnsiConsole.MarkupLine($"[green]✓[/] Device '{deviceName}' ({settings.IpAddress}) added successfully!");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]");
            return 1;
        }
    }
}

[Description("Remove a device")]
public class RemoveDeviceCommand : AsyncCommand<RemoveDeviceSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, RemoveDeviceSettings settings)
    {
        try
        {
            var configService = new ConfigService();
            var device = await configService.FindDeviceAsync(settings.Device);

            if (device == null)
            {
                AnsiConsole.MarkupLine($"[red]✗[/] Device '{settings.Device}' not found.");
                return 1;
            }

            var confirm = AnsiConsole.Confirm($"Remove device '[red]{device.Name}[/]' ({device.IpAddress})?");
            if (!confirm)
            {
                AnsiConsole.MarkupLine("[yellow]Device removal cancelled.[/]");
                return 0;
            }

            await configService.RemoveDeviceAsync(settings.Device);
            AnsiConsole.MarkupLine($"[green]✓[/] Device '{device.Name}' removed successfully!");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]");
            return 1;
        }
    }
}