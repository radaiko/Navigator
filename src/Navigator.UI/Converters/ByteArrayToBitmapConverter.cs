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

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
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

                        const int px = 24;
                        var tb = new TextBlock {
                            Text = text,
                            FontSize = 16,
                            Foreground = Brushes.Black,
                            TextAlignment = TextAlignment.Center,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                        };

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

