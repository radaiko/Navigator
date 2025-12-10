using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Navigator.Models.Nodes;

/// <summary>
///     Represents a folder item
/// </summary>
public partial class DirectoryNode : BaseNode {
    #region Constructor ---------------------------------------------

    public DirectoryNode(string path, bool isRecursive = true) : base(path) {
        var directoryInfo = new DirectoryInfo(path);
        _lastModified = directoryInfo.LastWriteTime;
        if (isRecursive) {
            UpdateChildren();
        }
    }

    #endregion

    #region Static Methods ------------------------------------------

    public new static string Icon => "📁";

    #endregion

    #region Methods -------------------------------------------------

    public void UpdateChildren(bool isRecursive = false) {
        try {
            IEnumerable<BaseNode> directories = Directory.GetDirectories(Path)
                .Select(dirPath => new DirectoryNode(dirPath, isRecursive));
            IEnumerable<BaseNode> files = Directory.GetFiles(Path)
                .Select(filePath => new FileNode(filePath));
            Children = [..directories.Concat(files)];
            Logger.Debug($"Fetched {Children.Length} children for directory: {Path}");
        } catch (UnauthorizedAccessException) {
            Logger.Warning($"Access denied to directory: {Path}");
            Children = [];
        } catch (Exception ex) {
            Logger.Error($"Error accessing directory {Path}", ex);
            Children = [];
        }

        OnPropertyChanged(nameof(Children));
    }

    #endregion

    #region Event Handler -------------------------------------------

    partial void OnIsExpandedChanged(bool value) {
        Logger.Debug($"OnIsExpandedChanged: {value}");
        if (value) {
            UpdateChildren(true);
        }
    }

    #endregion

    #region Properties ----------------------------------------------

    [ObservableProperty] private ImmutableArray<BaseNode> _children = [];
    [ObservableProperty] private bool _isExpanded;

    private readonly DateTime _lastModified;

    public override string Type => "Folder";
    public override string FormattedSize => "—";
    public override string LastModified => $"{_lastModified:yyyy-MM-dd HH:mm}";
    public override string DirectoryName => System.IO.Path.GetFileName(Path) ?? "";

    #endregion
}
