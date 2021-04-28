using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTCMachine
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (Mutex mutex = new Mutex(false, "{8F6F0AC4-BAA6-45fd-A64F-72FD4E6BDA34}"))
            {
                if (!mutex.WaitOne(0, false))
                {
                    int num = (int)MessageBox.Show("Process already exist.");
                    Environment.Exit(0);
                }
                Application.Run((Form)new DoTenTraderMain());
            }
        }
    }
}
