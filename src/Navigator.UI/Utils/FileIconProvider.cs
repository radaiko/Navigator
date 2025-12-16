using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;

namespace Navigator.UI.Utils;

public static class FileIconProvider {
    private static readonly Dictionary<string, Bitmap> IconCache = [];

    public static Bitmap GetFileIcon(string filePath, int size = 256) {
        var extension = System.IO.Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension))
            throw new NotImplementedException("Default icon for files without extension not yet implemented.");

        if (IconCache.TryGetValue(extension, out var bitmap))
            return bitmap;


        var newBitmap = GetFileIconBitmap(filePath, size);
        IconCache[extension] = newBitmap;
        return newBitmap;
    }

    private static Bitmap GetFileIconBitmap(string filePath, int size = 256) {
        if (OperatingSystem.IsMacOS())
            return Mac.FileIconGen.GetFileIconBitmap(filePath, size);
        if (OperatingSystem.IsWindows())
            throw new NotImplementedException("Windows implementation not yet available.");
        if (OperatingSystem.IsLinux())
            throw new NotImplementedException("Linux implementation not yet available.");
        throw new NotImplementedException($"Operating system not supported: {Environment.OSVersion}");
    }
}

