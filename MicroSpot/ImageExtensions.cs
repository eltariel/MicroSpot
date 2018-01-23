using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace MicroSpot
{
    public static class ImageExtensions
    {
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);    // https://stackoverflow.com/a/1546121/194717

        /// <summary> 
        /// Converts a <see cref="System.Drawing.Bitmap"/> into a WPF <see cref="BitmapSource"/>. 
        /// </summary> 
        /// <remarks>Uses GDI to do the conversion. Hence the call to the marshalled DeleteObject. 
        /// </remarks> 
        /// <param name="source">The source bitmap.</param> 
        /// <returns>A BitmapSource</returns> 
        public static BitmapSource ToBitmapSource(this Bitmap source)
        {
            var hBitmap = source.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }
        }
    }
}