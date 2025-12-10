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
        RootFolders = [];

        Title = "File Explorer";
        IconPath = "/Assets/navigator-32.png";

        InitializeRootFolders();

        // Create the view model
        ViewModel = new FileWindowTabViewModel(this);

        // Create the content view
        ContentControl = new FileWindowTabView(this);
        Logger.Debug("FileWindowTab initialized successfully");
    }

    public ObservableCollection<DirectoryNode> RootFolders { get; }

    private void InitializeRootFolders() {
        Logger.Debug("Initializing root folders");
        RootFolders.Clear();

        string homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var homeDir = new DirectoryNode(homeFolder);
        RootFolders.Add(homeDir);
        Logger.Debug($"Added Home folder: {homeFolder}");

        string desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        if (!string.IsNullOrEmpty(desktopFolder) && Directory.Exists(desktopFolder)) {
            var desktopDir = new DirectoryNode(desktopFolder);
            RootFolders.Add(desktopDir);
            Logger.Debug($"Added Desktop folder: {desktopFolder}");
        }

        string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (!string.IsNullOrEmpty(documentsFolder) && Directory.Exists(documentsFolder)) {
            var docDir = new DirectoryNode(documentsFolder);
            RootFolders.Add(docDir);
            Logger.Debug($"Added Documents folder: {documentsFolder}");
        }

        // Add drives (Windows) or root directory (macOS/Linux)
        if (OperatingSystem.IsWindows()) {
            foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.IsReady)) {
                var driveDir = new DirectoryNode(drive.Name);
                RootFolders.Add(driveDir);
                Logger.Debug($"Added drive: {drive.Name}");
            }
        } else {
            var rootDir = new DirectoryNode("/");
            RootFolders.Add(rootDir);
            Logger.Debug("Added root folder: /");
        }

        Logger.Info($"Root folders initialized successfully. Total folders: {RootFolders.Count}");
    }
}
