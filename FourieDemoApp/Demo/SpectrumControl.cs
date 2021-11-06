using System;
using System.Drawing;

namespace Demo
{
    internal class SpectrumControl : MyControl
    {
        public delegate float FN(float freq);

        public bool UsePower2 = false;
        public float Scale = 1f;
        public FN Fn;
        public float FreqStep = 0.1f;
        private const float MaxFreq = 20;
        
        public override bool UpdateFrame()
        {
            using (var g = Graphics.FromImage(_bmp.Bitmap))
            using (var fnPen = new Pen(Color.Yellow, 3))
            {
                g.FillRectangle(Brushes.Black, 0, 0, Width, Height);
                var step = (Width - 20) / MaxFreq;
                g.DrawLine(Pens.White, 10, Height - 32, Width - 10, Height - 32);
                for (float i = 0; i < MaxFreq; i += FreqStep)
                {
                    var y = Fn(i);
                    if (UsePower2) y *= y;
                    y *= Scale;
                    var x = 10 + step * i;
                    g.DrawLine(fnPen, x, Height - 32, x, (Height - 32) - y);
                    if (Math.Abs(i - Math.Round(i)) < 0.01f)
                    {
                        g.DrawLine(Pens.White, x, Height - 35, x, Height - 30);
                        var s = String.Format("{0:f01}", i);
                        g.DrawString(s, _font, Brushes.White, x - 5, Height - 27);
                    }

                }
            }

            Image = _bmp.Bitmap;
            return true;
        }
    }
}