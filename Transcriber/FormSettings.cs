using System;
using System.Windows.Forms;
using Eliason.AudioVisualizer;
using Eliason.TextEditor;

namespace Transcriber
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();
        }

        public ITextView TextView { get; set; }
        public AudioVisualizer AudioVisualizer { get; set; }
        public Form1 MainForm { get; set; }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            numRepeatLength.Value = (decimal) AudioVisualizer.RepeatLength;
            numRepeatBack.Value = (decimal) AudioVisualizer.RepeatBackwards;
            numRepeatPause.Value = (decimal) AudioVisualizer.RepeatPause;
            numTypingPause.Value = MainForm.TypingPauseLength / 1000m;
            numFrequencyRange.Value = AudioVisualizer.GetFrequencyRange();
            numTempo.Value = (decimal) AudioVisualizer.GetTempo();
            numSmallStep.Value = (decimal) AudioVisualizer.SmallStep;
            numLargeStep.Value = (decimal) AudioVisualizer.LargeStep;
            numZoomSeconds.Value = (decimal) AudioVisualizer.ZoomSeconds;
        }

        private void BtnClose_OnClick(object sender, EventArgs e)
        {
            NumRepeatLength_OnValueChanged(this, EventArgs.Empty);
            NumRepeatBack_OnValueChanged(this, EventArgs.Empty);
            NumRepeatPause_OnValueChanged(this, EventArgs.Empty);
            NumTypingPause_OnValueChanged(this, EventArgs.Empty);
            NumFrequencyRange_OnValueChanged(this, EventArgs.Empty);
            NumTempo_OnValueChanged(this, EventArgs.Empty);
            NumSmallStep_OnValueChanged(this, EventArgs.Empty);
            NumLargeStep_OnValueChanged(this, EventArgs.Empty);
            NumZoomSeconds_OnValueChanged(this, EventArgs.Empty);

            Close();
        }

        private void NumRepeatLength_OnValueChanged(object sender, EventArgs e)
        {
            AudioVisualizer.RepeatLength = (double) numRepeatLength.Value;
            AudioVisualizer.Invalidate();
        }

        private void NumRepeatBack_OnValueChanged(object sender, EventArgs e)
        {
            AudioVisualizer.RepeatBackwards = (double) numRepeatBack.Value;
            AudioVisualizer.Invalidate();
        }

        private void NumRepeatPause_OnValueChanged(object sender, EventArgs e)
        {
            AudioVisualizer.RepeatPause = (double) numRepeatPause.Value;
            AudioVisualizer.Invalidate();
        }

        private void NumTypingPause_OnValueChanged(object sender, EventArgs e)
        {
            MainForm.TypingPauseLength = (long) (numTypingPause.Value * 1000);
            AudioVisualizer.Invalidate();
        }

        private void NumFrequencyRange_OnValueChanged(object sender, EventArgs e)
        {
            AudioVisualizer.SetFrequencyRange((int) numFrequencyRange.Value);
            AudioVisualizer.Invalidate();
        }

        private void NumTempo_OnValueChanged(object sender, EventArgs e)
        {
            AudioVisualizer.SetTempo((float) numTempo.Value);
            AudioVisualizer.Invalidate();
        }

        private void NumSmallStep_OnValueChanged(object sender, EventArgs e)
        {
            AudioVisualizer.SmallStep = (double) numSmallStep.Value;
            AudioVisualizer.Invalidate();
        }

        private void NumLargeStep_OnValueChanged(object sender, EventArgs e)
        {
            AudioVisualizer.LargeStep = (double) numLargeStep.Value;
            AudioVisualizer.Invalidate();
        }

        private void NumZoomSeconds_OnValueChanged(object sender, EventArgs e)
        {
            AudioVisualizer.ZoomSeconds = (double) numZoomSeconds.Value;
            AudioVisualizer.Invalidate();
        }
    }
}