using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Navigator.UI.Utils;

namespace Navigator.UI.Models.Nodes;

public partial class DirectoryNode : BaseNode {
    public DirectoryNode(string path, DirectoryNode[] children) : base(path) {
        var directoryInfo = new DirectoryInfo(path);
        _lastModified = directoryInfo.LastWriteTime;
        _children = [.. children];
        foreach (var child in _children.OfType<DirectoryNode>()) {
            child._parent = this;
        }
    }

    public DirectoryNode(string path, bool isRecursive = true) : base(path) {
        var directoryInfo = new DirectoryInfo(path);
        _lastModified = directoryInfo.LastWriteTime;
        if (isRecursive) {
            UpdateChildren();
        }
    }

    public void UpdateChildren(bool isRecursive = false) {
        if (!Directory.Exists(Path)) {
            Logger.Warning($"Directory no longer exists: {Path}");
            Children = [];
            return;
        }

        try {
            IEnumerable<BaseNode> directories = Directory.GetDirectories(Path)
                .Select(dirPath => new DirectoryNode(dirPath, isRecursive) { _parent = this });
            IEnumerable<BaseNode> files = Directory.GetFiles(Path)
                .Select(filePath => new FileNode(filePath) { Parent = this });
            // TODO: add here special case detection for symlinks to avoid cycles
            // TODO: add here filtering based on user preferences (hidden files, file types, etc.)
            // TODO: add here error handling for individual files/directories that can't be accessed
            // TODO: add here special mac case detection ".App" directories ar executable bundles

            Children = NodeSorter.Sort([.. directories.Concat(files)]);
            Logger.Debug($"Fetched {Children.Length} children for directory: {Path}");
        } catch (UnauthorizedAccessException) {
            Logger.Warning($"Access denied to directory: {Path}");
            Children = [];
        } catch (Exception ex) {
            Logger.Error($"Error accessing directory {Path}", ex);
            Children = [];
        }

        OnPropertyChanged(nameof(Children));
        // Notify count properties so UI bindings update when Children change
        OnPropertyChanged(nameof(FileCount));
        OnPropertyChanged(nameof(DirectoryCount));
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(FormattedCounts));
    }

    partial void OnIsExpandedChanged(bool value) {
        Logger.Debug($"OnIsExpandedChanged: {value}");
        if (value) {
            UpdateChildren(true);
        }
    }

    [ObservableProperty] private ImmutableArray<BaseNode> _children = [];
    [ObservableProperty] private bool _isExpanded;

    private readonly DateTime _lastModified;

    public override string Type => "Folder";
    public override string FormattedSize => "—";
    public override string LastModified => $"{_lastModified:yyyy-MM-dd HH:mm}";
    public override string DirectoryName => System.IO.Path.GetFileName(Path);

    public DirectoryNode? Parent {
        get => _parent ??= ResolveParent();
    }

    private DirectoryNode? ResolveParent() {
        var parentDir = Directory.GetParent(Path);
        return parentDir == null ? null : new DirectoryNode(parentDir.FullName, false);
    }

    private DirectoryNode? _parent;

    // Public accessor for sorting by last modified
    public DateTime LastModifiedDate => _lastModified;

    public int CountOfFiles() {
        return Children.Count(node => node is FileNode);
    }

    public int CountOfDirectories() {
        return Children.Count(node => node is DirectoryNode);
    }

    public int CountOfAllItems() {
        return Children.Length;
    }

    // Read-only properties for data binding (preferred over calling methods from XAML)
    public int FileCount => CountOfFiles();
    public int DirectoryCount => CountOfDirectories();
    public int TotalCount => CountOfAllItems();

    // Pre-formatted status string for easy binding in the view
    public string FormattedCounts => $"Files: {FileCount}   Dirs: {DirectoryCount}   Total: {TotalCount}";
}
