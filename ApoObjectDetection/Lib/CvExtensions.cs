using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;

namespace ApoObjectDetection.Lib
{
    public static class CvExtensions
    {
        public static Image<Gray, byte> Gray(this Image<Bgr, byte> img)
        {
            return img.Convert<Gray, byte>();
        }

        public static Image<Bgr, byte> Bgr(this Image<Gray, byte> img)
        {
            return img.Convert<Bgr, byte>();
        }

        public static ImageSource ToImageSource(this Image<Gray, byte> image)
        {
            return image.ToBitmap().ToImageSource();
        }

        public static ImageSource ToImageSource(this Image<Bgr, byte> image)
        {
            return image.ToBitmap().ToImageSource();
        }
    }

    public static class Extensions
    {
        //If you get 'dllimport unknown'-, then add 'using System.Runtime.InteropServices;'
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static ImageSource ToImageSource(this Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(handle);
            }
        }
    }
}