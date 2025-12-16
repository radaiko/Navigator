using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Avalonia.Threading;

namespace Navigator.UI.Utils;

public static class FileExtensions {
    public static event Action<string>? IconUpdated;

    private class CachedIcon {
        public byte[] Bytes { get; }
        public DateTime LastWriteUtc { get; }
        public long Size { get; }
        public DateTime CreatedAt { get; }
        public CachedIcon(byte[] bytes, DateTime lastWriteUtc) {
            Bytes = bytes;
            LastWriteUtc = lastWriteUtc;
            Size = bytes.Length;
            CreatedAt = DateTime.UtcNow;
        }
    }

    private static readonly ConcurrentDictionary<string, CachedIcon> _macIconCache = new();
    private static readonly SemaphoreSlim _qlmanageSemaphore = new(1, 1);
    private static readonly ConcurrentDictionary<string, object> _pathLocks = new();
    private const int MAX_CACHE_ENTRIES = 500;
    private static readonly TimeSpan CACHE_TTL = TimeSpan.FromMinutes(10);
    private static readonly string CACHE_DIR = Path.Combine(Path.GetTempPath(), "nav_icon_cache");
    private static readonly ConcurrentDictionary<string, bool> _generationInProgress = new();

    extension(File) {
        public static byte[]? GetIcon(string path, int size = 64) {
            Logger.Debug($"GetIcon called: path={path} size={size}");
            var sw = Stopwatch.StartNew();
            try {
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) {
                    Logger.Debug($"GetIcon early return: path missing or does not exist: {path}");
                    return null;
                }

                byte[]? result;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    Logger.Debug($"GetIcon: using Windows branch for {path}");
                    result = GetWindowsIcon(path, size);
                } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                    Logger.Debug($"GetIcon: using macOS branch for {path}");
                    result = GetMacIconWithQlmanage(path, size);
                } else {
                    Logger.Debug($"GetIcon: unsupported platform for {path}");
                    result = null;
                }

                sw.Stop();
                Logger.Debug($"GetIcon finished: path={path} elapsed={sw.ElapsedMilliseconds}ms result={(result != null ? "ok" : "null")}");
                return result;
            } catch (Exception ex) {
                sw.Stop();
                Logger.Debug($"GetIcon exception: path={path} elapsed={sw.ElapsedMilliseconds}ms ex={ex}");
                return null;
            }
        }
    }

    private static byte[]? GetWindowsIcon(string path, int size) {
        var sw = Stopwatch.StartNew();
        Logger.Debug($"GetWindowsIcon START: path={path} size={size}");
        try {
            var flags = SHGFI_ICON | (size <= 32 ? SHGFI_SMALLICON : SHGFI_LARGEICON);
            SHFILEINFO shfi = new();
            SHGetFileInfo(path, 0, out shfi, (uint)Marshal.SizeOf(shfi), flags);
            Logger.Debug($"SHGetFileInfo returned hIcon={(shfi.hIcon == IntPtr.Zero ? "Zero" : shfi.hIcon.ToString())}");
            if (shfi.hIcon == IntPtr.Zero) {
                sw.Stop();
                Logger.Debug($"GetWindowsIcon: no icon for {path} elapsed={sw.ElapsedMilliseconds}ms");
                return null;
            }

            DestroyIcon(shfi.hIcon);
            sw.Stop();
            Logger.Debug($"GetWindowsIcon FINISH (icon freed): path={path} elapsed={sw.ElapsedMilliseconds}ms");
            return null;
        } catch (Exception ex) {
            sw.Stop();
            Logger.Debug($"GetWindowsIcon EXCEPTION: path={path} elapsed={sw.ElapsedMilliseconds}ms ex={ex}");
            return null;
        }
    }

    private static byte[]? GetMacIconWithQlmanage(string path, int size) {
        var requestSw = Stopwatch.StartNew();
        DateTime lastWriteUtc = File.GetLastWriteTimeUtc(path);

        if (_macIconCache.TryGetValue(path, out var cached) && cached.LastWriteUtc == lastWriteUtc && (DateTime.UtcNow - cached.CreatedAt) <= CACHE_TTL) {
            Logger.Debug($"GetMacIconWithQlmanage CACHE HIT: path={path} size={cached.Size} created={cached.CreatedAt} elapsed={requestSw.ElapsedMilliseconds}ms");
            return cached.Bytes;
        }

        Logger.Debug($"GetMacIconWithQlmanage CACHE MISS: path={path} lastWriteUtc={lastWriteUtc}");

        try {
            Directory.CreateDirectory(CACHE_DIR);
            string key = ComputeKey(path, size);
            string diskPng = Path.Combine(CACHE_DIR, key + ".png");
            string diskMeta = Path.Combine(CACHE_DIR, key + ".meta");
            if (File.Exists(diskPng) && File.Exists(diskMeta)) {
                try {
                    var metaText = File.ReadAllText(diskMeta);
                    if (long.TryParse(metaText, out var ticks) && ticks == lastWriteUtc.Ticks) {
                        var bytes = File.ReadAllBytes(diskPng);
                        var newCached = new CachedIcon(bytes, lastWriteUtc);
                        _macIconCache[path] = newCached;
                        Logger.Debug($"GetMacIconWithQlmanage DISK CACHE HIT: {path} size={bytes.Length}");
                        return bytes;
                    }
                } catch (Exception ex) {
                    Logger.Debug($"Error reading disk cache for {path}: {ex}");
                }
            }
        } catch (Exception ex) {
            Logger.Debug($"Error ensuring cache dir: {ex}");
        }

        Logger.Debug($"GetMacIconWithQlmanage: proceeding to qlmanage for {path}");

        TryStartBackgroundGeneration(path, size, lastWriteUtc);
        return null;
    }

    private static void TryStartBackgroundGeneration(string path, int size, DateTime lastWriteUtc) {
        string key = path + "|" + size;
        if (!_generationInProgress.TryAdd(key, true)) {
            Logger.Debug($"Background generation already in progress for {path}");
            return;
        }

        Logger.Debug($"Enqueued background generation for {path}");
        Task.Run(() => {
            try {
                GenerateAndCacheIcon(path, size, lastWriteUtc);
            } catch (Exception ex) {
                Logger.Debug($"Background icon generation failed for {path}: {ex}");
            } finally {
                _generationInProgress.TryRemove(key, out _);
            }
        });
    }

    private static void GenerateAndCacheIcon(string path, int size, DateTime lastWriteUtc) {
        var locker = _pathLocks.GetOrAdd(path, _ => new object());
        try {
            lock (locker) {
                if (_macIconCache.TryGetValue(path, out var cached) && cached.LastWriteUtc == lastWriteUtc && (DateTime.UtcNow - cached.CreatedAt) <= CACHE_TTL) {
                    Logger.Debug($"Background generation aborted: cache already populated for {path}");
                    return;
                }

                bool entered = false;
                try {
                    entered = _qlmanageSemaphore.Wait(5000);
                    if (!entered) {
                        Logger.Debug($"Background generation: timed out waiting for qlmanage semaphore for {path}");
                        return;
                    }

                    string tmpDir = Path.Combine(Path.GetTempPath(), "nav_icon_" + Guid.NewGuid().ToString("N"));
                    var sw = Stopwatch.StartNew();
                    Logger.Debug($"Background GetMacIconWithQlmanage START: path={path} size={size} tmpDir={tmpDir}");
                    try {
                        Directory.CreateDirectory(tmpDir);
                        var psi = new ProcessStartInfo("qlmanage", $"-t -s {size} -o \"{tmpDir}\" \"{path}\"") {
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        Logger.Debug($"Starting qlmanage (background): {psi.FileName} {psi.Arguments}");
                        using var p = Process.Start(psi);
                        if (p == null) {
                            Logger.Debug($"qlmanage failed to start for {path} (background)");
                            sw.Stop();
                            return;
                        }

                        p.WaitForExit(5000);
                        string stdout = "";
                        string stderr = "";
                        try {
                            stdout = p.StandardOutput.ReadToEnd();
                            stderr = p.StandardError.ReadToEnd();
                        } catch (Exception ex) {
                            Logger.Debug($"Error reading qlmanage streams (background): {ex}");
                        }
                        Logger.Debug($"qlmanage finished (background) HasExited={p.HasExited} ExitCode={(p.HasExited ? p.ExitCode.ToString() : "<not exited>")} stdoutLen={stdout.Length} stderrLen={stderr.Length}");

                        var png = Directory.EnumerateFiles(tmpDir, "*.png").OrderBy(f => f).FirstOrDefault();
                        if (png == null) {
                            sw.Stop();
                            Logger.Debug($"Background GetMacIconWithQlmanage: no png produced for {path} elapsed={sw.ElapsedMilliseconds}ms stdoutLen={stdout.Length} stderrLen={stderr.Length}");
                            return;
                        }

                        Logger.Debug($"Background GetMacIconWithQlmanage: png produced: {png} size={new FileInfo(png).Length}");
                        var bytes = File.ReadAllBytes(png);

                        try {
                            var newCached = new CachedIcon(bytes, lastWriteUtc);
                            _macIconCache[path] = newCached;
                            Logger.Debug($"Background GetMacIconWithQlmanage: cached icon for {path} size={newCached.Size}");
                            try { IconUpdated?.Invoke(path); } catch (Exception ex) { Logger.Debug($"IconUpdated invoke failed for {path}: {ex}"); }

                            if (_macIconCache.Count > MAX_CACHE_ENTRIES) {
                                try {
                                    var oldest = _macIconCache.OrderBy(kv => kv.Value.CreatedAt).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(oldest.Key)) {
                                        _macIconCache.TryRemove(oldest.Key, out _);
                                        Logger.Debug($"Evicted cached icon: {oldest.Key}");
                                    }
                                } catch (Exception ex) {
                                    Logger.Debug($"Error during cache eviction (background): {ex}");
                                }
                            }
                        } catch (Exception ex) {
                            Logger.Debug($"Failed to cache icon for {path} (background): {ex}");
                        }

                        try {
                            Directory.CreateDirectory(CACHE_DIR);
                            string key = ComputeKey(path, size);
                            string diskPng = Path.Combine(CACHE_DIR, key + ".png");
                            string diskMeta = Path.Combine(CACHE_DIR, key + ".meta");
                            File.WriteAllBytes(diskPng, bytes);
                            File.WriteAllText(diskMeta, lastWriteUtc.Ticks.ToString());
                            Logger.Debug($"Background GetMacIconWithQlmanage: wrote disk cache {diskPng}");
                        } catch (Exception ex) {
                            Logger.Debug($"Failed to write disk cache for {path} (background): {ex}");
                        }

                        sw.Stop();
                        Logger.Debug($"Background GetMacIconWithQlmanage FINISH: path={path} elapsed={sw.ElapsedMilliseconds}ms bytes={bytes.Length}");
                        return;
                    } catch (Exception ex) {
                        sw.Stop();
                        Logger.Debug($"Background GetMacIconWithQlmanage EXCEPTION: path={path} elapsed={sw.ElapsedMilliseconds}ms ex={ex}");
                        return;
                    } finally {
                        try { Directory.Delete(tmpDir, true); } catch (Exception ex) { Logger.Debug($"Failed to delete temp icon dir (background): {ex}"); }
                    }
                } finally {
                    if (entered) {
                        try { _qlmanageSemaphore.Release(); } catch (Exception ex) { Logger.Debug($"Failed to release qlmanage semaphore (background): {ex}"); }
                    }
                }
            }
        } finally {
            _pathLocks.TryRemove(path, out _);
        }
    }

    private static string ComputeKey(string path, int size) {
        using var sha = SHA256.Create();
        var input = System.Text.Encoding.UTF8.GetBytes(path + "|" + size);
        var hash = sha.ComputeHash(input);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHFILEINFO {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    private const uint SHGFI_ICON = 0x000000100;
    private const uint SHGFI_LARGEICON = 0x000000000;
    private const uint SHGFI_SMALLICON = 0x000000001;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);
}

