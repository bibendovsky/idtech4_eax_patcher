using System;
using System.Threading;
using System.Windows.Forms;

namespace Id4Eax4Patcher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += Application_ThreadException;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show("Unhandled exception:\n" + e.Exception.Message,
                "idTech4 EAX Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
