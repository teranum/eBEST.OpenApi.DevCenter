using System.Globalization;
using System.Windows.Data;

namespace eBEST.OpenApi.DevCenter.Converters
{
    public class Equal2BooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolval && boolval)
                return parameter;
            return Binding.DoNothing;
        }
    }
}
