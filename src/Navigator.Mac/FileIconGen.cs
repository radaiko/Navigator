using System;
using System.Runtime.InteropServices;
using AppKit;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CoreGraphics;

namespace Navigator.Mac;

public static class FileIconGen {
    public static Bitmap GetFileIconBitmap(string filePath, int size = 256) {
        var nsImage = NSWorkspace.SharedWorkspace.IconForFile(filePath);

        // Resize if needed
        var resized = new NSImage(new CoreGraphics.CGSize(size, size));
        resized.LockFocus();
        nsImage.Draw(new CoreGraphics.CGRect(0, 0, size, size),
            new CoreGraphics.CGRect(0, 0, nsImage.Size.Width, nsImage.Size.Height),
            NSCompositingOperation.SourceOver, 1.0f, true, null);
        resized.UnlockFocus();

        // Convert NSImage to Bitmap
        return NsImageToAvaloniaBitmapFast(resized);
    }

    private static Bitmap NsImageToAvaloniaBitmapFast(NSImage nsImage) {
        CGRect rect = CGRect.Empty;
        using var cgImage = nsImage.AsCGImage(ref rect, null, null);

        int width = (int)cgImage.Width;
        int height = (int)cgImage.Height;

        int bytesPerPixel = 4;
        int bytesPerRow = bytesPerPixel * width;
        int bufferSize = height * bytesPerRow;

        // Allocate unmanaged memory
        IntPtr pixelBuffer = IntPtr.Zero;

        try {
            pixelBuffer = Marshal.AllocHGlobal(bufferSize);

            using var colorSpace = CGColorSpace.CreateDeviceRGB();
            using var context = new CGBitmapContext(
                pixelBuffer,
                width,
                height,
                8,
                bytesPerRow,
                colorSpace,
                CGImageAlphaInfo.PremultipliedLast
            );

            context.DrawImage(new CGRect(0, 0, width, height), cgImage);

            var bitmap = new Bitmap(
                PixelFormat.Rgba8888,
                AlphaFormat.Premul,
                pixelBuffer,
                new PixelSize(width, height),
                new Vector(96, 96),
                bytesPerRow
            );

            return bitmap;
        } finally {
            // Ensure we always free the unmanaged memory to avoid leaks if an exception occurs.
            if (pixelBuffer != IntPtr.Zero) {
                Marshal.FreeHGlobal(pixelBuffer);
            }
        }
    }
}
