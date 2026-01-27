using System;
using System.Windows.Forms;
using Photo8;

namespace Photo8
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new DelegateFunction());
        }
    }
}