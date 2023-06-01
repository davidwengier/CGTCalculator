using Microsoft.Win32;

namespace CGTCalculator.Code;

internal static class SettingsStorage
{
    internal static string ReadString(string keyName)
    {
        using var registry = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\CgtCalculator");
        if (registry is not null)
        {
            return registry.GetValue(keyName, "") as string ?? "";
        }

        return "";
    }

    internal static void WriteString(string keyName, string value)
    {
        var registry = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\CgtCalculator");
        try
        {
            if (registry is null)
            {
                registry = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\CgtCalculator");
            }
            registry.SetValue(keyName, value, RegistryValueKind.String);
        }
        finally
        {
            registry?.Dispose();
        }
    }
}
