using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Navigator.UI.Converters;

/// <summary>
/// Selects a platform-specific icon URI. Parameter should be in the format "mac: /Assets/navigator.icns;default: /Assets/navigator.ico"
/// It returns the mac path when running on macOS, otherwise the default path.
/// </summary>
public class PlatformIconConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        if (parameter is not string param) return null;

        // parse parameter like "mac:/Assets/navigator.icns;default:/Assets/navigator.ico"
        string mac = null!;
        string def = null!;
        var parts = param.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts) {
            var kv = p.Split(':', 2);
            if (kv.Length != 2) continue;
            var key = kv[0].Trim().ToLowerInvariant();
            var val = kv[1].Trim();
            if (key == "mac" || key == "macos") mac = val;
            if (key == "default") def = val;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return mac ?? def;
        return def ?? mac;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        throw new NotImplementedException();
    }
}

