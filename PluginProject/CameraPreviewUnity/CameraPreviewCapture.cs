using System;
using System.Threading.Tasks;

namespace CameraPreview
{
    public delegate void CaptureObjectCreatedCallback(CameraPreviewCapture createdObject);
    public delegate void FrameArrivedCallback(int frameLength);

    public sealed class CameraPreviewCapture
    {

#pragma warning disable 067
        public event FrameArrivedCallback OnFrameArrived;
#pragma warning restore 067

        public bool IsStreaming {
            get {
                throw new NotImplementedException();
            }
        }

        public async static Task CreateAync(CaptureObjectCreatedCallback onCreatedCallback)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> StartVideoModeAsync(bool IsCapturedHologram)
        {
            throw new NotImplementedException();
        }

        public void CopyFrameToBuffer(byte[] buffer)
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
    }
}
