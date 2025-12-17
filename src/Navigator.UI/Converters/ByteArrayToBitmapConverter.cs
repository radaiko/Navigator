using System;
using System.IO;
using System.Globalization;
using System.Text;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Media;
using System.Runtime.InteropServices;

namespace Navigator.UI.Converters {
    public class ByteArrayToBitmapConverter : IValueConverter {
        public static readonly ByteArrayToBitmapConverter Instance = new ByteArrayToBitmapConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture) {
            try {
                if (value is byte[] bytes && bytes.Length > 0) {
                    try {
                        var ms = new MemoryStream(bytes);
                        return new Bitmap(ms);
                    } catch {
                    }

                    try {
                        var text = Encoding.UTF8.GetString(bytes).Trim();
                        if (string.IsNullOrEmpty(text))
                            return null;

                        // Render a larger canvas for emoji so the resulting Bitmap is big enough when shown in the UI.
                        // This avoids the icon looking tiny; keep some left padding to prevent left-side clipping.
                        const int px = 128;
                        var tb = new TextBlock {
                            Text = text,
                            FontSize = 96,
                            Foreground = Brushes.Black,
                            TextAlignment = TextAlignment.Center,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                        };

                        // Shift right a little to account for emoji glyphs that draw left of the origin.
                        tb.Padding = new Thickness(12, 0, 0, 0);
                        tb.Margin = new Thickness(0);

                        try {
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                                tb.FontFamily = new FontFamily("Apple Color Emoji");
                            }
                        } catch {
                        }

                        tb.Measure(new Size(px, px));
                        tb.Arrange(new Rect(0, 0, px, px));

                        var rtb = new RenderTargetBitmap(new PixelSize(px, px), new Vector(96, 96));
                        rtb.Render(tb);
                        return rtb;
                    } catch {
                        return null;
                    }
                }

                return null;
            } catch {
                return null;
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
