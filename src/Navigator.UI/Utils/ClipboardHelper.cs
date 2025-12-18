using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;

namespace Navigator.UI.Utils;

public static class ClipboardHelper {

    private static Window? Window => Application.Current?.ApplicationLifetime is not Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop ? null : desktop.MainWindow;
    private static IClipboard? Clipboard => Window?.Clipboard;

    public static void SetTextAsync(string text) {
        Clipboard?.SetTextAsync(text);
    }

    public static void SetDataAsync(IStorageItem data) {
        Clipboard?.SetFileAsync(data);
    }
}
