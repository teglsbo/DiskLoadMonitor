using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DiskLoadMonitor {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new Form1 {Visible = false};
            Application.Run();
        }
    }
}
