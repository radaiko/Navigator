using Navigator.UI.Views;
using Navigator.UI.ViewModels;

namespace Navigator.UI.Models;

public class FileWindowTab : TabItem {
    public FileWindowTabViewModel ViewModel { get; private set; }

    public FileWindowTab() {
        Logger.Info("Initializing FileWindowTab");

        Title = "File Explorer";
        IconPath = "/Assets/navigator-32.png";

        ViewModel = new FileWindowTabViewModel(this);

        ContentControl = new FileWindowTabView(this);
        Logger.Debug("FileWindowTab initialized successfully");
    }

}

