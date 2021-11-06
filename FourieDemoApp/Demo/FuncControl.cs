using System;
using System.Collections.Generic;
using System.Drawing;

namespace Demo
{
    internal class FuncControl : MyControl
    {
        public delegate float FN(float t, float height);

        public FN Fn
        {
            get { return _fn; }
            set
            {
                if (_fn != value) _cache.Clear();
                _fn = value;
            }
        }

        private FN _fn;
        private float _t = 0;
        private List<float> _cache = new List<float>(2048);

        public FuncControl()
        {
            Fn = _calc;
        }

        public override bool UpdateFrame()
        {
            using (var g = Graphics.FromImage(_bmp.Bitmap))
            using (var fnPen = new Pen(Color.Chartreuse, 2))
            using (var markerPen = new Pen(Color.Red, 2))
            {
                g.FillRectangle(Brushes.Black, 0, 0, Width, Height);
                var zeroLevel = Height / 2;
                g.DrawLine(Pens.White, 0, zeroLevel, Width, zeroLevel);
                int i = 0;
                var xPrev = i;
                var yPrev = _callFn(i);

                for (; i < Width; i++)
                {
                    float y;
                    if (_cache.Count > i)
                    {
                        y = _cache[i];
                    }
                    else
                    {
                        y = _callFn(i);
                        _cache.Add(y);
                    }
                    g.DrawLine(fnPen, xPrev, yPrev, i, y);
                    xPrev = i;
                    yPrev = y;
                }

                for (var j = 0; j < Width; j += (Width / 20))
                {
                    g.DrawLine(Pens.White, j, Height / 2 - 5, j, Height / 2 + 5);
                    _bmp.SetPixel(j, Height / 2 + 5, Color.White.ToArgb());
                    var s = String.Format("{0:f02}", (float) j / Width);
                    g.DrawString(s, _font, Brushes.White, j - 5, Height / 2 + 8);
                }

                var yMarker = _callFn(_t);
                var xMarker = _t * Width;
                g.DrawLine(markerPen, xMarker, Height / 2f, xMarker, yMarker);
                g.FillEllipse(Brushes.Red, xMarker - 5, yMarker - 5, 10, 10);
            }

            Image = _bmp.Bitmap;
            var delta = 1f / Width;
            if (_t < 1f - delta)
            {
                _t += delta;
                return true;
            }

            return false;
        }

        protected override void _resetState()
        {
            _t = 0;
        }
        private float _callFn(int i)
        {
            return Height / 2f - Fn((float)i / Width, Height / 2f);
        }

        private float _callFn(float t)
        {
            return Height / 2f - Fn(t, Height / 2f);
        }

        private float _calc(float time, float maxAmpl)
        {
            const int freq = 10;
            const int phase = 0;
            const int amplitude = 1;
            const int yOffset = 0;
            var w = 2 * Math.PI * freq;
            var vertUnit = maxAmpl * 0.9f;
            var y = amplitude * vertUnit * Math.Cos(w * time + phase) + yOffset * vertUnit;
            return (float) y;
        }
    }
}