using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraPreview
{
    public struct CameraParameters
    {
        public CapturePixelFormat pixelFormat;

        public int cameraResolutionHeight;

        public int cameraResolutionWidth;

        public int frameRate;

        public bool rotateImage180Degrees;

        public float hologramOpacity;
        public bool enableHolograms {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int videoStabilizationBufferSize;
        public bool enableVideoStabilization {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public CameraParameters(
            CapturePixelFormat pixelFormat = CapturePixelFormat.BGRA32,
            int cameraResolutionHeight = 720,
            int cameraResolutionWidth = 1280,
            int frameRate = 30,
            bool rotateImage180Degrees = true)
        { throw new NotImplementedException(); }
    }
}
