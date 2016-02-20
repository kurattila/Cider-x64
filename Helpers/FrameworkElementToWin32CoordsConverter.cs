using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Cider_x64.Helpers
{
    [ValueConversion(typeof(FrameworkElement), typeof(Rect))]
    public class FrameworkElementToWin32CoordsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var feRect = new Rect(0, 0, 0, 0);
            if (value is FrameworkElement)
            {
                var fe = value as FrameworkElement;
                feRect = GetFrameworkElementWin32PixelRect(fe);
            }
            return feRect;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Seam for retrieving a FrameworkElement's placement in Win32 screen coordinates
        /// </summary>
        /// <param name="fe"></param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public virtual Rect GetFrameworkElementWin32PixelRect(FrameworkElement fe)
        {
            var feLocation = fe.PointToScreen(new Point(0, 0));
            return new Rect(feLocation.X, feLocation.Y, fe.ActualWidth, fe.ActualHeight);
        }
    }
}
