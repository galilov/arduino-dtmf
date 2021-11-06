namespace Demo
{
    partial class MainForm
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
            this.timer = new System.Timers.Timer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.numScale = new System.Windows.Forms.NumericUpDown();
            this.checkBoxPower2 = new System.Windows.Forms.CheckBox();
            this.btnPauseResume = new System.Windows.Forms.Button();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.numStep = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numEnd = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numBegin = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.timer)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStep)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEnd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBegin)).BeginInit();
            this.SuspendLayout();
            // 
            // timer
            // 
            this.timer.Interval = 6D;
            this.timer.SynchronizingObject = this;
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_Elapsed);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.numScale);
            this.panel1.Controls.Add(this.checkBoxPower2);
            this.panel1.Controls.Add(this.btnPauseResume);
            this.panel1.Controls.Add(this.btnStartStop);
            this.panel1.Controls.Add(this.numStep);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.numEnd);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.numBegin);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(12, 1034);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1894, 44);
            this.panel1.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(1573, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(128, 38);
            this.label4.TabIndex = 10;
            this.label4.Text = "Масштаб:";
            // 
            // numScale
            // 
            this.numScale.DecimalPlaces = 3;
            this.numScale.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numScale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numScale.Location = new System.Drawing.Point(1707, 3);
            this.numScale.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numScale.Name = "numScale";
            this.numScale.Size = new System.Drawing.Size(120, 35);
            this.numScale.TabIndex = 9;
            this.numScale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numScale.ValueChanged += new System.EventHandler(this.numScale_ValueChanged);
            // 
            // checkBoxPower2
            // 
            this.checkBoxPower2.Cursor = System.Windows.Forms.Cursors.Default;
            this.checkBoxPower2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.checkBoxPower2.Location = new System.Drawing.Point(1146, 5);
            this.checkBoxPower2.Name = "checkBoxPower2";
            this.checkBoxPower2.Size = new System.Drawing.Size(406, 31);
            this.checkBoxPower2.TabIndex = 8;
            this.checkBoxPower2.Text = "Показать значения в квадрате";
            this.checkBoxPower2.UseVisualStyleBackColor = true;
            this.checkBoxPower2.CheckedChanged += new System.EventHandler(this.checkBoxPower2_CheckedChanged);
            // 
            // btnPauseResume
            // 
            this.btnPauseResume.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnPauseResume.Location = new System.Drawing.Point(869, 1);
            this.btnPauseResume.Name = "btnPauseResume";
            this.btnPauseResume.Size = new System.Drawing.Size(122, 37);
            this.btnPauseResume.TabIndex = 7;
            this.btnPauseResume.Text = "Пауза";
            this.btnPauseResume.UseVisualStyleBackColor = true;
            this.btnPauseResume.Click += new System.EventHandler(this.btnPauseResume_Click);
            // 
            // btnStartStop
            // 
            this.btnStartStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnStartStop.Location = new System.Drawing.Point(730, 1);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(122, 37);
            this.btnStartStop.TabIndex = 6;
            this.btnStartStop.Text = "Старт";
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // numStep
            // 
            this.numStep.DecimalPlaces = 1;
            this.numStep.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numStep.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numStep.Location = new System.Drawing.Point(606, 1);
            this.numStep.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numStep.Name = "numStep";
            this.numStep.Size = new System.Drawing.Size(94, 35);
            this.numStep.TabIndex = 5;
            this.numStep.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(484, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 29);
            this.label3.TabIndex = 4;
            this.label3.Text = "Шаг f,Гц";
            // 
            // numEnd
            // 
            this.numEnd.DecimalPlaces = 1;
            this.numEnd.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numEnd.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numEnd.Location = new System.Drawing.Point(368, 3);
            this.numEnd.Name = "numEnd";
            this.numEnd.Size = new System.Drawing.Size(94, 35);
            this.numEnd.TabIndex = 3;
            this.numEnd.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(241, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(121, 35);
            this.label2.TabIndex = 2;
            this.label2.Text = "Кон. f, Гц";
            // 
            // numBegin
            // 
            this.numBegin.DecimalPlaces = 1;
            this.numBegin.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numBegin.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numBegin.Location = new System.Drawing.Point(128, 3);
            this.numBegin.Name = "numBegin";
            this.numBegin.Size = new System.Drawing.Size(94, 35);
            this.numBegin.TabIndex = 1;
            this.numBegin.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(3, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 38);
            this.label1.TabIndex = 0;
            this.label1.Text = "Нач. f, Гц";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1918, 1090);
            this.Controls.Add(this.panel1);
            this.Name = "MainForm";
            this.Text = "Fourie transform demo by AGalilov";
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.timer)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStep)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEnd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBegin)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.NumericUpDown numScale;

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBoxPower2;
        private System.Windows.Forms.NumericUpDown numericUpDown1;

        private System.Windows.Forms.Button btnPauseResume;

        private System.Windows.Forms.Button btnStartStop;

        private System.Windows.Forms.NumericUpDown numStart;
        private System.Windows.Forms.NumericUpDown numStop;

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numBegin;
        private System.Windows.Forms.NumericUpDown numEnd;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numStep;
        private System.Windows.Forms.Label label3;

        private System.Windows.Forms.Panel panel1;

        private System.Timers.Timer timer;

        #endregion
    }
}