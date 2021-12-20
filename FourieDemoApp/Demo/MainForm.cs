using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows.Forms;

namespace Demo
{
    public partial class MainForm : Form
    {
        private readonly FuncControl _funcControl;
        private readonly CircleFuncControl _circleFuncControl;
        private readonly SpectrumControl _spectrumControl;

        private readonly IDictionary<float, Tuple<float, float>> _spectrum =
            new Dictionary<float, Tuple<float, float>>(new FloatEqualityComparer());

        public MainForm()
        {
            _funcControl = new FuncControl();
            _circleFuncControl = new CircleFuncControl();
            _spectrumControl = new SpectrumControl();
            _funcControl.DeltaArgSeconds = 0.016f;
            _funcControl.Fn = _calc;
            _circleFuncControl.Fn = _calc;
            _circleFuncControl.FnResponse = (freq, complexAmplitude) => { _spectrum[freq] = complexAmplitude; };
            _spectrumControl.Fn = (freq) =>
            {
                if (_spectrum.TryGetValue(freq, out var massDistance))
                {
                    return massDistance;
                }

                return new Tuple<float, float>(0, 0);
            };

            InitializeComponent();
            AddCustomControls();
        }

        private void AddCustomControls()
        {
            SuspendLayout();
            Controls.Add(_funcControl);
            Controls.Add(_circleFuncControl);
            Controls.Add(_spectrumControl);

            ResumeLayout();
            _resizeFuncControl();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            _resizeFuncControl();
        }

        private void _resizeFuncControl()
        {
            _funcControl.Left = 10;
            _funcControl.Top = 10;
            _funcControl.Width = ClientSize.Width - 20;
            _funcControl.Height = (int) (ClientSize.Height * 4 / 10);

            _circleFuncControl.Left = 10;
            _circleFuncControl.Width = ClientSize.Width / 2 - 15;
            _circleFuncControl.Top = _funcControl.Bottom + 10;
            _circleFuncControl.Height = Height / 2;
            _funcControl.DeltaArgSeconds = (float)(timer.Interval / 10000);
            _circleFuncControl.DeltaArgSeconds = _funcControl.DeltaArgSeconds;

            _spectrumControl.Top = _circleFuncControl.Top;
            _spectrumControl.Left = _circleFuncControl.Right + 10;
            _spectrumControl.Width = _circleFuncControl.Width;
            _spectrumControl.Height = _circleFuncControl.Height;

            _spectrumControl.Scale = _circleFuncControl.Scale;

            _funcControl.Reset();
            _funcControl.UpdateFrame();
            _circleFuncControl.Reset();
            _circleFuncControl.UpdateFrame();
            _spectrumControl.Reset();
            _spectrumControl.UpdateFrame();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            _resizeFuncControl();
        }

        private static float _calc(float t)
        {
            var amplitude = 0.4;
            var phase = Math.PI/4;
            var yOffset = 0.24;
            var f0 = 0.5;
            var f1 = 7;
            var f2 = 91;
            var f3 = 2;
            var w0 = 2 * Math.PI * f0;
            var w1 = 2 * Math.PI * f1;
            var w2 = 2 * Math.PI * f2;
            var w3 = 2 * Math.PI * f3;
            var y1 = amplitude *  Math.Cos(w1 * t + phase) + yOffset;
            var y2 = amplitude / 10 * Math.Sin(w2 * t + phase) + yOffset;
            //var y3 = amplitude / 5 * Math.Cos(w3 * t + phase);
            var y = y2 + y1;// +  y3; 
            return (float) y;
        }

        private static float _calc6(float t)
        {
            var amplitude = 0.5;
            var phase = 0;//Math.PI;
            var yOffset = 0;//0.5;
            var f0 = 7;
            var f1 = 3;
            var f2 = 11;
            var f3 = 20;
            var w0 = 2 * Math.PI * f0;
            var w1 = 2 * Math.PI * f1;
            var w2 = 2 * Math.PI * f2;
            var w3 = 2 * Math.PI * f3;
            var y0 = amplitude * Math.Sin(w0 * t + phase) + yOffset;
            var y1 = amplitude * 0.5 *  Math.Cos(w1 * t + phase) + yOffset;
            //var y2 = amplitude * vertUnit *0.7 * Math.Cos(w2 * t + phase);
            //var y3 = amplitude * vertUnit * 0.6 * Math.Cos(w3 * t + phase);
            var y =  y0 + y1 ;
            return (float)y;
        }

        private static float _calc4(float t)
        {
            var amplitude = 0.5;
            var phase = Math.PI;
            var yOffset = 0.3;
            var f0 = 7;
            var f1 = 10;
            var f2 = 11;
            var f3 = 20;
            var w0 = 2 * Math.PI * f0;
            var w1 = 2 * Math.PI * f1;
            var w2 = 2 * Math.PI * f2;
            var w3 = 2 * Math.PI * f3;
            //var y0 = amplitude * Math.Sin(w0 * t + phase) + yOffset;
            var y1 = amplitude * Math.Cos(w1 * t + phase) + yOffset;
            //var y2 = amplitude * vertUnit *0.7 * Math.Cos(w2 * t + phase);
            //var y3 = amplitude * vertUnit * 0.6 * Math.Cos(w3 * t + phase);
            var y =  y1;
            return (float)y;
        }

        private static float _calc3(float t)
        {
            var amplitude = 1;
            var phase = Math.PI/4;
            var yOffset = 0;
            var f1 = 1;
            var w1 = 2 * Math.PI * f1;
            //var vertUnit = 0.9f;
            var y1 = amplitude * Math.Cos(w1 * t + phase) + yOffset;
            var y = y1;
            return (float)y;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!_circleFuncControl.UpdateFrame())
            {
                if (checkBoxAutoStop.Checked)
                {
                    timer.Enabled = false;
                    checkBoxAutoStop.Checked = false;
                    _updateButtons();
                    return;
                }
                _circleFuncControl.RotationFrequencyHz += (float) numStep.Value;
                _circleFuncControl.Reset();
            }

            if (!_funcControl.UpdateFrame())
            {
                _funcControl.Reset();
            }

            _spectrumControl.UpdateFrame();

            if (_circleFuncControl.RotationFrequencyHz > (float) numEnd.Value)
            {
                timer.Stop();
                _updateButtons();
            }
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (!timer.Enabled)
            {
                _circleFuncControl.RotationFrequencyHz = (float) numBegin.Value;
                _spectrumControl.MinFreq = (float) numBegin.Value;
                _spectrumControl.FreqStep = (float) numStep.Value;
                _spectrumControl.MaxFreq = (float)numEnd.Value;
                _circleFuncControl.Reset();
                _funcControl.Reset();
                _spectrum.Clear();
                btnPauseResume.Enabled = true;
            }

            timer.Enabled = !timer.Enabled;
            _updateButtons();
        }

        private void btnPauseResume_Click(object sender, EventArgs e)
        {
            timer.Enabled = !timer.Enabled;
            _updateButtons();
        }

        private void _updateButtons()
        {
            if (timer.Enabled)
            {
                btnStartStop.Text = "Стоп";
                btnPauseResume.Text = "Пауза";
            }
            else
            {
                btnStartStop.Text = "Старт";
                btnPauseResume.Text = "Продолжить";
            }
        }

        private void checkBoxPower2_CheckedChanged(object sender, EventArgs e)
        {
            _spectrumControl.UsePower2 = checkBoxPower2.Checked;
            _spectrumControl.UpdateFrame();
        }

        private void numScale_ValueChanged(object sender, EventArgs e)
        {
            _spectrumControl.ScaleGraph = (float) numScale.Value;
            _spectrumControl.UpdateFrame();
        }
    }
}