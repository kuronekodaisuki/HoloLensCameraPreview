using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraPreview
{
    public delegate void OnCameraPreviewCreatedCallback(CameraPreviewCapture captureObject);
    public delegate void OnFrameArrivedCallback();

    public class CameraPreviewCapture
    {
        public event OnFrameArrivedCallback FrameArrived;
        public bool IsStreamin => false;

        public static async Task CreateInstanceAsync(OnCameraPreviewCreatedCallback onCreatedCallback)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> StartCameraPreviewCapture(bool IsCapturedHologram)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> StopVideoModeAsync()
        {
            throw new NotImplementedException();
        }

        public async Task Dispose()
        {
            throw new NotImplementedException();
        }

        public void CopyFrameToBuffer(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}
