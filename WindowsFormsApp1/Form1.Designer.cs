namespace WindowsFormsApp1
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.timer3 = new System.Windows.Forms.Timer(this.components);
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.timer4 = new System.Windows.Forms.Timer(this.components);
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.timerCHRun = new System.Windows.Forms.Timer(this.components);
            this.timerState = new System.Windows.Forms.Timer(this.components);
            this.timer5 = new System.Windows.Forms.Timer(this.components);
            this.timer6 = new System.Windows.Forms.Timer(this.components);
            this.button9 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 30000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 53);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(155, 71);
            this.button1.TabIndex = 0;
            this.button1.Text = "SCRrun";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 141);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(155, 65);
            this.button2.TabIndex = 1;
            this.button2.Text = "SCRstop";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "1",
            "2",
            "5",
            "6"});
            this.comboBox1.Location = new System.Drawing.Point(12, 12);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(155, 23);
            this.comboBox1.TabIndex = 2;
            // 
            // timer2
            // 
            this.timer2.Interval = 30000;
            this.timer2.Tick += new System.EventHandler(this.Timer2_Tick);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(12, 229);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(155, 57);
            this.button3.TabIndex = 3;
            this.button3.Text = "风门run";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.Button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(12, 310);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(155, 57);
            this.button4.TabIndex = 4;
            this.button4.Text = "风门stop";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.Button4_Click);
            // 
            // timer3
            // 
            this.timer3.Interval = 20000;
            this.timer3.Tick += new System.EventHandler(this.Timer3_Tick);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(12, 469);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(155, 57);
            this.button5.TabIndex = 6;
            this.button5.Text = "腐蚀stop";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.Button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(12, 388);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(155, 57);
            this.button6.TabIndex = 5;
            this.button6.Text = "腐蚀run";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.Button6_Click);
            // 
            // timer4
            // 
            this.timer4.Enabled = true;
            this.timer4.Interval = 60000;
            this.timer4.Tick += new System.EventHandler(this.timer4_Tick);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(211, 53);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(181, 71);
            this.button7.TabIndex = 7;
            this.button7.Text = "吹灰  run";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(211, 141);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(181, 65);
            this.button8.TabIndex = 8;
            this.button8.Text = "吹灰 stop";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // timerCHRun
            // 
            this.timerCHRun.Enabled = true;
            this.timerCHRun.Interval = 30000;
            this.timerCHRun.Tick += new System.EventHandler(this.timerCHRun_Tick);
            // 
            // timerState
            // 
            this.timerState.Enabled = true;
            this.timerState.Interval = 60000;
            this.timerState.Tick += new System.EventHandler(this.timerState_Tick);
            // 
            // timer5
            // 
            this.timer5.Enabled = true;
            this.timer5.Interval = 60000;
            this.timer5.Tick += new System.EventHandler(this.Timer5_Tick);
            // 
            // timer6
            // 
            this.timer6.Enabled = true;
            this.timer6.Interval = 10000;
            this.timer6.Tick += new System.EventHandler(this.timer6_Tick);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(211, 310);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(181, 58);
            this.button9.TabIndex = 9;
            this.button9.Text = "button9";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(413, 539);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "提奥斯_v 3.37";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Timer timer3;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Timer timer4;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Timer timerCHRun;
        private System.Windows.Forms.Timer timerState;
        private System.Windows.Forms.Timer timer5;
        private System.Windows.Forms.Timer timer6;
        private System.Windows.Forms.Button button9;
    }
}

