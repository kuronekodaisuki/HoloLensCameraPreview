using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraPreview
{
    public struct Resolution
    {
        /// <summary>
        /// The width property.
        /// </summary>
        public readonly int width;

        /// <summary>
        /// The height property.
        /// </summary>
        public readonly int height;

        public Resolution(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
    }
}
