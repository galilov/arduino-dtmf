using System;
using System.Drawing;

namespace Demo
{
    internal class SpectrumControl : MyControl
    {
        public delegate Tuple<float, float> FN(float freq);
        public bool UsePower2 = false;
        public float ScaleGraph = 1f;
        public FN Fn;
        public float MinFreq = 1f;
        public float FreqStep = 1f;
        public float MaxFreq = 20;
        public float Scale = 0;

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
                    var complexAmplitude = Fn(i);
                    var y = (float)(Scale*Math.Sqrt(complexAmplitude.Item1*complexAmplitude.Item1 + complexAmplitude.Item2 * complexAmplitude.Item2));
                    if (UsePower2) y *= y;
                    y *= ScaleGraph;
                    var x = 10 + step * i;
                    var xMassCenter = complexAmplitude.Item1;
                    var yMassCenter = complexAmplitude.Item2;
                    var xMassMarker = x;
                    var yMassMarker = (Height - 32) - y;
                    if (y > 0.01f)
                    {
                        var a = $"{xMassCenter:f2}";
                        var b = $"{(Math.Sign(yMassCenter) >= 0 ? "+" : "-")}{Math.Abs(yMassCenter):f2}";
                        var sPosCaption = $"{a}{b}i";
                        var szPosCaption = g.MeasureString(sPosCaption, _font);
                        g.FillRectangle(Brushes.Gray, (float) (xMassMarker - 11),
                            (float) (yMassMarker - szPosCaption.Height - 3), szPosCaption.Width + 2,
                            szPosCaption.Height + 2);
                        g.DrawString(sPosCaption, _font, Brushes.Yellow, (float) (xMassMarker - 10),
                            (float) (yMassMarker - szPosCaption.Height - 2));
                    }

                    g.DrawLine(fnPen, xMassMarker, yMassMarker + y, xMassMarker, yMassMarker);
                }
            }

            Image = _bmp.Bitmap;
            return true;
        }
    }
}