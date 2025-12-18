using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Navigator.UI.Models;
using Navigator.UI.ViewModels;
using Navigator.UI.Views;
using System.Reflection;
using System.Linq.Expressions;

namespace Navigator.UI;

public class App : Application {
    private MainWindowViewModel? _mainWindowViewModel;

    public override void Initialize() {
        Logger.EnableDebugLogging = true;
        Logger.UseFile = true;
        Logger.Info("---- Application Started ----");

        // Observe AppDomain unhandled exceptions as a safety net (cannot always prevent termination,
        // but will allow logging and showing a dialog when possible).
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            DisableAvaloniaDataAnnotationValidation();

            _mainWindowViewModel = new MainWindowViewModel();

            // Add a FileWindowTab by default
            var fileTab = new FileWindowTab();
            _mainWindowViewModel.AddTab(fileTab);

            desktop.MainWindow = new MainWindow {
                DataContext = _mainWindowViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e) {
        try {
            Exception ex = e.ExceptionObject as Exception ?? new Exception("Unknown AppDomain exception");
            Logger.Error("AppDomain unhandled exception; IsTerminating=" + e.IsTerminating, ex);

            // Try to show the exception on the UI thread. If the UI thread isn't available this may fail.
            try {
                Dispatcher.UIThread.Post(() => ShowExceptionWindow(ex));
            } catch (Exception dispatchEx) {
                Logger.Error("Failed to dispatch AppDomain exception to UI thread", dispatchEx);
            }
        } catch (Exception logEx) {
            Logger.Error("Error in CurrentDomain_UnhandledException", logEx);
        }
    }

    private void ShowExceptionWindow(Exception exception) {
        try {
            var exceptionDialog = new ExceptionWindow(exception);
            var owner = (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (owner is not null) {
                exceptionDialog.ShowDialog(owner);
            } else {
                exceptionDialog.Show();
            }
        } catch (Exception ex) {
            // Logging only; avoid throwing from the global handler
            Logger.Error("Failed to show exception window", ex);
        }
    }

    private void OpenSettingsTab(object? sender, EventArgs eventArgs) {
        if (_mainWindowViewModel is null) {
            Logger.Warning("MainWindowViewModel is null, cannot open settings tab");
            return;
        }

        // Check if a SettingsTab is already open
        var existingTab = _mainWindowViewModel.Tabs.FirstOrDefault(t => t.GetType() == typeof(SettingsTab));
        if (existingTab is not null) {
            Logger.Debug("Settings tab is already open");
            _mainWindowViewModel.SelectedTab = existingTab;
            return;
        }

        // Create and add a new SettingsTab
        var settingsTab = new SettingsTab();
        _mainWindowViewModel?.AddTab(settingsTab);
        Logger.Debug("Settings tab opened");
    }

    private void DisableAvaloniaDataAnnotationValidation() {
        DataAnnotationsValidationPlugin[] dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (DataAnnotationsValidationPlugin plugin in dataValidationPluginsToRemove) {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private void CheckForUpdates(object? sender, EventArgs e) {
        throw new NotImplementedException();
    }

    private void OpenAboutDialog(object? sender, EventArgs e) {
        // Create and show the About dialog. If we have a main window, show as modal to it,
        // otherwise show as a standalone window.
        var aboutDialog = new AboutDialog();
        var owner = (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (owner is not null) {
            aboutDialog.ShowDialog(owner);
        } else {
            aboutDialog.Show();
        }
    }
}
