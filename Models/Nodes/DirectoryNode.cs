using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Navigator.Models.Nodes;

/// <summary>
/// Represents a folder item
/// </summary>
public partial class DirectoryNode : BaseNode {
    #region Properties ----------------------------------------------
    [ObservableProperty]
    private ImmutableArray<BaseNode> _children = [];
    #endregion

    #region Constructor ---------------------------------------------
    public DirectoryNode(string path, bool isRecursive = true) : base(path) {
        if (isRecursive)
            UpdateChildren();
    }
    #endregion

    #region Methods -------------------------------------------------
    public void UpdateChildren(bool isRecursive = false) {
        try {
            IEnumerable<BaseNode> directories = System.IO.Directory.GetDirectories(Path)
                .Select(dirPath => new DirectoryNode(dirPath, isRecursive));
            IEnumerable<BaseNode> files = System.IO.Directory.GetFiles(Path)
                .Select(filePath => new FileNode(filePath));
            Children = [..directories.Concat(files)];
            Logger.Debug($"Fetched {Children.Length} children for directory: {Path}");
        }
        catch (UnauthorizedAccessException) {
            Logger.Warning($"Access denied to directory: {Path}");
            Children = [];
        }
        catch (Exception ex) {
            Logger.Error($"Error accessing directory {Path}", ex);
            Children  = [];
        }
        OnPropertyChanged(nameof(Children));
    }
    #endregion

    #region Static Methods ------------------------------------------
    public new static string Icon => "📁";
    #endregion
}



