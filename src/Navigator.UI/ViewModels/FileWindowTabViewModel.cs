using System.Collections.Immutable;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Navigator.UI.Models;
using Navigator.UI.Models.Nodes;

namespace Navigator.UI.ViewModels;

public partial class FileWindowTabViewModel : ViewModelBase {

    #region Fields and Properties ----------------------------------
    [ObservableProperty] private RootFolders _root = new();

    [ObservableProperty] private ImmutableArray<BaseNode> _currentNodeChildren = [];

    [ObservableProperty] private string _tabName = string.Empty;

    // New selected node set when user right-clicks an item
    [ObservableProperty] private BaseNode? _selectedNode;

    private readonly FileWindowTab _model;
    #endregion

    #region Constructor / Initialization ---------------------------
    public FileWindowTabViewModel(FileWindowTab model) {
        _model = model;
        Initialize();
    }

    private void Initialize() {
        Logger.Info("Initializing FileWindowTabViewModel");
        UpdateStateFromRoot();
        SubscribeToRootPropertyChanged();
        LogDebug($"Assigned model instance: {_model.GetHashCode()}");
        Logger.Debug("FileWindowTabViewModel initialized successfully");
    }

    #endregion

    #region Commands ------------------------------------------------
    [RelayCommand]
    private void DoubleClickItem(BaseNode node) {
        LogDebug($"Double-clicked on directory: {node.Path}");
        if (node is not DirectoryNode directory) {
            LogDebug("Item is not a directory. No action taken.");
            return;
        }

        Root.GoToPath(directory.Path);
        LogDebug($"Navigated to directory: {directory.Path}");
    }

    [RelayCommand]
    private void BackClicked() {
        Root.GoBack();
        LogDebug("Back clicked");
    }

    [RelayCommand]
    private void UpClicked() {
        Root.GoUp();
        LogDebug("Up clicked");
    }

    [RelayCommand]
    private void ForwardClicked() {
        Root.GoForward();
        LogDebug("Forward clicked");
    }

    [RelayCommand]
    private void RefreshClicked() {
        Root.Refresh();
        LogDebug("Refresh clicked");
    }

    [RelayCommand]
    private void HomeClicked() {
        Root.GoHome();
        LogDebug("Home clicked");
    }

    [RelayCommand]
    private void NewFolderClicked()
    {
        Root.CreateNewFolder();
    }

    // Context menu commands
    [RelayCommand]
    private void Open(BaseNode node) {
        if (node is DirectoryNode d) {
            Root.GoToPath(d.Path);
            LogDebug($"ContextMenu Open -> navigated to folder: {d.Path}");
            return;
        }

        // TODO: implement file opening with registered handler
        LogDebug($"Open file requested: {node.Path}");
    }

    [RelayCommand]
    private void CopyPath(BaseNode node) {
        // For now just log the path. Platform clipboard access can be added via a service.
        LogDebug($"CopyPath requested: {node.Path}");
    }

    [RelayCommand]
    private void NewFolder() {
        Root.CreateNewFolder();
    }

    [RelayCommand]
    private void Delete(BaseNode node) {
        try {
            if (node is DirectoryNode) {
                System.IO.Directory.Delete(node.Path, true);
            } else {
                System.IO.File.Delete(node.Path);
            }

            Root.Refresh();
            LogDebug($"Deleted node: {node.Path}");
        } catch (Exception ex) {
            Logger.Error($"Failed to delete node: {node.Path}", ex);
        }
    }

    [RelayCommand]
    private void Rename(BaseNode node) {
        // TODO: show rename dialog. For now log and refresh
        LogDebug($"Rename requested for: {node.Path}");
    }

    [RelayCommand]
    private void Properties(BaseNode node) {
        // TODO: show properties dialog
        LogDebug($"Properties requested for: {node.Path}");
    }
    #endregion

    #region Private Helper Methods ---------------------------------
    private void SubscribeToRootPropertyChanged() {
        Root.PropertyChanged -= OnRootPropertyChanged;
        Root.PropertyChanged += OnRootPropertyChanged;
    }

    private void OnRootPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(RootFolders.ActualNode)) {
            UpdateStateFromRoot();
        }
    }

    private void UpdateStateFromRoot() {
        DirectoryNode actual = Root.ActualNode;
        CurrentNodeChildren = actual.Children;
        TabName = actual.Name;
    }

    private static void LogDebug(string message) => Logger.Debug(message);
    #endregion
}
