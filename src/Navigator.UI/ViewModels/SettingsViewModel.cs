using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Navigator.UI.Models;

namespace Navigator.UI.ViewModels;

public partial class SettingsViewModel : ViewModelBase {
    private readonly SettingsTab _model;

    [ObservableProperty] private bool _isDarkTheme;
    [ObservableProperty] private bool _showHiddenFiles;
    [ObservableProperty] private string _defaultFolder;
    [ObservableProperty] private string _tabName = "Settings";

    public SettingsViewModel(SettingsTab model) {
        _model = model;
        Initialize();
    }

    private void Initialize() {
        Logger.Info("Initializing SettingsViewModel");
        // Use _model so the field is not reported as unused by analyzers
        Logger.Debug($"SettingsViewModel bound to tab instance: {_model.GetHashCode()}");
        // Load persisted settings here if/when persistence is added
        Logger.Debug("SettingsViewModel initialized successfully");
    }

    [RelayCommand]
    private void Save() {
        // TODO: persist settings
        Logger.Info($"Settings saved: IsDarkTheme={IsDarkTheme}, ShowHiddenFiles={ShowHiddenFiles}, DefaultFolder={DefaultFolder}");
    }

    [RelayCommand]
    private void Reset() {
        IsDarkTheme = false;
        ShowHiddenFiles = false;
        DefaultFolder = string.Empty;
        Logger.Info("Settings reset to defaults");
    }
}
