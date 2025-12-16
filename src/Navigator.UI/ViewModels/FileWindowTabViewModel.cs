using System.Collections.Immutable;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Navigator.UI.Models;
using Navigator.UI.Models.Nodes;

namespace Navigator.UI.ViewModels;

public partial class FileWindowTabViewModel : ViewModelBase {

    #region Fields and Properties ----------------------------------
    [ObservableProperty] private RootFolders _root = new();

    [ObservableProperty] private ImmutableArray<BaseNode> _currentNodeChildren = [];

    [ObservableProperty] private string _currentFolderName = string.Empty;

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
        CurrentFolderName = actual.Name;
    }

    private static void LogDebug(string message) => Logger.Debug(message);
    #endregion
}

