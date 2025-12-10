using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Navigator.Converters;

/// <summary>
/// Converter that determines the current operating system platform
/// </summary>
public class OSPlatformConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        if (parameter is string platformName) {
            return GetCurrentPlatform() == platformName;
        }

        return GetCurrentPlatform();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the current operating system platform as a string
    /// </summary>
    public static string GetCurrentPlatform() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "Windows";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "macOS";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "Linux";

        return "Unknown";
    }
}
