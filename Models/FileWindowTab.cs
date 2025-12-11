using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Navigator.Models.Nodes;
using Navigator.Views;
using Navigator.ViewModels;

namespace Navigator.Models;

public class FileWindowTab : TabItem {
    public FileWindowTabViewModel? ViewModel { get; private set; }

    public FileWindowTab() {
        Logger.Info("Initializing FileWindowTab");

        Title = "File Explorer";
        IconPath = "/Assets/navigator-32.png";

        // Create the view model
        ViewModel = new FileWindowTabViewModel(this);

        // Create the content view
        ContentControl = new FileWindowTabView(this);
        Logger.Debug("FileWindowTab initialized successfully");
    }

}
