using System.Drawing;
using System.Windows.Forms;

namespace Demo
{
    internal abstract class MyControl : PictureBox
    {
        protected DirectBitmap _bmp;
        private int _prevWidth = -1;
        private int _prevHeight = -1;
        protected readonly Font _font = SystemFonts.CaptionFont;

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

        protected virtual void _resetState(){}
        public abstract bool UpdateFrame();
    }
}