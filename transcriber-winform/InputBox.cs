using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace transcriber_winform
{
    public partial class InputBox : Form
    {
        public long MillisecondResult { get; private set; }

        public InputBox()
        {
            InitializeComponent();

            this.MillisecondResult = -1;
            this.nudMinutes.Select(0, ("" + this.nudMinutes.Value).Length);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.MillisecondResult = (long)((this.nudMinutes.Value * 60 * 1000) + (this.nudSeconds.Value * 1000));
            if (this.MillisecondResult == 0)
            {
                this.MillisecondResult = -1;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.MillisecondResult = -1;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
