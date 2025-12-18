using Navigator.UI.ViewModels;

namespace Navigator.UI.Models;

public class SettingsTab : TabItem {
    public SettingsViewModel ViewModel { get; private set; }

    public SettingsTab() {
        Logger.Info("Initializing SettingsTab");

        Title = "Settings";
        IconPath = "/Assets/settings-32.png";

        ViewModel = new SettingsViewModel(this);

        ContentControl = new Views.SettingsView(this);
        Logger.Debug("SettingsTab initialized successfully");
    }
}
