using System;
using System.Globalization;
using System.Windows.Data;

namespace DesktopFolder
{
    public class ColumnWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int iconsPerRow) || iconsPerRow <= 0)
                return 90;

            const double minWidth = 70;
            const double maxWidth = 120;

            double width = 600.0 / iconsPerRow;
            width = Math.Max(minWidth, Math.Min(maxWidth, width));

            if (parameter is string param)
            {
                switch (param)
                {
                    case "Icon":
                        return width * 0.7;
                    case "Text":
                        return width * 0.8;
                }
            }

            return width;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}