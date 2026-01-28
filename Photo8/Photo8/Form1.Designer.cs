using DevExpress.Utils.Layout;
namespace Photo8
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

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnLoadTasks = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.rtbLabel = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.label2 = new System.Windows.Forms.Label();
            this.txtContractor = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.Input_link = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtContractNo = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtSeason = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtType = new System.Windows.Forms.TextBox();
            this.rtbPO = new System.Windows.Forms.RichTextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tablePanel1 = new DevExpress.Utils.Layout.TablePanel();
            this.button3 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tablePanel1)).BeginInit();
            this.tablePanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnLogin
            // 
            this.btnLogin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnLogin.Location = new System.Drawing.Point(13, 12);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(145, 29);
            this.btnLogin.TabIndex = 0;
            this.btnLogin.Text = "🔐 Mở trình duyệt để Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // btnLoadTasks
            // 
            this.tablePanel1.SetColumn(this.btnLoadTasks, 2);
            this.btnLoadTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLoadTasks.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnLoadTasks.Location = new System.Drawing.Point(162, 12);
            this.btnLoadTasks.Name = "btnLoadTasks";
            this.tablePanel1.SetRow(this.btnLoadTasks, 0);
            this.btnLoadTasks.Size = new System.Drawing.Size(150, 29);
            this.btnLoadTasks.TabIndex = 1;
            this.btnLoadTasks.Text = "⚙️ Load Draft";
            this.btnLoadTasks.UseVisualStyleBackColor = true;
            this.btnLoadTasks.Click += new System.EventHandler(this.btnLoadTasks_Click);
            // 
            // lblStatus
            // 
            this.tablePanel1.SetColumn(this.lblStatus, 0);
            this.tablePanel1.SetColumnSpan(this.lblStatus, 5);
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblStatus.Location = new System.Drawing.Point(14, 320);
            this.lblStatus.Name = "lblStatus";
            this.tablePanel1.SetRow(this.lblStatus, 9);
            this.lblStatus.Size = new System.Drawing.Size(587, 23);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "Status: Waiting...";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // rtbLabel
            // 
            this.tablePanel1.SetColumn(this.rtbLabel, 1);
            this.tablePanel1.SetColumnSpan(this.rtbLabel, 3);
            this.rtbLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbLabel.Location = new System.Drawing.Point(121, 214);
            this.rtbLabel.Name = "rtbLabel";
            this.tablePanel1.SetRow(this.rtbLabel, 7);
            this.rtbLabel.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.rtbLabel.Size = new System.Drawing.Size(353, 27);
            this.rtbLabel.TabIndex = 5;
            this.rtbLabel.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.tablePanel1.SetColumn(this.label1, 0);
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(14, 219);
            this.label1.Name = "label1";
            this.tablePanel1.SetRow(this.label1, 7);
            this.label1.Size = new System.Drawing.Size(79, 17);
            this.label1.TabIndex = 6;
            this.label1.Text = "List Label";
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(13, 345);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(589, 155);
            this.dataGridView1.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.tablePanel1.SetColumn(this.label2, 0);
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(14, 95);
            this.label2.Name = "label2";
            this.tablePanel1.SetRow(this.label2, 3);
            this.label2.Size = new System.Drawing.Size(49, 17);
            this.label2.TabIndex = 8;
            this.label2.Text = "Type ";
            // 
            // txtContractor
            // 
            this.tablePanel1.SetColumn(this.txtContractor, 1);
            this.tablePanel1.SetColumnSpan(this.txtContractor, 3);
            this.txtContractor.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtContractor.Location = new System.Drawing.Point(121, 124);
            this.txtContractor.Name = "txtContractor";
            this.tablePanel1.SetRow(this.txtContractor, 4);
            this.txtContractor.Size = new System.Drawing.Size(353, 23);
            this.txtContractor.TabIndex = 9;
            this.txtContractor.TextChanged += new System.EventHandler(this.txtContractor_TextChanged);
            // 
            // button1
            // 
            this.tablePanel1.SetColumn(this.button1, 3);
            this.button1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.button1.Location = new System.Drawing.Point(315, 12);
            this.button1.Name = "button1";
            this.tablePanel1.SetRow(this.button1, 0);
            this.button1.Size = new System.Drawing.Size(159, 29);
            this.button1.TabIndex = 10;
            this.button1.Text = "⚙️ Upload Image";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.SaveBtn);
            // 
            // Input_link
            // 
            this.tablePanel1.SetColumn(this.Input_link, 1);
            this.tablePanel1.SetColumnSpan(this.Input_link, 3);
            this.Input_link.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Input_link.Location = new System.Drawing.Point(121, 59);
            this.Input_link.Name = "Input_link";
            this.tablePanel1.SetRow(this.Input_link, 2);
            this.Input_link.Size = new System.Drawing.Size(353, 24);
            this.Input_link.TabIndex = 12;
            this.Input_link.TextChanged += new System.EventHandler(this.Input_link_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.tablePanel1.SetColumn(this.label3, 0);
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(14, 62);
            this.label3.Name = "label3";
            this.tablePanel1.SetRow(this.label3, 2);
            this.label3.Size = new System.Drawing.Size(102, 17);
            this.label3.TabIndex = 13;
            this.label3.Text = "Image Folder";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.tablePanel1.SetColumn(this.label4, 0);
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(14, 158);
            this.label4.Name = "label4";
            this.tablePanel1.SetRow(this.label4, 5);
            this.label4.Size = new System.Drawing.Size(89, 17);
            this.label4.TabIndex = 14;
            this.label4.Text = "ContractNo";
            // 
            // txtContractNo
            // 
            this.tablePanel1.SetColumn(this.txtContractNo, 1);
            this.tablePanel1.SetColumnSpan(this.txtContractNo, 3);
            this.txtContractNo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtContractNo.Location = new System.Drawing.Point(121, 155);
            this.txtContractNo.Name = "txtContractNo";
            this.tablePanel1.SetRow(this.txtContractNo, 5);
            this.txtContractNo.Size = new System.Drawing.Size(353, 23);
            this.txtContractNo.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.tablePanel1.SetColumn(this.label5, 0);
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(14, 188);
            this.label5.Name = "label5";
            this.tablePanel1.SetRow(this.label5, 6);
            this.label5.Size = new System.Drawing.Size(62, 17);
            this.label5.TabIndex = 16;
            this.label5.Text = "Season";
            // 
            // txtSeason
            // 
            this.tablePanel1.SetColumn(this.txtSeason, 1);
            this.tablePanel1.SetColumnSpan(this.txtSeason, 3);
            this.txtSeason.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSeason.Location = new System.Drawing.Point(121, 185);
            this.txtSeason.Name = "txtSeason";
            this.tablePanel1.SetRow(this.txtSeason, 6);
            this.txtSeason.Size = new System.Drawing.Size(353, 23);
            this.txtSeason.TabIndex = 17;
            this.txtSeason.TextChanged += new System.EventHandler(this.txtSeason_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.tablePanel1.SetColumn(this.label6, 0);
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(14, 127);
            this.label6.Name = "label6";
            this.tablePanel1.SetRow(this.label6, 4);
            this.label6.Size = new System.Drawing.Size(84, 17);
            this.label6.TabIndex = 18;
            this.label6.Text = "Contractor";
            // 
            // txtType
            // 
            this.tablePanel1.SetColumn(this.txtType, 1);
            this.tablePanel1.SetColumnSpan(this.txtType, 3);
            this.txtType.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtType.Location = new System.Drawing.Point(121, 92);
            this.txtType.Name = "txtType";
            this.tablePanel1.SetRow(this.txtType, 3);
            this.txtType.Size = new System.Drawing.Size(353, 23);
            this.txtType.TabIndex = 19;
            this.txtType.TextChanged += new System.EventHandler(this.txtType_TextChanged);
            // 
            // rtbPO
            // 
            this.tablePanel1.SetColumn(this.rtbPO, 1);
            this.tablePanel1.SetColumnSpan(this.rtbPO, 3);
            this.rtbPO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbPO.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbPO.Location = new System.Drawing.Point(121, 245);
            this.rtbPO.Name = "rtbPO";
            this.tablePanel1.SetRow(this.rtbPO, 8);
            this.rtbPO.Size = new System.Drawing.Size(353, 73);
            this.rtbPO.TabIndex = 20;
            this.rtbPO.Text = "";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.tablePanel1.SetColumn(this.label7, 0);
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(14, 243);
            this.label7.Name = "label7";
            this.tablePanel1.SetRow(this.label7, 8);
            this.label7.Size = new System.Drawing.Size(102, 77);
            this.label7.TabIndex = 21;
            this.label7.Text = "List PO";
            // 
            // tablePanel1
            // 
            this.tablePanel1.AutoSize = true;
            this.tablePanel1.Columns.AddRange(new DevExpress.Utils.Layout.TablePanelColumn[] {
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 45.49F),
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 17.2F),
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 64.76F),
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 68.86F),
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 53.69F)});
            this.tablePanel1.Controls.Add(this.button3);
            this.tablePanel1.Controls.Add(this.lblStatus);
            this.tablePanel1.Controls.Add(this.txtSeason);
            this.tablePanel1.Controls.Add(this.txtType);
            this.tablePanel1.Controls.Add(this.label5);
            this.tablePanel1.Controls.Add(this.rtbPO);
            this.tablePanel1.Controls.Add(this.label7);
            this.tablePanel1.Controls.Add(this.btnLogin);
            this.tablePanel1.Controls.Add(this.label2);
            this.tablePanel1.Controls.Add(this.txtContractor);
            this.tablePanel1.Controls.Add(this.txtContractNo);
            this.tablePanel1.Controls.Add(this.Input_link);
            this.tablePanel1.Controls.Add(this.button1);
            this.tablePanel1.Controls.Add(this.label6);
            this.tablePanel1.Controls.Add(this.rtbLabel);
            this.tablePanel1.Controls.Add(this.label3);
            this.tablePanel1.Controls.Add(this.btnLoadTasks);
            this.tablePanel1.Controls.Add(this.dataGridView1);
            this.tablePanel1.Controls.Add(this.label4);
            this.tablePanel1.Controls.Add(this.label1);
            this.tablePanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePanel1.Location = new System.Drawing.Point(0, 0);
            this.tablePanel1.Name = "tablePanel1";
            this.tablePanel1.Rows.AddRange(new DevExpress.Utils.Layout.TablePanelRow[] {
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 33F),
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 11F),
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 34F),
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 31F),
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 33F),
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 30F),
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 30F),
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 31F),
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 77F),
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.AutoSize, 168F),
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 26F)});
            this.tablePanel1.Size = new System.Drawing.Size(615, 513);
            this.tablePanel1.TabIndex = 22;
            this.tablePanel1.UseSkinIndents = true;
            // 
            // button3
            // 
            this.tablePanel1.SetColumn(this.button3, 4);
            this.button3.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.button3.Location = new System.Drawing.Point(479, 12);
            this.button3.Name = "button3";
            this.tablePanel1.SetRow(this.button3, 0);
            this.button3.Size = new System.Drawing.Size(123, 29);
            this.button3.TabIndex = 22;
            this.button3.Text = "⚙️ Full Load";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(615, 513);
            this.Controls.Add(this.tablePanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Web Automation - Selenium Chrome";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tablePanel1)).EndInit();
            this.tablePanel1.ResumeLayout(false);
            this.tablePanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnLoadTasks;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.RichTextBox rtbLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtContractor;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox Input_link;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtContractNo;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtSeason;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtType;
        private System.Windows.Forms.RichTextBox rtbPO;
        private System.Windows.Forms.Label label7;
        private TablePanel tablePanel1;
        private System.Windows.Forms.Button button3;
    }
}
