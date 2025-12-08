using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Navigator.Models;
using Navigator.Models.Nodes;

namespace Navigator.Controls;

public partial class TableView : UserControl {
    private ItemsControl? _fileItemsContainer;
    private ObservableCollection<BaseNode> _items = [];
    private string _currentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public static readonly StyledProperty<string> CurrentPathProperty =
        AvaloniaProperty.Register<TableView, string>(
            nameof(CurrentPath),
            defaultValue: "");

    public string CurrentPath {
        get => GetValue(CurrentPathProperty);
        set => SetValue(CurrentPathProperty, value);
    }

    public ObservableCollection<BaseNode> Items => _items;

    public event Action<string>? FolderOpened;
    public event Action<string>? FileClicked;

    public TableView() {
        InitializeComponent();
        // Set the data context to self for binding
        DataContext = this;
    }

    protected override void OnInitialized() {
        base.OnInitialized();
        Logger.Debug("TableView OnInitialized called");
        _fileItemsContainer = this.FindControl<ItemsControl>("FileItemsContainer");

        LoadDirectory(_currentPath);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);

        if (change.Property == CurrentPathProperty) {
            var newPath = change.NewValue as string;
            if (!string.IsNullOrEmpty(newPath)) {
                Logger.Info($"CurrentPath property changed to: {newPath}");
                LoadDirectory(newPath);
            }
        }
    }

    private void OnItemContainerDoubleClicked(BaseNode item) {
        if (item is FileNode fileItem) {
            OnFileClicked(fileItem);
        }
        else if (item is DirectoryNode directoryItem) {
            OpenFolder(directoryItem.Path);
        }
    }

    private void OnItemContainerContextMenu(BaseNode item) {
        Logger.Debug($"Context menu requested for: {item.Name}");
    }

    private void LoadDirectory(string directoryPath) {
        _currentPath = directoryPath;

        _items.Clear();

        if (!System.IO.Directory.Exists(directoryPath)) {
            Logger.Warning($"Directory does not exist: {directoryPath}");
            return;
        }

        try {
            var directoryInfo = new DirectoryInfo(directoryPath);
            var allItems = new List<BaseNode>();

            // Add folders first
            try {
                var folders = directoryInfo.GetDirectories()
                    .OrderBy(d => d.Name)
                    .Select(d => new DirectoryNode(d.FullName));
                allItems.AddRange(folders);
            } catch (UnauthorizedAccessException ex) {
                Logger.Warning($"Cannot access folders in {directoryPath}: {ex.Message}");
            }

            // Add files
            try {
                var files = directoryInfo.GetFiles()
                    .OrderBy(f => f.Name)
                    .Select(f => new FileNode(f.FullName));
                allItems.AddRange(files);
            } catch (UnauthorizedAccessException ex) {
                Logger.Warning($"Cannot access files in {directoryPath}: {ex.Message}");
            }

            foreach (BaseNode item in allItems) {
                _items.Add(item);
            }

            Logger.Info($"Loaded {_items.Count} items from {directoryPath}");
        } catch (Exception ex) {
            Logger.Error($"Error loading directory {directoryPath}: {ex.Message}", ex);
        }
    }

    private string GetFileType(string fileName) {
        var extension = System.IO.Path.GetExtension(fileName).ToLower();
        return string.IsNullOrEmpty(extension) ? "File" : extension.TrimStart('.').ToUpper() + " File";
    }

    public void OpenFolder(string folderPath) {
        if (System.IO.Directory.Exists(folderPath)) {
            CurrentPath = folderPath;
            FolderOpened?.Invoke(folderPath);
        }
    }

    private void OnFileClicked(FileNode fileNode) {
        Logger.Info($"File clicked: {fileNode.Name} ({fileNode.Path})");
        FileClicked?.Invoke(fileNode.Path);
        // TODO: Implement the actual file execution logic here
    }
}


