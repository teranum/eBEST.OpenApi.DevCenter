using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace eBEST.OpenApi.DevCenter.Converters
{
    internal class IconIdToImageConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return GetTreeIcon((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static List<BitmapSource>? _images;
        public static BitmapSource? GetTreeIcon(int nIndex)
        {
            if (_images is null)
            {
                _images = [];
                var m_Png_dead = new BitmapImage(new Uri("pack://application:,,,/eBEST.OpenApi.DevCenter;component/Resources/icons.png"));
                int nCX = 16;
                int nImgCount = m_Png_dead.PixelWidth / nCX;
                int nCY = m_Png_dead.PixelHeight;
                for (int i = 0; i < nImgCount; i++)
                {
                    _images.Add(new CroppedBitmap(m_Png_dead, new System.Windows.Int32Rect(i * nCX, 0, nCX, nCY)));
                }
            }
            if (nIndex >= 0 && nIndex < _images.Count)
                return _images[nIndex];
            return null;
        }
    }

}
