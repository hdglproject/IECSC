using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace WCS.ExcuteJob
{
    public class JobStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int input = System.Convert.ToInt32(value);
            switch (input)
            {
                case 0:
                    return Brushes.Gray;
                case 1:
                    return Brushes.LightBlue;
                case 2:
                    return Brushes.LightGreen;
                case 3:
                    return Brushes.Red;
                case 4:
                    return Brushes.Orange;
                default:
                    return Brushes.White;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
