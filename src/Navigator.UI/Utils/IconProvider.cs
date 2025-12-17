using Avalonia.Media.Imaging;
using Navigator.UI.Converters;
using Navigator.UI.Models.Nodes;

namespace Navigator.UI.Utils;

public static class IconProvider {
    // Use a case-insensitive dictionary for extensions and initialize properly
    private static readonly Dictionary<string, Bitmap> IconCache = new Dictionary<string, Bitmap>(StringComparer.OrdinalIgnoreCase);

    public static Bitmap GetIcon(BaseNode node)
    {
        if (node is DirectoryNode) return GetDirectoryIcon();
        return GetFileIcon(node.Path);
    }

    private static Bitmap GetDirectoryIcon(int size = 256) {
        if (IconCache.TryGetValue("directory", out var bitmap))
            return bitmap;

        var byteArray = "📁"u8.ToArray();
        // Convert to bitmap
        var converter = new ByteArrayToBitmapConverter();
        var newBitmap = (Bitmap)converter.Convert(byteArray, typeof(Bitmap), null, null)!;
        IconCache["directory"] = newBitmap;
        return newBitmap;
    }

    private static Bitmap GetFileIcon(string filePath, int size = 256) {
        var extension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension))
            throw new NotImplementedException("Default icon for files without extension not yet implemented.");

        // Normalize extension to ensure consistent cache keys
        extension = extension.ToLowerInvariant();

        if (IconCache.TryGetValue(extension, out var bitmap))
            return bitmap;


        var newBitmap = GetFileIconBitmap(filePath, size);
        IconCache[extension] = newBitmap;
        return newBitmap;
    }

    private static Bitmap GetFileIconBitmap(string filePath, int size = 256) {
        if (OperatingSystem.IsMacOS()) {
            // Avoid a compile-time project reference to Navigator.Mac (it references Navigator.UI)
            // Find the type at runtime and invoke its static method via reflection.
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                var t = asm.GetType("Navigator.Mac.FileIconGen", false);
                if (t == null) continue;
                var method = t.GetMethod("GetFileIconBitmap", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (method == null) continue;
                try {
                    var result = method.Invoke(null, new object[] { filePath, size });
                    if (result is Bitmap bmp)
                        return bmp;
                } catch (System.Reflection.TargetInvocationException tie) {
                    throw tie.InnerException ?? tie;
                }
            }

            throw new NotImplementedException("Navigator.Mac.FileIconGen not found in loaded assemblies at runtime.");
        }

        if (OperatingSystem.IsWindows())
            throw new NotImplementedException("Windows implementation not yet available.");
        if (OperatingSystem.IsLinux())
            throw new NotImplementedException("Linux implementation not yet available.");
        throw new NotImplementedException($"Operating system not supported: {Environment.OSVersion}");
    }

    private static Bitmap GetDefaultIcon()
    {
        if (IconCache.TryGetValue("defaultIcon", out var bitmap))
            return bitmap;

        var byteArray = "📄"u8.ToArray();
        // Convert to bitmap
        var converter = new ByteArrayToBitmapConverter();
        var newBitmap = (Bitmap)converter.Convert(byteArray, typeof(Bitmap), null, null)!;
        IconCache["defaultIcon"] = newBitmap;
        return newBitmap;
    }
}
