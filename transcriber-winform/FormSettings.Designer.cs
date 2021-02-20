namespace transcriber_winform
{
    partial class FormSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.numRepeatLength = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numRepeatBack = new System.Windows.Forms.NumericUpDown();
            this.numRepeatPause = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numTypingPause = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numFrequencyRange = new System.Windows.Forms.NumericUpDown();
            this.numTempo = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.numLargeStep = new System.Windows.Forms.NumericUpDown();
            this.numSmallStep = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.numZoomSeconds = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numRepeatLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRepeatBack)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRepeatPause)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTypingPause)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFrequencyRange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTempo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLargeStep)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSmallStep)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numZoomSeconds)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Repeat Length";
            // 
            // numRepeatLength
            // 
            this.numRepeatLength.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numRepeatLength.DecimalPlaces = 1;
            this.numRepeatLength.Location = new System.Drawing.Point(143, 7);
            this.numRepeatLength.Name = "numRepeatLength";
            this.numRepeatLength.Size = new System.Drawing.Size(95, 20);
            this.numRepeatLength.TabIndex = 1;
            this.numRepeatLength.ValueChanged += new System.EventHandler(this.NumRepeatLength_OnValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Repeat Backstep";
            // 
            // numRepeatBack
            // 
            this.numRepeatBack.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numRepeatBack.DecimalPlaces = 1;
            this.numRepeatBack.Location = new System.Drawing.Point(143, 33);
            this.numRepeatBack.Name = "numRepeatBack";
            this.numRepeatBack.Size = new System.Drawing.Size(95, 20);
            this.numRepeatBack.TabIndex = 3;
            this.numRepeatBack.ValueChanged += new System.EventHandler(this.NumRepeatBack_OnValueChanged);
            // 
            // numRepeatPause
            // 
            this.numRepeatPause.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numRepeatPause.DecimalPlaces = 1;
            this.numRepeatPause.Location = new System.Drawing.Point(143, 59);
            this.numRepeatPause.Name = "numRepeatPause";
            this.numRepeatPause.Size = new System.Drawing.Size(95, 20);
            this.numRepeatPause.TabIndex = 4;
            this.numRepeatPause.ValueChanged += new System.EventHandler(this.NumRepeatPause_OnValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Repeat Pause";
            // 
            // numTypingPause
            // 
            this.numTypingPause.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numTypingPause.DecimalPlaces = 1;
            this.numTypingPause.Location = new System.Drawing.Point(143, 85);
            this.numTypingPause.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.numTypingPause.Name = "numTypingPause";
            this.numTypingPause.Size = new System.Drawing.Size(95, 20);
            this.numTypingPause.TabIndex = 7;
            this.numTypingPause.ValueChanged += new System.EventHandler(this.NumTypingPause_OnValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 87);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(108, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Typing Pause Length";
            // 
            // numFrequencyRange
            // 
            this.numFrequencyRange.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numFrequencyRange.Location = new System.Drawing.Point(143, 111);
            this.numFrequencyRange.Maximum = new decimal(new int[] {
            40000,
            0,
            0,
            0});
            this.numFrequencyRange.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numFrequencyRange.Name = "numFrequencyRange";
            this.numFrequencyRange.Size = new System.Drawing.Size(95, 20);
            this.numFrequencyRange.TabIndex = 9;
            this.numFrequencyRange.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numFrequencyRange.ValueChanged += new System.EventHandler(this.NumFrequencyRange_OnValueChanged);
            // 
            // numTempo
            // 
            this.numTempo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numTempo.Location = new System.Drawing.Point(143, 137);
            this.numTempo.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numTempo.Minimum = new decimal(new int[] {
            95,
            0,
            0,
            -2147483648});
            this.numTempo.Name = "numTempo";
            this.numTempo.Size = new System.Drawing.Size(95, 20);
            this.numTempo.TabIndex = 10;
            this.numTempo.ValueChanged += new System.EventHandler(this.NumTempo_OnValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 113);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(114, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Frequency Range (Hz)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 139);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Tempo (+%)";
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(163, 254);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 13;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_OnClick);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 191);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(59, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "Large Step";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 165);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(57, 13);
            this.label8.TabIndex = 16;
            this.label8.Text = "Small Step";
            // 
            // numLargeStep
            // 
            this.numLargeStep.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numLargeStep.DecimalPlaces = 1;
            this.numLargeStep.Location = new System.Drawing.Point(143, 189);
            this.numLargeStep.Name = "numLargeStep";
            this.numLargeStep.Size = new System.Drawing.Size(95, 20);
            this.numLargeStep.TabIndex = 15;
            this.numLargeStep.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numLargeStep.ValueChanged += new System.EventHandler(this.NumLargeStep_OnValueChanged);
            // 
            // numSmallStep
            // 
            this.numSmallStep.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numSmallStep.DecimalPlaces = 1;
            this.numSmallStep.Location = new System.Drawing.Point(143, 163);
            this.numSmallStep.Name = "numSmallStep";
            this.numSmallStep.Size = new System.Drawing.Size(95, 20);
            this.numSmallStep.TabIndex = 14;
            this.numSmallStep.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSmallStep.ValueChanged += new System.EventHandler(this.NumSmallStep_OnValueChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 217);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(129, 13);
            this.label9.TabIndex = 19;
            this.label9.Text = "Viewport/Zoom (seconds)";
            // 
            // numZoomSeconds
            // 
            this.numZoomSeconds.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numZoomSeconds.Location = new System.Drawing.Point(143, 215);
            this.numZoomSeconds.Name = "numZoomSeconds";
            this.numZoomSeconds.Size = new System.Drawing.Size(95, 20);
            this.numZoomSeconds.TabIndex = 18;
            this.numZoomSeconds.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numZoomSeconds.ValueChanged += new System.EventHandler(this.NumZoomSeconds_OnValueChanged);
            // 
            // FormSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(250, 289);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.numZoomSeconds);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.numLargeStep);
            this.Controls.Add(this.numSmallStep);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.numTempo);
            this.Controls.Add(this.numFrequencyRange);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numTypingPause);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numRepeatPause);
            this.Controls.Add(this.numRepeatBack);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numRepeatLength);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(260, 32);
            this.Name = "FormSettings";
            this.Text = "FormSettings";
            ((System.ComponentModel.ISupportInitialize)(this.numRepeatLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRepeatBack)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRepeatPause)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTypingPause)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFrequencyRange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTempo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLargeStep)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSmallStep)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numZoomSeconds)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numRepeatLength;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numRepeatBack;
        private System.Windows.Forms.NumericUpDown numRepeatPause;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numTypingPause;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numFrequencyRange;
        private System.Windows.Forms.NumericUpDown numTempo;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numLargeStep;
        private System.Windows.Forms.NumericUpDown numSmallStep;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown numZoomSeconds;
    }
}