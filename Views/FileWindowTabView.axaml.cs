using Avalonia.Controls;
using Navigator.Controls;
using Navigator.Models;
using Navigator.ViewModels;

namespace Navigator.Views;

public partial class FileWindowTabView : UserControl
{
    private TableView? _fileTableView;
    private FileWindowTabViewModel? _viewModel;

    public FileWindowTabView()
    {
        InitializeComponent();
    }

    public FileWindowTabView(FileWindowTab model) : this()
    {
        _viewModel = new FileWindowTabViewModel(model);
        DataContext = _viewModel;
    }

    protected override void OnInitialized() {
        base.OnInitialized();
        Logger.Debug("FileWindowTabView OnInitialized called");

        _fileTableView = this.FindControl<TableView>("FileTableView");

        if (_fileTableView != null && _viewModel != null) {
            // Subscribe to TreeView node selection changes
            var treeViewItems = _viewModel.TreeViewItems;
            if (treeViewItems.Count > 0) {
                // Load the home folder by default
                _fileTableView.CurrentPath = treeViewItems[0].Path;
                Logger.Info($"Initialized TableView with default path: {treeViewItems[0].Path}");
            }

            // When a folder is opened in the table view, update the path
            _fileTableView.FolderOpened += (folderPath) => {
                Logger.Info($"TableView folder opened: {folderPath}");
            };

            // When a file is clicked in the table view
            _fileTableView.FileClicked += (filePath) => {
                Logger.Info($"TableView file clicked: {filePath}");
                // TODO: Implement file opening/execution
            };
        } else {
            Logger.Warning("FileTableView or ViewModel not found during initialization");
        }
    }
}

