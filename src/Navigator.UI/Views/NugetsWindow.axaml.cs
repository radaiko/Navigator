using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Navigator.UI.Models;
using Navigator.UI.Utils;

namespace Navigator.UI.Views;

public partial class NugetsWindow : Window {
    // Collection bound to the ListBox (always initialized)
    private readonly ObservableCollection<PackageInfo> _items = [];

    public NugetsWindow() {
        InitializeComponent();

        // The generated field for the named ListBox may sometimes be null (depending on how XAML was loaded),
        // so resolve it defensively and set its ItemsSource.
        var list = PackagesList ?? this.FindControl<ListBox>("PackagesList");
        if (list != null) list.ItemsSource = _items;

        var sbomPath = FindSbomPath();
        if (sbomPath.IsBlank()) throw new ArgumentNullException(nameof(sbomPath), "SBOM not found");
        ParseSbom(sbomPath);
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }

    private string? FindSbomPath() {
        try {
            var baseDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "Resources"));
            var binCandidate = Path.Combine(baseDir, "sbom.json");
            if (File.Exists(binCandidate)) return binCandidate;
        } catch {
            // ignore IO errors and fall back to null
        }
        return null;
    }

    private void ParseSbom(string? sbomPath) {
        if (string.IsNullOrEmpty(sbomPath)) return;
        var txt = File.ReadAllText(sbomPath);
        var json = new Json(txt);

        var components = json["components"]?.A;
        if (components != null) {
            foreach (var component in components) {
                var obj = component.O;
                if (obj != null) _items?.Add(new PackageInfo(obj));
            }
        }
    }


    // Open license/project/nuget page when the license button is clicked
    private void LicenseButton_Click(object? sender, RoutedEventArgs e) {
        if (sender is Button btn && btn.DataContext is PackageInfo pi) {
            var url = pi.Url;
            if (string.IsNullOrEmpty(url)) return;
            try {
                var psi = new ProcessStartInfo { FileName = url, UseShellExecute = true };
                Process.Start(psi);
            } catch {
                // ignore
            }
        }
    }
}
