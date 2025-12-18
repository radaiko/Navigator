using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;

namespace Navigator.UI.Views;

public partial class AboutDialog : Window {
    public AboutDialog() {
        InitializeComponent();

        VersionText.Text = $"Version {GetVersionString()}";
        RuntimeInfoText.Text = $"Running on {Environment.OSVersion} with .NET {Environment.Version}";
        //LicenseText.Text = "Copyright © 2025 Radaiko. Licensed under the MIT License.";

        OkButton.Click += (_, _) => Close();
        WebsiteButton.Click += (_, _) => OpenUrl("https://github.com/radaiko/navigator");

        // Load icon if available
        try {
            string iconPath = Path.Combine(AppContext.BaseDirectory, "navigator.icns");
            if (!File.Exists(iconPath)) {
                var asmLoc = Assembly.GetEntryAssembly()?.Location;
                if (string.IsNullOrEmpty(asmLoc)) {
                    asmLoc = Assembly.GetExecutingAssembly().Location;
                }
                if (!string.IsNullOrEmpty(asmLoc)) {
                    var asmDir = Path.GetDirectoryName(asmLoc) ?? ".";
                    iconPath = Path.Combine(asmDir, "navigator.icns");
                }
            }

            if (File.Exists(iconPath)) {
                IconImage.Source = new Bitmap(iconPath);
            }
        } catch {
            // ignore
        }
    }

    private static string GetVersionString() {
        try {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var name = assembly.GetName();
            return name.Version?.ToString() ?? "unknown";
        } catch {
            return "unknown";
        }
    }

    private void OpenNugets_Click(object? sender, RoutedEventArgs e) {
        var win = new NugetsWindow(); // create this window to list the NuGet packages
        // Show as modal dialog to this About dialog
        _ = win.ShowDialog(this);
    }

    private static void OpenUrl(string url) {
        try {
            var psi = new ProcessStartInfo {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(psi);
        } catch {
            // ignore
        }
    }
}
