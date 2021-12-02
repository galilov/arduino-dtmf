using System;
using System.Collections.Generic;
using System.Drawing;

namespace Demo
{
    internal class CircleFuncControl : MyControl
    { 
        public delegate void FNResponse(float freq, Tuple<float,float> complexAmplitude);

        public FN Fn
        {
            get => _fn;
            set
            {
                if (_fn != value) _cache.Clear();
                _fn = value;
            }
        }
        public FNResponse FnResponse;
        public float RotationFrequencyHz
        {
            get => _rotationFrequencyHz;
            set
            {
                if (Math.Abs(_rotationFrequencyHz - value) > float.Epsilon) _cache.Clear();
                _rotationFrequencyHz = value;
            }
        }
        public float FnMinArgSeconds { 
            get { return _fnMinArgSeconds; }
            set
            {
                if (Math.Abs(_fnMinArgSeconds - value) > float.Epsilon) _cache.Clear();
                _fnMinArgSeconds = value;
            }
        }
        public float FnMaxArgSeconds
        {
            get { return _nMaxArgSeconds; }
            set
            {
                if (Math.Abs(_nMaxArgSeconds - value) > float.Epsilon) _cache.Clear();
                _nMaxArgSeconds = value;
            }
        }
        public float DeltaArgSeconds
        {
            get => _deltaArgSeconds;
            set
            {
                if (Math.Abs(_deltaArgSeconds - value) > float.Epsilon) _cache.Clear();
                _deltaArgSeconds = value;
            }
        }

        public float Scale = 0;

        private float _t;
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
            Scale = Math.Min(Width, Height) / 2f;
            _t = FnMinArgSeconds;
            _w = 2 * Math.PI * RotationFrequencyHz;
            _cache.Clear();
        }

        public override bool UpdateFrame()
        {
            float xMassCenter = 0, yMassCenter = 0;
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
                    s = $"Период вращения: {1f / RotationFrequencyHz:f03} с / Частота: {RotationFrequencyHz:f01} Гц";
                }
                else
                {
                    s = $"Период вращения: ∞ / Частота: {RotationFrequencyHz:f01} Гц";
                }

                g.DrawString(s, _font, Brushes.White, 10, 10);
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
                        f = Fn(t);
                        coswt = (float)Math.Cos(wt);
                        sinwt = (float)Math.Sin(wt);
                        _cache.Add(new Tuple<float, float, float>(coswt, sinwt, f));
                    }
                    x = coswt * f;
                    y = sinwt * f;
                    sumXMass += x;
                    sumYMass += y;
                    var xpx = (float) (_zeroLevelX + x * Scale);
                    var ypx = (float) (_zeroLevelY - y * Scale);
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

                xMassCenter = (float)(sumXMass / n);
                yMassCenter = (float)(sumYMass / n);
                var xMassMarker = _zeroLevelX + xMassCenter* Scale;
                var yMassMarker = _zeroLevelY - yMassCenter* Scale;
                g.DrawLine(massCenterPen, _zeroLevelX, _zeroLevelY, (float) xMassMarker, (float) yMassMarker);
                g.FillEllipse(Brushes.Yellow, (float) (xMassMarker - 5), (float) (yMassMarker - 5), 10f, 10f);
                var a = $"{xMassCenter:f2}";
                var b = $"{(Math.Sign(yMassCenter) >= 0 ? "+" : "-")}{Math.Abs(yMassCenter):f2}";
                var sPosCaption = $"{a}{b}i";
                var szPosCaption = g.MeasureString(sPosCaption, _font);
                g.FillRectangle(Brushes.Gray, (float)(xMassMarker - 11), (float)(yMassMarker + 9), szPosCaption.Width+2, szPosCaption.Height+2);
                g.DrawString(sPosCaption, _font, Brushes.Yellow, (float)(xMassMarker - 10), (float)(yMassMarker + 10));
                var xMarker = _zeroLevelX + x * Scale;
                var yMarker = _zeroLevelY - y * Scale;

                var xRay = _zeroLevelX + Scale * coswt;
                var yRay = _zeroLevelY - Scale * sinwt;

                g.DrawLine(arrowPen, _zeroLevelX, _zeroLevelY, (float) xRay, (float) yRay);
                g.DrawLine(markerPen, _zeroLevelX, _zeroLevelY, (float) xMarker, (float) yMarker);
                g.FillEllipse(Brushes.Red, (float) (xMarker - 5), (float) (yMarker - 5), 10f, 10f);
            }

            Image = _bmp.Bitmap;
            FnResponse(RotationFrequencyHz, new Tuple<float, float>(xMassCenter , yMassCenter));
            if (_t < FnMaxArgSeconds - DeltaArgSeconds)
            {
                _t += DeltaArgSeconds;
                return true;
            }

            return false;
        }
    }
}