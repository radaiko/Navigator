using System;
using System.IO;
using System.Runtime.InteropServices;
using Navigator.UI.Utils;
using Avalonia.Threading;

namespace Navigator.UI.Models.Nodes;

public class FileNode : BaseNode {
    public FileNode(string path) : base(path) {
        var fileInfo = new FileInfo(path);
        _size = fileInfo.Length;
        _lastModified = fileInfo.LastWriteTime;
        Type = fileInfo.Extension.ToUpper().TrimStart('.') + " File";

        try {
            FileExtensions.IconUpdated += OnIconUpdated;
        } catch {
        }
    }

    private void OnIconUpdated(string updatedPath) {
        try {
            var a = System.IO.Path.GetFullPath(updatedPath);
            var b = System.IO.Path.GetFullPath(Path);
            var comparison = StringComparison.OrdinalIgnoreCase;
            if (!string.Equals(a, b, comparison))
                return;
        } catch {
            if (!string.Equals(updatedPath, Path, StringComparison.Ordinal))
                return;
        }
        try {
            Dispatcher.UIThread.Post(() => OnPropertyChanged(nameof(Icon)));
        } catch {
            try { OnPropertyChanged(nameof(Icon)); } catch { }
        }
    }

    private readonly long _size;
    private readonly DateTime _lastModified;

    public override string Type { get; }
    public override string FormattedSize => FormatBytes(_size);
    public override string LastModified => $"{_lastModified:yyyy-MM-dd HH:mm}";
    public DirectoryNode? Parent { get; internal set; }
    public override string DirectoryName => Parent?.Name ?? System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(Path)) ?? "";
}

