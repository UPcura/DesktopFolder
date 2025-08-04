using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace DesktopFolder;

[SupportedOSPlatform("windows")]
public static class IconExtractor
{
    private static ImageSource? _defaultIcon;

    public static ImageSource? GetIcon(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return GetDefaultIcon();

        try
        {
            using var icon = Icon.ExtractAssociatedIcon(filePath);
            return icon != null
                ? ConvertIconToImageSource(icon)
                : GetDefaultIcon();
        }
        catch
        {
            return GetDefaultIcon();
        }
    }

    [SupportedOSPlatform("windows")]
    private static ImageSource ConvertIconToImageSource(Icon icon)
    {
        return Imaging.CreateBitmapSourceFromHIcon(
            icon.Handle,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
    }

    [SupportedOSPlatform("windows")]
    private static ImageSource GetDefaultIcon()
    {
        if (_defaultIcon != null) return _defaultIcon;

        try
        {
            _defaultIcon = ConvertIconToImageSource(SystemIcons.WinLogo);
        }
        catch
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,,/folder.ico");
                bitmap.EndInit();
                _defaultIcon = bitmap;
            }
            catch
            {
                _defaultIcon = new DrawingImage();
            }
        }
        return _defaultIcon;
    }
}