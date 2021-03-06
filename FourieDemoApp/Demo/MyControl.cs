using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Demo
{
    internal abstract class MyControl : PictureBox
    {
        public delegate float FN(float freq);
        protected DirectBitmap _bmp;
        private int _prevWidth = -1;
        private int _prevHeight = -1;
        protected readonly Font _font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold);

        public void Reset()
        {
            if (Width != _prevWidth || Height != _prevHeight)
            {
                _bmp = new DirectBitmap(Width, Height);
                _prevWidth = Width;
                _prevHeight = Height;
            }

            _resetState();
        }

        protected virtual void _resetState() { }
        public abstract bool UpdateFrame();
    }
}