using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Immutable;
using Avalonia.Media.Imaging;
using Navigator.UI.Utils;

namespace Navigator.UI.Models.Nodes;

public class BaseNode : ObservableObject {
    protected BaseNode(string path) {
        Path = path;
        Name = System.IO.Path.GetFileName(path);
        if (string.IsNullOrEmpty(Name)) {
            Name = path;
        }
    }

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

    public string Name { get; }
    public string Path { get; }

    public Bitmap Icon => IconProvider.GetIcon(this);

    // New property used by XAML to distinguish folders from files
    public virtual bool IsDirectory => false;

    public virtual string Type => "";
    public virtual string FormattedSize => "";
    public virtual string LastModified => "";
    public virtual ImmutableArray<BaseNode> Children => [];
    public virtual string DirectoryName => "";
}
