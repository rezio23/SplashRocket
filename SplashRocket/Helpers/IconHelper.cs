using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SplashRocket.Helpers
{
    public static class IconHelper
    {
        private static readonly Dictionary<string, Image> Cache = new();
        private static readonly Image FallbackIcon = SystemIcons.Application.ToBitmap();

        public static Image GetIconImage(string? appPath, string? customIconPath = null, int size = 48)
        {
            if (!string.IsNullOrWhiteSpace(customIconPath) && File.Exists(customIconPath))
            {
                try
                {
                    using var stream = File.OpenRead(customIconPath);
                    using var original = Image.FromStream(stream);
                    return Scale(original, size);
                }
                catch
                {
                    // fall through to app icon or fallback
                }
            }

            if (string.IsNullOrWhiteSpace(appPath))
                return Scale(FallbackIcon, size);

            var key = $"{appPath}:{customIconPath ?? "__none__"}:{size}";
            if (Cache.TryGetValue(key, out var cached))
                return cached;

            Image? icon = null;
            try
            {
                if (File.Exists(appPath))
                {
                    using var extracted = Icon.ExtractAssociatedIcon(appPath);
                    if (extracted != null)
                    {
                        var bitmap = extracted.ToBitmap();
                        icon = Scale(bitmap, size);
                    }
                }
            }
            catch
            {
                // ignore extraction errors
            }

            icon ??= Scale(FallbackIcon, size);
            Cache[key] = icon;
            return icon;
        }

        public static void ClearCache()
        {
            foreach (var image in Cache.Values)
            {
                image.Dispose();
            }
            Cache.Clear();
        }

        private static Image Scale(Image source, int size)
        {
            if (source.Width == size && source.Height == size)
                return new Bitmap(source);

            var scaled = new Bitmap(size, size);
            using (var g = Graphics.FromImage(scaled))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.DrawImage(source, 0, 0, size, size);
            }
            return scaled;
        }
    }
}
