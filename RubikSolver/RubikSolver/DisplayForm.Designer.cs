namespace RubikSolver
{
    partial class DisplayForm
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
            openGLControl1 = new SharpGL.OpenGLControl();
            pbUnfold = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)openGLControl1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbUnfold).BeginInit();
            SuspendLayout();
            // 
            // openGLControl1
            // 
            openGLControl1.DrawFPS = false;
            openGLControl1.Location = new Point(408, 12);
            openGLControl1.Margin = new Padding(4, 5, 4, 5);
            openGLControl1.Name = "openGLControl1";
            openGLControl1.OpenGLVersion = SharpGL.Version.OpenGLVersion.OpenGL2_1;
            openGLControl1.RenderContextType = SharpGL.RenderContextType.DIBSection;
            openGLControl1.RenderTrigger = SharpGL.RenderTrigger.TimerBased;
            openGLControl1.Size = new Size(379, 424);
            openGLControl1.TabIndex = 18;
            openGLControl1.OpenGLInitialized += openGLControl1_OpenGLInitialized;
            openGLControl1.OpenGLDraw += openGLControl1_OpenGLDraw;
            openGLControl1.Resize += openGLControl1_Resize;
            // 
            // pbUnfold
            // 
            pbUnfold.Location = new Point(12, 12);
            pbUnfold.Name = "pbUnfold";
            pbUnfold.Size = new Size(379, 426);
            pbUnfold.TabIndex = 17;
            pbUnfold.TabStop = false;
            // 
            // DisplayForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 454);
            Controls.Add(openGLControl1);
            Controls.Add(pbUnfold);
            Name = "DisplayForm";
            Text = "DisplayForm";
            Load += DisplayForm_Load;
            ((System.ComponentModel.ISupportInitialize)openGLControl1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbUnfold).EndInit();
            ResumeLayout(false);
        }

        #endregion

        public SharpGL.OpenGLControl openGLControl1;
        public PictureBox pbUnfold;
    }
}