using System;
using System.Collections.Generic;
using System.Drawing;

namespace Demo
{
    internal class CircleFuncControl : MyControl
    {
        public delegate float FN(float t, float height);

        public delegate void FNResponse(float freq, float module);

        public FN Fn
        {
            get { return _fn; }
            set
            {
                if (_fn != value) _cache.Clear();
                _fn = value;
            }
        }
        public FNResponse FnResponse;
        public float RotationFrequencyHz
        {
            get { return _rotationFrequencyHz; }
            set
            {
                if (_rotationFrequencyHz != value) _cache.Clear();
                _rotationFrequencyHz = value;
            }
        }
        public float FnMinArgSeconds { 
            get { return _fnMinArgSeconds; }
            set
            {
                if (_fnMinArgSeconds != value) _cache.Clear();
                _fnMinArgSeconds = value;
            }
        }
        public float FnMaxArgSeconds
        {
            get { return _nMaxArgSeconds; }
            set
            {
                if (_nMaxArgSeconds != value) _cache.Clear();
                _nMaxArgSeconds = value;
            }
        }
        public float DeltaArgSeconds
        {
            get { return _deltaArgSeconds; }
            set
            {
                if (_deltaArgSeconds != value) _cache.Clear();
                _deltaArgSeconds = value;
            }
        }
        private float _t, _maxAmpl;
        private double _w;
        private int _zeroLevelX, _zeroLevelY;
        private float _fnMinArgSeconds = 0;
        private float _nMaxArgSeconds = 1;
        private float _rotationFrequencyHz;
        private float _deltaArgSeconds = 0.1f;
        private FN _fn;
        private List<Tuple<float, float, float>> _cache = new List<Tuple<float, float, float>>(2048);

        protected override void _resetState()
        {
            _zeroLevelX = Width / 2;
            _zeroLevelY = Height / 2;
            _maxAmpl = Math.Min(Width, Height) / 2f;
            _t = FnMinArgSeconds;
            _w = 2 * Math.PI * RotationFrequencyHz;
        }

        public override bool UpdateFrame()
        {
            double xMassCenter = 0, yMassCenter = 0;
            using (var g = Graphics.FromImage(_bmp.Bitmap))
            using (var markerPen = new Pen(Color.Red, 2))
            using (var arrowPen = new Pen(Color.White, 2))
            using (var wirePen = new Pen(Color.Chartreuse, 2))
            using (var massCenterPen = new Pen(Color.Yellow, 2))
            {
                g.FillRectangle(Brushes.Black, 0, 0, Width, Height);
                g.DrawLine(Pens.White, 0, _zeroLevelY, Width, _zeroLevelY);
                g.DrawLine(Pens.White, _zeroLevelX, 0, _zeroLevelX, Height);
                string s;
                if (RotationFrequencyHz != 0)
                {
                    s = String.Format("Период: {0:f03} с / Частота: {1:f01} Гц",
                        1f / RotationFrequencyHz, RotationFrequencyHz);
                }
                else
                {
                    s = String.Format("Период: ∞ / Частота: {0:f01} Гц",
                        RotationFrequencyHz);
                }

                g.DrawString(s, _font, Brushes.White, 20, 20);
                double x = 0, y = 0;
                double sumXMass = 0, sumYMass = 0;
                var n = 1;
                float coswt = 0, sinwt = 0;
                var pxColor = Color.Chartreuse.ToArgb();
                float xpxPrev = float.NaN, ypxPrev = float.NaN;
                for (var t = FnMinArgSeconds; t <= _t; t += DeltaArgSeconds)
                {
                    float f;
                    if (_cache.Count >= n)
                    {
                        var tuple = _cache[n - 1];
                        coswt = tuple.Item1;
                        sinwt = tuple.Item2;
                        f = tuple.Item3;
                    }
                    else
                    {
                        var wt = _w * t;
                        f = Fn(t, _maxAmpl);
                        coswt = (float)Math.Cos(wt);
                        sinwt = (float)Math.Sin(wt);
                        _cache.Add(new Tuple<float, float, float>(coswt, sinwt, f));
                    }
                    x = coswt * f;
                    y = sinwt * f;
                    sumXMass += x;
                    sumYMass += y;
                    var xpx = (float) (_zeroLevelX + x);
                    var ypx = (float) (_zeroLevelY - y);
                    if (float.IsNaN(xpxPrev))
                    {
                        _bmp.SetPixel((int)xpx, (int)ypx, pxColor);
                    }
                    else
                    {
                        g.DrawLine(wirePen, xpxPrev, ypxPrev, xpx, ypx);
                    }
                    xpxPrev = xpx;
                    ypxPrev = ypx;
                    n++;
                }

                xMassCenter = sumXMass / n;
                yMassCenter = sumYMass / n;
                var xMassMarker = _zeroLevelX + xMassCenter;
                var yMassMarker = _zeroLevelY - yMassCenter;
                g.DrawLine(massCenterPen, _zeroLevelX, _zeroLevelY, (float) xMassMarker, (float) yMassMarker);
                g.FillEllipse(Brushes.Yellow, (float) (xMassMarker - 5), (float) (yMassMarker - 5), 10f, 10f);

                var xMarker = _zeroLevelX + x;
                var yMarker = _zeroLevelY - y;

                var xRay = _zeroLevelX + _maxAmpl * coswt;
                var yRay = _zeroLevelY - _maxAmpl * sinwt;

                g.DrawLine(arrowPen, _zeroLevelX, _zeroLevelY, (float) xRay, (float) yRay);
                g.DrawLine(markerPen, _zeroLevelX, _zeroLevelY, (float) xMarker, (float) yMarker);
                g.FillEllipse(Brushes.Red, (float) (xMarker - 5), (float) (yMarker - 5), 10f, 10f);
            }

            Image = _bmp.Bitmap;
            FnResponse?.Invoke(RotationFrequencyHz,
                (float) Math.Sqrt(xMassCenter * xMassCenter + yMassCenter * yMassCenter));
            if (_t < FnMaxArgSeconds - DeltaArgSeconds)
            {
                _t += DeltaArgSeconds;
                return true;
            }

            return false;
        }
    }
}