using System;
using System.Windows.Forms;
using Eliason.AudioVisualizer;
using Eliason.TextEditor;

namespace transcriber_winform
{
    public partial class FormSettings : Form
    {
        public ITextView TextView { get; set; }
        public AudioVisualizer AudioVisualizer { get; set; }
        public Form1 MainForm { get; set; }

        public FormSettings()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            numRepeatLength.Value = (decimal) this.AudioVisualizer.RepeatLength;
            numRepeatBack.Value = (decimal)this.AudioVisualizer.RepeatBackwards;
            numRepeatPause.Value = (decimal)this.AudioVisualizer.RepeatPause;
            numTypingPause.Value = (this.MainForm.TypingPauseLength / 1000m);
            numFrequencyRange.Value = this.AudioVisualizer.GetFrequencyRange();
            numTempo.Value = (decimal) this.AudioVisualizer.GetTempo();
            numSmallStep.Value = (decimal)this.AudioVisualizer.SmallStep;
            numLargeStep.Value = (decimal)this.AudioVisualizer.LargeStep;
            numZoomSeconds.Value = (decimal)this.AudioVisualizer.ZoomSeconds;
        }

        private void BtnClose_OnClick(object sender, EventArgs e)
        {
            this.NumRepeatLength_OnValueChanged(this, EventArgs.Empty);
            this.NumRepeatBack_OnValueChanged(this, EventArgs.Empty);
            this.NumRepeatPause_OnValueChanged(this, EventArgs.Empty);
            this.NumTypingPause_OnValueChanged(this, EventArgs.Empty);
            this.NumFrequencyRange_OnValueChanged(this, EventArgs.Empty);
            this.NumTempo_OnValueChanged(this, EventArgs.Empty);
            this.NumSmallStep_OnValueChanged(this, EventArgs.Empty);
            this.NumLargeStep_OnValueChanged(this, EventArgs.Empty);
            this.NumZoomSeconds_OnValueChanged(this, EventArgs.Empty);

            this.Close();
        }

        private void NumRepeatLength_OnValueChanged(object sender, EventArgs e)
        {
            this.AudioVisualizer.RepeatLength = (double) this.numRepeatLength.Value;
            this.AudioVisualizer.Invalidate();
        }

        private void NumRepeatBack_OnValueChanged(object sender, EventArgs e)
        {
            this.AudioVisualizer.RepeatBackwards = (double)this.numRepeatBack.Value;
            this.AudioVisualizer.Invalidate();
        }

        private void NumRepeatPause_OnValueChanged(object sender, EventArgs e)
        {
            this.AudioVisualizer.RepeatPause = (double)this.numRepeatPause.Value;
            this.AudioVisualizer.Invalidate();
        }

        private void NumTypingPause_OnValueChanged(object sender, EventArgs e)
        {
            this.MainForm.TypingPauseLength = (long)(this.numTypingPause.Value * 1000);
            this.AudioVisualizer.Invalidate();
        }

        private void NumFrequencyRange_OnValueChanged(object sender, EventArgs e)
        {
            this.AudioVisualizer.SetFrequencyRange((int)this.numFrequencyRange.Value);
            this.AudioVisualizer.Invalidate();
        }

        private void NumTempo_OnValueChanged(object sender, EventArgs e)
        {
            this.AudioVisualizer.SetTempo((float) this.numTempo.Value);
            this.AudioVisualizer.Invalidate();
        }

        private void NumSmallStep_OnValueChanged(object sender, EventArgs e)
        {
            this.AudioVisualizer.SmallStep = ((double)this.numSmallStep.Value);
            this.AudioVisualizer.Invalidate();
        }

        private void NumLargeStep_OnValueChanged(object sender, EventArgs e)
        {
            this.AudioVisualizer.LargeStep = ((double)this.numLargeStep.Value);
            this.AudioVisualizer.Invalidate();
        }

        private void NumZoomSeconds_OnValueChanged(object sender, EventArgs e)
        {
            this.AudioVisualizer.ZoomSeconds = ((double)this.numZoomSeconds.Value);
            this.AudioVisualizer.Invalidate();
        }
    }
}
