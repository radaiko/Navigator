using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Navigator.UI.Models;
using Navigator.UI.ViewModels;
using Navigator.UI.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace Navigator.UI;

public class App : Application {
    private MainWindowViewModel? _mainWindowViewModel;

    public override void Initialize() {
        Logger.EnableDebugLogging = true;
        Logger.UseFile = true;
        Logger.Info("---- Application Started ----");

        // Observe task scheduler unobserved exceptions so they don't become a process-level crash
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        // Try to subscribe to an Avalonia dispatcher-level unhandled exception event if available
        TrySubscribeToDispatcherUnhandledException();

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

    private void TrySubscribeToDispatcherUnhandledException() {
        try {
            // Use reflection to avoid compile-time dependency on a specific Avalonia API shape.
            var uiThread = typeof(Dispatcher).GetProperty("UIThread", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
            if (uiThread is not null) {
                var evt = uiThread.GetType().GetEvent("UnhandledException");
                if (evt is not null) {
                    // Subscribe with a generic handler that will attempt to read Exception and Handled properties
                    var handler = Delegate.CreateDelegate(evt.EventHandlerType!, this, nameof(DispatcherUnhandledExceptionBridge));
                    evt.AddEventHandler(uiThread, handler);
                    Logger.Debug("Subscribed to Dispatcher.UIThread.UnhandledException (via reflection)");
                }
            }
        } catch (Exception ex) {
            Logger.Warning("Failed to subscribe to Dispatcher unhandled exception via reflection: " + ex.Message);
        }
    }

    // Bridge method used for dispatcher-level unhandled exceptions discovered via reflection.
    // The actual args type may vary by Avalonia version; use reflection/dynamic to access Exception and Handled.
    private void DispatcherUnhandledExceptionBridge(object? _, object args) {
        try {
            // Use dynamic-like reflection to extract Exception and Handled if present
            var exProp = args.GetType().GetProperty("Exception") ?? args.GetType().GetProperty("InnerException");
            var handledProp = args.GetType().GetProperty("Handled");
            Exception? ex = null;
            if (exProp is not null) {
                ex = exProp.GetValue(args) as Exception;
            }

            if (ex is null) ex = new Exception("Unknown dispatcher exception");

            Logger.Error("Dispatcher unhandled exception", ex);

            // Mark handled if the event args exposes a Handled boolean property.
            if (handledProp is not null && handledProp.PropertyType == typeof(bool) && handledProp.CanWrite) {
                handledProp.SetValue(args, true);
            }

            // Show dialog on UI thread (we should already be on UI thread, but be safe)
            Dispatcher.UIThread.Post(() => ShowExceptionWindow(ex));
        } catch (Exception e) {
            // If anything goes wrong here, log it but don't rethrow
            Logger.Error("Error in DispatcherUnhandledExceptionBridge", e);
        }
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) {
        try {
            Logger.Error("Unobserved task exception", e.Exception);
            // Prevent further escalation
            e.SetObserved();

            // Show UI notification
            Dispatcher.UIThread.Post(() => ShowExceptionWindow(e.Exception));
        } catch (Exception ex) {
            Logger.Error("Error handling UnobservedTaskException", ex);
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
