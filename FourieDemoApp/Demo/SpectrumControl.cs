using System;
using System.Drawing;

namespace Demo
{
    internal class SpectrumControl : MyControl
    {
        public bool UsePower2 = false;
        public float ScaleGraph = 1f;
        public FN Fn;
        public float MinFreq = 1f;
        public float FreqStep = 1f;
        public float MaxFreq = 20;

        public override bool UpdateFrame()
        {
            using (var g = Graphics.FromImage(_bmp.Bitmap))
            using (var fnPen = new Pen(Color.Yellow, 3))
            {
                g.FillRectangle(Brushes.Black, 0, 0, Width, Height);
                var step = (Width - 20) / MaxFreq;
                for (float i = 0; i < MaxFreq; i += 1)
                {
                    var x = 10 + step * i;
                    g.DrawLine(Pens.White, x, Height - 35, x, Height - 30);
                    var s = $"{i:f0}";
                    g.DrawString(s, _font, Brushes.White, x - 5, Height - 27);
                }

                g.DrawLine(Pens.White, 10, Height - 32, Width - 10, Height - 32);
                for (float i = MinFreq; i < MaxFreq; i += FreqStep)
                {
                    var y = Fn(i);
                    if (UsePower2) y *= y;
                    y *= ScaleGraph;
                    var x = 10 + step * i;
                    g.DrawLine(fnPen, x, Height - 32, x, (Height - 32) - y);
                }
            }

            Image = _bmp.Bitmap;
            return true;
        }
    }
}