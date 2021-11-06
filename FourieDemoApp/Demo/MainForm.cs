using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows.Forms;

namespace Demo
{
    public partial class MainForm : Form
    {
        private FuncControl _funcControl;
        private CircleFuncControl _circleFuncControl;
        private SpectrumControl _spectrumControl;

        private readonly IDictionary<float, float> _spectrum =
            new Dictionary<float, float>(new FloatEqualityComparer());

        public MainForm()
        {
            InitializeComponent();
            AddCustomControls();
        }

        private void AddCustomControls()
        {
            SuspendLayout();
            _funcControl = new FuncControl();
            _funcControl.Fn = _calc;
            Controls.Add(_funcControl);

            _circleFuncControl = new CircleFuncControl();
            _circleFuncControl.Fn = _calc;
            _circleFuncControl.FnResponse = (freq, module) => { _spectrum[freq] = module; };
            Controls.Add(_circleFuncControl);

            _spectrumControl = new SpectrumControl();
            _spectrumControl.Fn = (freq) =>
            {
                if (_spectrum.TryGetValue(freq, out var massDistance))
                {
                    return massDistance;
                }

                return 0f;
            };
            Controls.Add(_spectrumControl);

            ResumeLayout();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            _resizeFuncControl();
        }

        private void _resizeFuncControl()
        {
            _funcControl.Left = 10;
            _funcControl.Top = 10;
            _funcControl.Width = Width - 40;
            _funcControl.Height = (int) (Height * 0.4);

            _circleFuncControl.Left = 10;
            _circleFuncControl.Width = Width / 2 - 40;
            _circleFuncControl.Top = (int) (Height * 0.4) + _funcControl.Top + 10;
            _circleFuncControl.Height = (int) (Height * 0.45);
            _circleFuncControl.DeltaArgSeconds = 1f / _funcControl.Width;

            _spectrumControl.Top = _circleFuncControl.Top;
            _spectrumControl.Left = _circleFuncControl.Right + 20;
            _spectrumControl.Width = Width - 60 - _circleFuncControl.Width;
            _spectrumControl.Height = _circleFuncControl.Height;

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

        private static float _calc(float t, float maxAmpl)
        {
            var amplitude = 0.5;
            var phase = 0;
            var yOffset = 0.5;
            var f1 = 10;
            var w1 = 2 * Math.PI * f1;
            var vertUnit = maxAmpl * 0.9f;
            var y1 = amplitude * vertUnit * Math.Cos(w1 * t + phase) + yOffset * vertUnit;
            var y = y1;
            return (float) y;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!_circleFuncControl.UpdateFrame())
            {
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
                _spectrumControl.FreqStep = (float) numStep.Value;
                _circleFuncControl.Reset();
                _funcControl.Reset();
                _spectrum.Clear();
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
            _spectrumControl.Scale = (float) numScale.Value;
            _spectrumControl.UpdateFrame();
        }
    }
}