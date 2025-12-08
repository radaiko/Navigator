using CommunityToolkit.Mvvm.ComponentModel;

namespace Navigator.Models.Nodes;

public class BaseNode : ObservableObject {

    #region Properties ----------------------------------------------
    public string Name { get; }
    public string Path { get; }

    public string Icon => this is FileNode ? FileNode.Icon : DirectoryNode.Icon;
    #endregion

    protected BaseNode(string path) {
        Path = path;
        Name = System.IO.Path.GetFileName(path);
        if (string.IsNullOrEmpty(Name)) {
            Name = path; // Root directory case
        }
    }

    #region Static Methods ------------------------------------------
    internal static string FormatBytes(long bytes) {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1) {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
    #endregion
}
