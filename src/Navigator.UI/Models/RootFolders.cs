using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Navigator.UI.Models.Nodes;

namespace Navigator.UI.Models;

public partial class RootFolders : ObservableObject {
    private static DirectoryNode TopLevel {
        get {
            if (field != null) {
                return field;
            }

            field = OperatingSystem.IsWindows() ? GetWindowsDrives() : GetUnixDrives();
            Logger.Debug($"Initialized TopLevel at path: {field.Path}");
            return field;
        }
    }

    [ObservableProperty] private DirectoryNode _actualNode = TopLevel;
    private readonly History _history = new();

    public void GoToPath(string path, bool ignoreHistory = false) {
        var normalizedPath = NormalizePath(path);
        var targetNode = TraverseToPath(normalizedPath);

        if (!Path.Exists(normalizedPath))
        {
            Logger.Debug($"Path '{normalizedPath}' does not exist.");
            return;
        }

        if (ActualNode.Path == targetNode.Path && !ignoreHistory) {
            Logger.Debug($"Ignoring navigation to same path: {targetNode.Path}");
            return;
        }

        targetNode.UpdateChildren();
        ActualNode = targetNode;
        Logger.Debug($"Set ActualNode to: {targetNode.Path}");

        if (!ignoreHistory)
            _history.Add(targetNode.Path);
    }

    public void GoUp() {
        if (IsAtTopLevel())
            return;

        var parentPath = Directory.GetParent(ActualNode.Path)?.FullName ?? TopLevel.Path;
        GoToPath(parentPath);
    }

    public void GoBack() {
        var previousPath = _history.TryGetPrevious();
        if (previousPath != null)
            GoToPath(previousPath, true);
    }

    public void GoForward() {
        var nextPath = _history.TryGetNext();
        if (nextPath != null)
            GoToPath(nextPath, true);
    }

    public void Refresh() {
        Logger.Debug($"Refreshing node: {ActualNode.Path}");
        ActualNode.UpdateChildren();
    }

    private string NormalizePath(string path) {
        if (string.IsNullOrWhiteSpace(path)) {
            return TopLevel.Path;
        }

        var trimmed = path.Trim();
        if (OperatingSystem.IsWindows()) {
            trimmed = trimmed.Replace('/', System.IO.Path.DirectorySeparatorChar);
            if (trimmed.Equals("Computer", StringComparison.OrdinalIgnoreCase)) {
                return TopLevel.Path;
            }

            if (!trimmed.StartsWith("Computer", StringComparison.OrdinalIgnoreCase)) {
                trimmed = System.IO.Path.Join("Computer", trimmed);
            }
        }

        return trimmed;
    }

    private DirectoryNode TraverseToPath(string path) {
        if (string.Equals(path, TopLevel.Path, StringComparison.OrdinalIgnoreCase)) {
            return TopLevel;
        }

        var segments = path.Split(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar)
            .Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

        DirectoryNode currentNode = TopLevel;
        foreach (var segment in segments.Skip(OperatingSystem.IsWindows() ? 1 : 0)) {
            var nextNode = currentNode.Children
                .OfType<DirectoryNode>()
                .FirstOrDefault(n => string.Equals(n.Name, segment, StringComparison.OrdinalIgnoreCase));

            if (nextNode == null) {
                Logger.Warning($"Segment '{segment}' not found under '{currentNode.Path}' while traversing '{path}'");
                throw new DirectoryNotFoundException($"The path '{path}' does not exist.");
            }

            currentNode = nextNode;
        }
        return currentNode;
    }

    private bool IsAtTopLevel() => ActualNode.Path == TopLevel.Path;

    private static DirectoryNode GetUnixDrives() {
        return new DirectoryNode("/");
    }

    private static DirectoryNode GetWindowsDrives() {
        List<DirectoryNode> drives = [];
        foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.IsReady)) {
            drives.Add(new DirectoryNode(drive.Name));
            Logger.Debug($"Added drive: {drive.Name}");
        }
        return new DirectoryNode("Computer", drives.ToArray());
    }
}

