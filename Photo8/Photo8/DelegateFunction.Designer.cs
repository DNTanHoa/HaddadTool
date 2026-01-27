namespace Photo8
{
    partial class DelegateFunction
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnOpenForm1;
        private System.Windows.Forms.Button btnOpenPdfToSheet;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnOpenForm1 = new System.Windows.Forms.Button();
            this.btnOpenPdfToSheet = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(420, 60);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Choose screen to open";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.btnOpenForm1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnOpenPdfToSheet, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 60);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(20);
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(420, 180);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // btnOpenForm1
            // 
            this.btnOpenForm1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpenForm1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnOpenForm1.Location = new System.Drawing.Point(23, 23);
            this.btnOpenForm1.Name = "btnOpenForm1";
            this.btnOpenForm1.Size = new System.Drawing.Size(374, 64);
            this.btnOpenForm1.TabIndex = 0;
            this.btnOpenForm1.Text = "Open Photo8";
            this.btnOpenForm1.UseVisualStyleBackColor = true;
            this.btnOpenForm1.Click += new System.EventHandler(this.btnOpenForm1_Click);
            // 
            // btnOpenPdfToSheet
            // 
            this.btnOpenPdfToSheet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpenPdfToSheet.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnOpenPdfToSheet.Location = new System.Drawing.Point(23, 93);
            this.btnOpenPdfToSheet.Name = "btnOpenPdfToSheet";
            this.btnOpenPdfToSheet.Size = new System.Drawing.Size(374, 64);
            this.btnOpenPdfToSheet.TabIndex = 1;
            this.btnOpenPdfToSheet.Text = "Open PDFToSheet";
            this.btnOpenPdfToSheet.UseVisualStyleBackColor = true;
            this.btnOpenPdfToSheet.Click += new System.EventHandler(this.btnOpenPdfToSheet_Click);
            // 
            // DelegateFunction
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 240);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "DelegateFunction";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Delegate / Navigator";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}