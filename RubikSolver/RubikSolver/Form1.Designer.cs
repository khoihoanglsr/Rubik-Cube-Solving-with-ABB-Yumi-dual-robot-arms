namespace RubikSolver
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            comboBoxSelectedTask = new ComboBox();
            comboBoxControllers = new ComboBox();
            labelStatus = new Label();
            label1 = new Label();
            labelTask1 = new Label();
            labelTask2 = new Label();
            listModulefromTask = new ListBox();
            buttonstartRAPID = new Button();
            buttonstopRAPID = new Button();
            buttonToResetPPToMain = new Button();
            buttonPlay = new Button();
            rtbLog = new RichTextBox();
            openFileDialog1 = new OpenFileDialog();
            btnReset = new Button();
            wmpBackground = new AxWMPLib.AxWindowsMediaPlayer();
            ((System.ComponentModel.ISupportInitialize)wmpBackground).BeginInit();
            SuspendLayout();
            // 
            // comboBoxSelectedTask
            // 
            comboBoxSelectedTask.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 163);
            comboBoxSelectedTask.FormattingEnabled = true;
            comboBoxSelectedTask.Location = new Point(12, 123);
            comboBoxSelectedTask.Name = "comboBoxSelectedTask";
            comboBoxSelectedTask.Size = new Size(275, 26);
            comboBoxSelectedTask.TabIndex = 12;
            comboBoxSelectedTask.SelectedIndexChanged += comboBoxSelectedTask_SelectedIndexChanged;
            // 
            // comboBoxControllers
            // 
            comboBoxControllers.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 163);
            comboBoxControllers.FormattingEnabled = true;
            comboBoxControllers.Location = new Point(12, 41);
            comboBoxControllers.Name = "comboBoxControllers";
            comboBoxControllers.Size = new Size(275, 26);
            comboBoxControllers.TabIndex = 11;
            comboBoxControllers.SelectedIndexChanged += comboBoxControllers_SelectedIndexChanged;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.BackColor = Color.Black;
            labelStatus.Font = new Font("Consolas", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            labelStatus.ForeColor = Color.White;
            labelStatus.Location = new Point(12, 18);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(180, 20);
            labelStatus.TabIndex = 2;
            labelStatus.Text = "Select a Controller";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Black;
            label1.Font = new Font("Consolas", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            label1.ForeColor = Color.White;
            label1.Location = new Point(12, 100);
            label1.Name = "label1";
            label1.Size = new Size(117, 20);
            label1.TabIndex = 3;
            label1.Text = "Current Task";
            // 
            // labelTask1
            // 
            labelTask1.AutoSize = true;
            labelTask1.BackColor = Color.Black;
            labelTask1.Font = new Font("Consolas", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            labelTask1.ForeColor = Color.White;
            labelTask1.Location = new Point(674, 18);
            labelTask1.Name = "labelTask1";
            labelTask1.Size = new Size(81, 20);
            labelTask1.TabIndex = 4;
            labelTask1.Text = "Task 1: ";
            // 
            // labelTask2
            // 
            labelTask2.AutoSize = true;
            labelTask2.BackColor = Color.Black;
            labelTask2.Font = new Font("Consolas", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            labelTask2.ForeColor = Color.White;
            labelTask2.Location = new Point(674, 47);
            labelTask2.Name = "labelTask2";
            labelTask2.Size = new Size(81, 20);
            labelTask2.TabIndex = 5;
            labelTask2.Text = "Task 2: ";
            // 
            // listModulefromTask
            // 
            listModulefromTask.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 163);
            listModulefromTask.FormattingEnabled = true;
            listModulefromTask.ItemHeight = 18;
            listModulefromTask.Location = new Point(12, 193);
            listModulefromTask.Name = "listModulefromTask";
            listModulefromTask.Size = new Size(275, 220);
            listModulefromTask.TabIndex = 6;
            // 
            // buttonstartRAPID
            // 
            buttonstartRAPID.BackColor = Color.FromArgb(128, 255, 128);
            buttonstartRAPID.FlatStyle = FlatStyle.Flat;
            buttonstartRAPID.Font = new Font("Consolas", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            buttonstartRAPID.ForeColor = SystemColors.ActiveCaptionText;
            buttonstartRAPID.Location = new Point(12, 477);
            buttonstartRAPID.Name = "buttonstartRAPID";
            buttonstartRAPID.Size = new Size(131, 35);
            buttonstartRAPID.TabIndex = 7;
            buttonstartRAPID.Text = "Start RAPID";
            buttonstartRAPID.UseVisualStyleBackColor = false;
            buttonstartRAPID.Click += buttonstartRAPID_Click;
            // 
            // buttonstopRAPID
            // 
            buttonstopRAPID.BackColor = Color.Red;
            buttonstopRAPID.FlatStyle = FlatStyle.Flat;
            buttonstopRAPID.Font = new Font("Consolas", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            buttonstopRAPID.ForeColor = SystemColors.ActiveCaptionText;
            buttonstopRAPID.Location = new Point(156, 477);
            buttonstopRAPID.Name = "buttonstopRAPID";
            buttonstopRAPID.Size = new Size(131, 35);
            buttonstopRAPID.TabIndex = 8;
            buttonstopRAPID.Text = "Stop RAPID";
            buttonstopRAPID.UseVisualStyleBackColor = false;
            buttonstopRAPID.Click += buttonstopRAPID_Click;
            // 
            // buttonToResetPPToMain
            // 
            buttonToResetPPToMain.BackColor = Color.FromArgb(128, 255, 255);
            buttonToResetPPToMain.FlatStyle = FlatStyle.Flat;
            buttonToResetPPToMain.Font = new Font("Consolas", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            buttonToResetPPToMain.Location = new Point(12, 434);
            buttonToResetPPToMain.Name = "buttonToResetPPToMain";
            buttonToResetPPToMain.Size = new Size(275, 35);
            buttonToResetPPToMain.TabIndex = 9;
            buttonToResetPPToMain.Text = "PP to main";
            buttonToResetPPToMain.UseVisualStyleBackColor = false;
            buttonToResetPPToMain.Click += buttonToResetPPToMain_Click;
            // 
            // buttonPlay
            // 
            buttonPlay.BackColor = Color.Yellow;
            buttonPlay.FlatStyle = FlatStyle.Flat;
            buttonPlay.Font = new Font("Consolas", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            buttonPlay.Location = new Point(674, 82);
            buttonPlay.Name = "buttonPlay";
            buttonPlay.Size = new Size(131, 35);
            buttonPlay.TabIndex = 10;
            buttonPlay.Text = "Play";
            buttonPlay.UseVisualStyleBackColor = false;
            buttonPlay.Click += buttonPlay_Click;
            // 
            // rtbLog
            // 
            rtbLog.BackColor = SystemColors.ActiveCaptionText;
            rtbLog.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 163);
            rtbLog.ForeColor = Color.White;
            rtbLog.Location = new Point(674, 126);
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            rtbLog.Size = new Size(278, 398);
            rtbLog.TabIndex = 14;
            rtbLog.Text = "";
            rtbLog.TextChanged += rtbLog_TextChanged;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // btnReset
            // 
            btnReset.BackColor = Color.FromArgb(255, 128, 0);
            btnReset.FlatStyle = FlatStyle.Flat;
            btnReset.Font = new Font("Consolas", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnReset.Location = new Point(821, 82);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(131, 35);
            btnReset.TabIndex = 15;
            btnReset.Text = "Reset";
            btnReset.UseVisualStyleBackColor = false;
            btnReset.Click += btnReset_Click;
            // 
            // wmpBackground
            // 
            wmpBackground.Dock = DockStyle.Fill;
            wmpBackground.Enabled = true;
            wmpBackground.Location = new Point(0, 0);
            wmpBackground.Name = "wmpBackground";
            wmpBackground.OcxState = (AxHost.State)resources.GetObject("wmpBackground.OcxState");
            wmpBackground.Size = new Size(967, 528);
            wmpBackground.TabIndex = 16;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(967, 528);
            Controls.Add(btnReset);
            Controls.Add(rtbLog);
            Controls.Add(buttonPlay);
            Controls.Add(buttonToResetPPToMain);
            Controls.Add(buttonstopRAPID);
            Controls.Add(buttonstartRAPID);
            Controls.Add(listModulefromTask);
            Controls.Add(labelTask2);
            Controls.Add(labelTask1);
            Controls.Add(label1);
            Controls.Add(labelStatus);
            Controls.Add(comboBoxControllers);
            Controls.Add(comboBoxSelectedTask);
            Controls.Add(wmpBackground);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "YuMi Solving Rubik";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)wmpBackground).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox comboBoxSelectedTask;
        private ComboBox comboBoxControllers;
        private Label labelStatus;
        private Label label1;
        private Label labelTask1;
        private Label labelTask2;
        private ListBox listModulefromTask;
        private Button buttonstartRAPID;
        private Button buttonstopRAPID;
        private Button buttonToResetPPToMain;
        private Button buttonPlay;
        private RichTextBox rtbLog;
        private OpenFileDialog openFileDialog1;
        private Button btnReset;
        private AxWMPLib.AxWindowsMediaPlayer wmpBackground;
    }
}
