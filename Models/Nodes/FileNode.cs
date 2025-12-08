using System;

namespace Navigator.Models.Nodes;

/// <summary>
/// Represents a file item
/// </summary>
public class FileNode : BaseNode {
    #region Fields --------------------------------------------------
    private readonly long _size;
    private readonly DateTime _lastModified;
    #endregion

    #region Properties ----------------------------------------------
    public string Type { get; }
    public string FormattedSize => FormatBytes(_size);
    public string LastModified => $"{_lastModified:yyyy-MM-dd HH:mm}";
    #endregion

    #region Constructor ------------------------------------------------
    public FileNode(string path) : base(path) {
        var fileInfo = new System.IO.FileInfo(path);
        _size = fileInfo.Length;
        _lastModified = fileInfo.LastWriteTime;
        Type = fileInfo.Extension.ToUpper().TrimStart('.') + " File";
    }
    #endregion

    #region Static Methods ------------------------------------------
    public new static string Icon => "📄";
    #endregion
}
