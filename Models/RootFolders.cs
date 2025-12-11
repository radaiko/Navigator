using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Navigator.Models.Nodes;

namespace Navigator.Models;

public partial class RootFolders : ObservableObject, IEnumerable<DirectoryNode> {
    public static DirectoryNode TopLevel {
        get {
            field ??= OperatingSystem.IsWindows() ? GetWindowsDrives() : GetUnixDrives();
            return field;
        }
    }

    [ObservableProperty] private DirectoryNode _actualNode = TopLevel;
    private static List<string>? _history = [];

    // Navigates to the specified path and updates ActualNode accordingly,
    // throwing DirectoryNotFoundException if the path does not exist.
    // Starts from TopLevel and traverses down the directory tree until it hits the target path.
    // If the target path is a directory it set ActualNode to that directory node.
    public void GoToPath(string path) {
        if (OperatingSystem.IsWindows()) {
            if (!path.StartsWith("Computer")) path = Path.Combine("Computer", path);
        }

        var segments = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Where(s => !string.IsNullOrEmpty(s)).ToArray();
        DirectoryNode currentNode = TopLevel;
        foreach (var segment in segments) {
            var nextNode = currentNode.Children
                .OfType<DirectoryNode>()
                .FirstOrDefault(n => string.Equals(n.Name, segment, StringComparison.OrdinalIgnoreCase));
            if (nextNode == null) {
                throw new DirectoryNotFoundException($"The path '{path}' does not exist.");
            }
            currentNode = nextNode;
        }
        ActualNode = currentNode;
        ActualNode.UpdateChildren();
        Logger.Debug($"Set ActualNode to: {currentNode.Path}");
        AddToHistory(currentNode.Path);
    }

    public void GoUp() {
        if (ActualNode.Path == TopLevel.Path) {
            return; // Already at top level
        }

        var parentPath = Directory.GetParent(ActualNode.Path)?.FullName;
        if (parentPath == null) {
            ActualNode = TopLevel;
            return;
        }

        GoToPath(parentPath);
    }

    private void AddToHistory(string path) {
        if (_history == null) {
            _history = [];
        }
        if (_history is { Count: > 100 }) {
            _history.RemoveAt(0);
        }
        Logger.Debug($"Adding to history: {path}");
        _history.Add(path);
    }

    private static DirectoryNode GetUnixDrives() {
        return new DirectoryNode("/");
    }

    private static DirectoryNode GetWindowsDrives() {
        List<DirectoryNode> drives = [];
        foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.IsReady)) {
            var driveDir = new DirectoryNode(drive.Name);
            drives.Add(driveDir);
            Logger.Debug($"Added drive: {drive.Name}");
        }
        var rootNode = new DirectoryNode("Computer", drives.ToArray());
        return rootNode;
    }

    public IEnumerator<DirectoryNode> GetEnumerator() {
        yield return TopLevel;
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
