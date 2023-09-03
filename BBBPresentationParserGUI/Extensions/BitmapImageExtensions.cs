using System.IO;
using System.Windows.Media.Imaging;

namespace BBBPresentationParser.Extensions
{
    public static class BitmapImageUtils
    {
        public static BitmapImage BitmapImageFromBytes(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
    }
}
