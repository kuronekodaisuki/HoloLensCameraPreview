using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraPreview
{
    public class VideoCaptureSample
    {
        public int dataLength { get; private set; }


        public CapturePixelFormat pixelFormat { get; private set; }

        public void CopyRawImageDataIntoBuffer(byte[] byteBuffer)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
