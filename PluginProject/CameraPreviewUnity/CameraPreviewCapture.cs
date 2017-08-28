using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraPreview
{
    public delegate void OnVideoCaptureResourceCreatedCallback(CameraPreviewCapture captureObject);
    public delegate void OnVideoModeStartedCallback(VideoCaptureResult result);
    public delegate void FrameSampleAcquiredCallback(VideoCaptureSample videoCaptureSample);
    public delegate void OnVideoModeStoppedCallback(VideoCaptureResult result);

    public sealed class CameraPreviewCapture
    {

#pragma warning disable 067
        public event FrameSampleAcquiredCallback FrameSampleAcquired;
#pragma warning restore 067

        public bool IsStreaming {
            get {
                throw new NotImplementedException();
            }
        }


        public static void CreateAync(OnVideoCaptureResourceCreatedCallback onCreatedCallback)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Resolution> GetSupportedResolutions()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<float> GetSupportedFrameRatesForResolution(Resolution resolution)
        {
            throw new NotImplementedException();
        }

        public void StartVideoModeAsync(bool IsCapturedHologram)
        {
            throw new NotImplementedException();
        }

        public void StopVideoModeAsync(OnVideoModeStoppedCallback onVideoModeStoppedCallback)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
