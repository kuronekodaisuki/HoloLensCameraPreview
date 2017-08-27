using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Capture.Frames;

namespace CameraPreview
{
    public class VideoCaptureSample
    {
        public int dataLength {
            get {
                return 4 * bitmap.PixelHeight * bitmap.PixelWidth;
            }
        }

        public CapturePixelFormat pixelFormat {
            get; private set;
        }

        internal SoftwareBitmap bitmap { get; private set; }

        MediaFrameReference frameReference;

        internal bool isBitmapCopied {
            get; private set;


        }

        internal VideoCaptureSample(MediaFrameReference frameReference)
        {
            if (frameReference == null)
            {
                throw new ArgumentNullException("frameReference.");
            }

            this.frameReference = frameReference;
            // this.worldOrigin = worldOrigin;

            bitmap = frameReference.VideoMediaFrame.SoftwareBitmap;
        }

        public void CopyRawImageDataIntoBuffer(byte[] byteBuffer)
        {
            //Here is a potential way to get direct access to the buffer:
            //http://stackoverflow.com/questions/25481840/how-to-change-mediacapture-to-byte

            if (byteBuffer == null)
            {
                throw new ArgumentNullException("byteBuffer");
            }

            if (byteBuffer.Length < dataLength)
            {
                throw new IndexOutOfRangeException("Your byteBuffer is not big enough." +
                    " Please use the VideoCaptureSample.dataLength property to allocate a large enough array.");
            }

            bitmap.CopyToBuffer(byteBuffer.AsBuffer());
            isBitmapCopied = true;
        }

        public void Dispose()
        {
            bitmap.Dispose();
            frameReference.Dispose();
        }

        static CapturePixelFormat ConvertBitmapPixelFormatToCapturePixelFormat(BitmapPixelFormat format)
        {
            switch (format)
            {
                case BitmapPixelFormat.Bgra8:
                    return CapturePixelFormat.BGRA32;
                case BitmapPixelFormat.Nv12:
                    return CapturePixelFormat.NV12;
                default:
                    return CapturePixelFormat.Unknown;
            }
        }
    }
}
