using System.Text;
using ThermalRight.AIOMonitor.Capture;

namespace ThermalRight.AIOMonitor.App;

public static class VirtualDisplay
{
    public const string MonitorName = "ThermalRight.AIOMonitor";
    public const string SettingsFileName = "vdd_settings.xml";

    public static MonitorInfo? Find(IReadOnlyList<MonitorInfo> monitors)
    {
        foreach (var monitor in monitors)
        {
            if (MonitorCapture.FriendlyName(monitor.DeviceName).Contains(MonitorName, StringComparison.OrdinalIgnoreCase))
            {
                return monitor;
            }
        }

        return null;
    }

    public static string SettingsXml(int width, int height, int refreshRate)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<?xml version='1.0' encoding='utf-8'?>");
        builder.AppendLine("<vdd_settings>");
        builder.AppendLine("  <monitors>");
        builder.AppendLine("    <count>1</count>");
        builder.AppendLine("  </monitors>");
        builder.AppendLine("  <gpu>");
        builder.AppendLine("    <friendlyname>default</friendlyname>");
        builder.AppendLine("  </gpu>");
        builder.AppendLine("  <global>");
        builder.AppendLine($"    <g_refresh_rate>{refreshRate}</g_refresh_rate>");
        builder.AppendLine("  </global>");
        builder.AppendLine("  <resolutions>");
        builder.AppendLine($"    <resolution><width>{width}</width><height>{height}</height><refresh_rate>{refreshRate}</refresh_rate></resolution>");
        builder.AppendLine("  </resolutions>");
        builder.AppendLine("  <edid>");
        builder.AppendLine("    <CustomEdid>false</CustomEdid>");
        builder.AppendLine("    <PreventSpoof>false</PreventSpoof>");
        builder.AppendLine("    <EdidCeaOverride>false</EdidCeaOverride>");
        builder.AppendLine("  </edid>");
        builder.AppendLine("</vdd_settings>");
        return builder.ToString();
    }
}
