namespace Photo8
{
    public partial class PDFToSheet_PdfUI
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pdf_pnlTop;
        private System.Windows.Forms.Label pdf_lblServer;
        private System.Windows.Forms.TextBox pdf_txtServerUrl;
        private System.Windows.Forms.Label pdf_lblUser;
        private System.Windows.Forms.TextBox pdf_txtUsername;
        private System.Windows.Forms.Label pdf_lblPass;
        private System.Windows.Forms.TextBox pdf_txtPassword;
        private System.Windows.Forms.Button pdf_btnLogin;

        private System.Windows.Forms.Button pdf_btnChoosePdfs;
        private System.Windows.Forms.Button pdf_btnUploadRun;
        private System.Windows.Forms.Button pdf_btnClearLog;
        private System.Windows.Forms.Button pdf_btnCopyLog;

        private System.Windows.Forms.Label pdf_lblStatus;
        private System.Windows.Forms.ProgressBar pdf_progressBar1;

        private System.Windows.Forms.SplitContainer pdf_splitContainer1;
        private System.Windows.Forms.GroupBox pdf_grpFiles;
        private System.Windows.Forms.ListBox pdf_lstFiles;

        private System.Windows.Forms.GroupBox pdf_grpLog;
        private System.Windows.Forms.TextBox pdf_txtLog;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.pdf_pnlTop = new System.Windows.Forms.Panel();
            this.pdf_lblServer = new System.Windows.Forms.Label();
            this.pdf_txtServerUrl = new System.Windows.Forms.TextBox();
            this.pdf_lblUser = new System.Windows.Forms.Label();
            this.pdf_txtUsername = new System.Windows.Forms.TextBox();
            this.pdf_lblPass = new System.Windows.Forms.Label();
            this.pdf_txtPassword = new System.Windows.Forms.TextBox();
            this.pdf_btnLogin = new System.Windows.Forms.Button();

            this.pdf_btnChoosePdfs = new System.Windows.Forms.Button();
            this.pdf_btnUploadRun = new System.Windows.Forms.Button();
            this.pdf_btnClearLog = new System.Windows.Forms.Button();
            this.pdf_btnCopyLog = new System.Windows.Forms.Button();

            this.pdf_lblStatus = new System.Windows.Forms.Label();
            this.pdf_progressBar1 = new System.Windows.Forms.ProgressBar();

            this.pdf_splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pdf_grpFiles = new System.Windows.Forms.GroupBox();
            this.pdf_lstFiles = new System.Windows.Forms.ListBox();

            this.pdf_grpLog = new System.Windows.Forms.GroupBox();
            this.pdf_txtLog = new System.Windows.Forms.TextBox();

            this.pdf_pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pdf_splitContainer1)).BeginInit();
            this.pdf_splitContainer1.Panel1.SuspendLayout();
            this.pdf_splitContainer1.Panel2.SuspendLayout();
            this.pdf_splitContainer1.SuspendLayout();
            this.pdf_grpFiles.SuspendLayout();
            this.pdf_grpLog.SuspendLayout();
            this.SuspendLayout();

            // Form
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1080, 680);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PDFToSheet_PdfUI (Secure Upload)";
            this.Font = new System.Drawing.Font("Segoe UI", 9F);

            // pdf_pnlTop
            this.pdf_pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pdf_pnlTop.Height = 120;
            this.pdf_pnlTop.Padding = new System.Windows.Forms.Padding(12);

            // pdf_lblServer
            this.pdf_lblServer.AutoSize = true;
            this.pdf_lblServer.Location = new System.Drawing.Point(12, 10);
            this.pdf_lblServer.Text = "Server URL";

            // pdf_txtServerUrl
            this.pdf_txtServerUrl.Location = new System.Drawing.Point(12, 30);
            this.pdf_txtServerUrl.Size = new System.Drawing.Size(360, 23);
            this.pdf_txtServerUrl.Text = "http://127.0.0.1:8000";

            // pdf_lblUser
            this.pdf_lblUser.AutoSize = true;
            this.pdf_lblUser.Location = new System.Drawing.Point(390, 10);
            this.pdf_lblUser.Text = "Username";

            // pdf_txtUsername
            this.pdf_txtUsername.Location = new System.Drawing.Point(390, 30);
            this.pdf_txtUsername.Size = new System.Drawing.Size(150, 23);

            // pdf_lblPass
            this.pdf_lblPass.AutoSize = true;
            this.pdf_lblPass.Location = new System.Drawing.Point(555, 10);
            this.pdf_lblPass.Text = "Password";

            // pdf_txtPassword
            this.pdf_txtPassword.Location = new System.Drawing.Point(555, 30);
            this.pdf_txtPassword.Size = new System.Drawing.Size(150, 23);
            this.pdf_txtPassword.UseSystemPasswordChar = true;

            // pdf_btnLogin
            this.pdf_btnLogin.Location = new System.Drawing.Point(720, 27);
            this.pdf_btnLogin.Size = new System.Drawing.Size(90, 28);
            this.pdf_btnLogin.Text = "Login";
            this.pdf_btnLogin.UseVisualStyleBackColor = true;
            this.pdf_btnLogin.Click += new System.EventHandler(this.pdf_btnLogin_Click);

            // pdf_btnChoosePdfs
            this.pdf_btnChoosePdfs.Location = new System.Drawing.Point(12, 64);
            this.pdf_btnChoosePdfs.Size = new System.Drawing.Size(140, 30);
            this.pdf_btnChoosePdfs.Text = "Choose PDFs";
            this.pdf_btnChoosePdfs.UseVisualStyleBackColor = true;
            this.pdf_btnChoosePdfs.Click += new System.EventHandler(this.pdf_btnChoosePdfs_Click);

            // pdf_btnUploadRun
            this.pdf_btnUploadRun.Location = new System.Drawing.Point(160, 64);
            this.pdf_btnUploadRun.Size = new System.Drawing.Size(140, 30);
            this.pdf_btnUploadRun.Text = "Upload & Run";
            this.pdf_btnUploadRun.UseVisualStyleBackColor = true;
            this.pdf_btnUploadRun.Click += new System.EventHandler(this.pdf_btnUploadRun_Click);

            // pdf_btnClearLog
            this.pdf_btnClearLog.Location = new System.Drawing.Point(308, 64);
            this.pdf_btnClearLog.Size = new System.Drawing.Size(90, 30);
            this.pdf_btnClearLog.Text = "Clear Log";
            this.pdf_btnClearLog.UseVisualStyleBackColor = true;
            this.pdf_btnClearLog.Click += new System.EventHandler(this.pdf_btnClearLog_Click);

            // pdf_btnCopyLog
            this.pdf_btnCopyLog.Location = new System.Drawing.Point(406, 64);
            this.pdf_btnCopyLog.Size = new System.Drawing.Size(90, 30);
            this.pdf_btnCopyLog.Text = "Copy Log";
            this.pdf_btnCopyLog.UseVisualStyleBackColor = true;
            this.pdf_btnCopyLog.Click += new System.EventHandler(this.pdf_btnCopyLog_Click);

            // pdf_lblStatus
            this.pdf_lblStatus.Location = new System.Drawing.Point(510, 64);
            this.pdf_lblStatus.Size = new System.Drawing.Size(550, 30);
            this.pdf_lblStatus.Text = "Status: Not logged in";
            this.pdf_lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // pdf_progressBar1
            this.pdf_progressBar1.Location = new System.Drawing.Point(12, 104);
            this.pdf_progressBar1.Size = new System.Drawing.Size(1050, 8);
            this.pdf_progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.pdf_progressBar1.MarqueeAnimationSpeed = 30;
            this.pdf_progressBar1.Visible = false;

            // add controls to pdf_pnlTop
            this.pdf_pnlTop.Controls.Add(this.pdf_lblServer);
            this.pdf_pnlTop.Controls.Add(this.pdf_txtServerUrl);
            this.pdf_pnlTop.Controls.Add(this.pdf_lblUser);
            this.pdf_pnlTop.Controls.Add(this.pdf_txtUsername);
            this.pdf_pnlTop.Controls.Add(this.pdf_lblPass);
            this.pdf_pnlTop.Controls.Add(this.pdf_txtPassword);
            this.pdf_pnlTop.Controls.Add(this.pdf_btnLogin);

            this.pdf_pnlTop.Controls.Add(this.pdf_btnChoosePdfs);
            this.pdf_pnlTop.Controls.Add(this.pdf_btnUploadRun);
            this.pdf_pnlTop.Controls.Add(this.pdf_btnClearLog);
            this.pdf_pnlTop.Controls.Add(this.pdf_btnCopyLog);
            this.pdf_pnlTop.Controls.Add(this.pdf_lblStatus);
            this.pdf_pnlTop.Controls.Add(this.pdf_progressBar1);

            // pdf_splitContainer1
            this.pdf_splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pdf_splitContainer1.SplitterDistance = 420;
            this.pdf_splitContainer1.Orientation = System.Windows.Forms.Orientation.Vertical;

            // pdf_grpFiles
            this.pdf_grpFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pdf_grpFiles.Text = "Selected PDFs";
            this.pdf_grpFiles.Padding = new System.Windows.Forms.Padding(10);

            // pdf_lstFiles
            this.pdf_lstFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pdf_grpFiles.Controls.Add(this.pdf_lstFiles);

            // pdf_grpLog
            this.pdf_grpLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pdf_grpLog.Text = "Log";
            this.pdf_grpLog.Padding = new System.Windows.Forms.Padding(10);

            // pdf_txtLog
            this.pdf_txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pdf_txtLog.Multiline = true;
            this.pdf_txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.pdf_txtLog.WordWrap = false;

            this.pdf_grpLog.Controls.Add(this.pdf_txtLog);

            // split panels
            this.pdf_splitContainer1.Panel1.Controls.Add(this.pdf_grpFiles);
            this.pdf_splitContainer1.Panel2.Controls.Add(this.pdf_grpLog);

            // add to form
            this.Controls.Add(this.pdf_splitContainer1);
            this.Controls.Add(this.pdf_pnlTop);

            this.pdf_pnlTop.ResumeLayout(false);
            this.pdf_pnlTop.PerformLayout();

            this.pdf_splitContainer1.Panel1.ResumeLayout(false);
            this.pdf_splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pdf_splitContainer1)).EndInit();
            this.pdf_splitContainer1.ResumeLayout(false);

            this.pdf_grpFiles.ResumeLayout(false);
            this.pdf_grpLog.ResumeLayout(false);
            this.pdf_grpLog.PerformLayout();

            this.ResumeLayout(false);
        }
    }
}