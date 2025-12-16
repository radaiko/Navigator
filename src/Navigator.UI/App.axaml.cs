using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Navigator.UI.Models;
using Navigator.UI.ViewModels;
using Navigator.UI.Views;

namespace Navigator.UI;

public class App : Application {
    public override void Initialize() {
        Logger.EnableDebugLogging = true;
        Logger.UseFile = true;
        Logger.Info("---- Application Started ----");
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            DisableAvaloniaDataAnnotationValidation();

            var mainViewModel = new MainWindowViewModel();

            // Add a FileWindowTab by default
            var fileTab = new FileWindowTab();
            mainViewModel.AddTab(fileTab);

            desktop.MainWindow = new MainWindow {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation() {
        DataAnnotationsValidationPlugin[] dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (DataAnnotationsValidationPlugin plugin in dataValidationPluginsToRemove) {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}

