using System;
using System.Windows.Forms;
using Photo8;

namespace Photo8
{
    public partial class DelegateFunction : Form
    {
        public DelegateFunction()
        {
            InitializeComponent();
        }

        private void btnOpenForm1_Click(object sender, EventArgs e)
        {
            var f1 = new Form1();
            f1.FormClosed += (_, __) => this.Show();
            f1.Show();
            this.Hide();
        }

        private void btnOpenPdfToSheet_Click(object sender, EventArgs e)
        {
            var f2 = new PDFToSheet_PdfUI();
            f2.FormClosed += (_, __) => this.Show();
            f2.Show();
            this.Hide();
        }
    }
}