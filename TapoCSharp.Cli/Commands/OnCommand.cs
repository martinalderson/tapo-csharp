using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using TapoCSharp.Cli.Services;
using TapoCSharp.Cli.Settings;

namespace TapoCSharp.Cli.Commands;

[Description("Turn a device on")]
public class OnCommand : AsyncCommand<DeviceCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, DeviceCommandSettings settings)
    {
        try
        {
            var configService = new ConfigService();
            var deviceService = new DeviceService(configService);

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"Turning on {settings.Device}...", async ctx =>
                {
                    var device = await deviceService.ConnectToDeviceAsync(settings.Device);
                    if (device != null)
                        await device.OnAsync();
                });

            AnsiConsole.MarkupLine($"[green]✓[/] Device '{settings.Device}' turned on successfully!");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]");
            return 1;
        }
    }
}