using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Navigator.Models.Nodes;

/// <summary>
///     Represents a file item
/// </summary>
public partial class FileNode : BaseNode {
    #region Constructor -------------------------------------------

    public FileNode(string path) : base(path) {
        var fileInfo = new FileInfo(path);
        _size = fileInfo.Length;
        _lastModified = fileInfo.LastWriteTime;
        Type = fileInfo.Extension.ToUpper().TrimStart('.') + " File";
    }

    #endregion

    #region Static Methods -----------------------------------------

    public new static string Icon => "📄";

    #endregion

    #region Fields ------------------------------------------------

    private readonly long _size;
    private readonly DateTime _lastModified;

    #endregion

    #region Properties -------------------------------------------

    public override string Type { get; }
    public override string FormattedSize => FormatBytes(_size);
    public override string LastModified => $"{_lastModified:yyyy-MM-dd HH:mm}";
    public DirectoryNode? Parent { get; internal set; }
    public override string DirectoryName => Parent?.Name ?? System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(Path)) ?? "";

    #endregion
}
