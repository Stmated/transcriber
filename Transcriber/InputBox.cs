using System;
using System.Windows.Forms;

namespace Transcriber
{
    public partial class InputBox : Form
    {
        public InputBox()
        {
            InitializeComponent();

            MillisecondResult = -1;
            nudMinutes.Select(0, ("" + nudMinutes.Value).Length);
        }

        public long MillisecondResult { get; private set; }

        private void btnOK_Click(object sender, EventArgs e)
        {
            MillisecondResult = (long) (nudMinutes.Value * 60 * 1000 + nudSeconds.Value * 1000);
            if (MillisecondResult == 0) MillisecondResult = -1;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            MillisecondResult = -1;
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}