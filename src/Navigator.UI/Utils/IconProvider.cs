using Avalonia.Media.Imaging;
using Navigator.UI.Converters;
using Navigator.UI.Models.Nodes;

namespace Navigator.UI.Utils;

public static class IconProvider {
    // Use a case-insensitive dictionary for extensions and initialize properly
    private static readonly Dictionary<string, Bitmap> IconCache = new Dictionary<string, Bitmap>(StringComparer.OrdinalIgnoreCase);

    public static Bitmap GetIcon(BaseNode? node)
    {
        Logger.Debug($"GetIcon called: nodeType={(node == null ? "<null>" : node.GetType().Name)} path={(node == null ? "<null>" : node.Path)}");
        if (node == null) {
            Logger.Debug("GetIcon: node is null, returning default icon");
            return GetDefaultIcon();
        }
        if (node is DirectoryNode) {
            var dirIcon = GetDirectoryIcon();
            Logger.Debug($"GetIcon returning directory icon for path={(node.Path ?? "<null>")}");
            return dirIcon;
        }
        var path = node.Path ?? string.Empty;
        var fileIcon = GetFileIcon(path);
        Logger.Debug($"GetIcon returning file icon for path={path}");
        return fileIcon;
    }

    private static Bitmap GetDirectoryIcon(int size = 256) {
        Logger.Debug($"GetDirectoryIcon called: size={size}");
        if (IconCache.TryGetValue("directory", out var bitmap)) {
            Logger.Debug("GetDirectoryIcon cache hit: directory");
            return bitmap;
        }

        Logger.Debug("GetDirectoryIcon cache miss: generating directory icon");
        var byteArray = "📁"u8.ToArray();
        // Convert to bitmap
        var converter = new ByteArrayToBitmapConverter();
        var newBitmap = (Bitmap)converter.Convert(byteArray, typeof(Bitmap), null, null)!;
        IconCache["directory"] = newBitmap;
        Logger.Debug("GetDirectoryIcon generated and cached: directory");
        return newBitmap;
    }

    private static Bitmap GetFileIcon(string filePath, int size = 256) {
        Logger.Debug($"GetFileIcon called: filePath={filePath} size={size}");
        var extension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension)) {
            Logger.Debug($"GetFileIcon: no extension for path={filePath}");
            return GetDefaultIcon();
        }

        // Normalize extension to ensure consistent cache keys
        extension = extension.ToLowerInvariant();

        if (IconCache.TryGetValue(extension, out var bitmap)) {
            Logger.Debug($"GetFileIcon cache hit: extension={extension}");
            return bitmap;
        }


        Logger.Debug($"GetFileIcon cache miss: extension={extension}; generating via GetFileIconBitmap");
        var newBitmap = GetFileIconBitmap(filePath, size);
        IconCache[extension] = newBitmap;
        Logger.Debug($"GetFileIcon cached new icon for extension={extension}");
        return newBitmap;
    }

    private static Bitmap GetFileIconBitmap(string filePath, int size = 256) {
        Logger.Debug($"GetFileIconBitmap called: filePath={filePath} size={size} OS={Environment.OSVersion}");
        if (OperatingSystem.IsMacOS()) {
            Logger.Debug("GetFileIconBitmap: macOS branch - scanning loaded assemblies for Navigator.Mac.FileIconGen");
            // Avoid a compile-time project reference to Navigator.Mac (it references Navigator.UI)
            // Find the type at runtime and invoke its static method via reflection.
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                var t = asm.GetType("Navigator.Mac.FileIconGen", false);
                if (t == null) continue;
                Logger.Debug($"GetFileIconBitmap: found type Navigator.Mac.FileIconGen in assembly {asm.FullName}");
                var method = t.GetMethod("GetFileIconBitmap", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (method == null) {
                    Logger.Debug($"GetFileIconBitmap: method GetFileIconBitmap not found on type in assembly {asm.FullName}");
                    continue;
                }
                Logger.Debug($"GetFileIconBitmap: invoking GetFileIconBitmap on assembly {asm.FullName}");
                try {
                    var result = method.Invoke(null, new object[] { filePath, size });
                    if (result is Bitmap bmp) {
                        Logger.Debug($"GetFileIconBitmap: received Bitmap from Navigator.Mac.FileIconGen for path={filePath}");
                        return bmp;
                    }
                    Logger.Debug($"GetFileIconBitmap: invocation returned null or unexpected type for path={filePath}");
                } catch (System.Reflection.TargetInvocationException tie) {
                    Logger.Debug($"GetFileIconBitmap: TargetInvocationException invoking Navigator.Mac.FileIconGen: {tie.InnerException ?? tie}");
                    throw tie.InnerException ?? tie;
                } catch (Exception ex) {
                    Logger.Debug($"GetFileIconBitmap: Exception invoking Navigator.Mac.FileIconGen: {ex}");
                    throw;
                }
            }

            Logger.Debug("GetFileIconBitmap: Navigator.Mac.FileIconGen not found in loaded assemblies");
            throw new NotImplementedException("Navigator.Mac.FileIconGen not found in loaded assemblies at runtime.");
        }

        if (OperatingSystem.IsWindows()) {
            Logger.Debug($"GetFileIconBitmap: Windows branch not implemented for path={filePath}");
            throw new NotImplementedException("Windows implementation not yet available.");
        }
        if (OperatingSystem.IsLinux()) {
            Logger.Debug($"GetFileIconBitmap: Linux branch not implemented for path={filePath}");
            throw new NotImplementedException("Linux implementation not yet available.");
        }
        Logger.Debug($"GetFileIconBitmap: unsupported operating system: {Environment.OSVersion}");
        throw new NotImplementedException($"Operating system not supported: {Environment.OSVersion}");
    }

    private static Bitmap GetDefaultIcon()
    {
        Logger.Debug("GetDefaultIcon called");
        if (IconCache.TryGetValue("defaultIcon", out var bitmap)) {
            Logger.Debug("GetDefaultIcon cache hit: defaultIcon");
            return bitmap;
        }

        Logger.Debug("GetDefaultIcon cache miss: generating default icon");
        var byteArray = "📄"u8.ToArray();
        // Convert to bitmap
        var converter = new ByteArrayToBitmapConverter();
        var newBitmap = (Bitmap)converter.Convert(byteArray, typeof(Bitmap), null, null)!;
        IconCache["defaultIcon"] = newBitmap;
        Logger.Debug("GetDefaultIcon generated and cached: defaultIcon");
        return newBitmap;
    }
}
