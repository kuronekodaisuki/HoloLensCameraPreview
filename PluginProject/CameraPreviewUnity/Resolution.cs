using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraPreview
{
    public struct Resolution
    {
        public readonly int width;
        public readonly int height;

        public Resolution(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
    }
}
