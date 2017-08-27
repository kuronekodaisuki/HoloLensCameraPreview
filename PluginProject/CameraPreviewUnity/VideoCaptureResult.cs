using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraPreview
{
    public struct VideoCaptureResult
    {
        public readonly long hResult;

        public readonly ResultType resultType;

        public readonly bool success;
    }
}
