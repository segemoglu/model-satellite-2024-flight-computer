using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mergen_GCSS
{
    internal static class Program
    {
        /// <summary>
        /// Uygulamanın ana girdi noktası.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            loginForm login = new loginForm();
            if (login.ShowDialog() == DialogResult.OK)
            {
                // Display the SplashScreen
                splash splash = new splash();
                splash.Show();
                Application.DoEvents();

                // Wait for the SplashScreen to close
                System.Threading.Thread.Sleep(3000);

                // Show the MainForm
                Application.Run(new mainForm());
            }
            else
            {
                // Kullanıcı giriş yapmazsa uygulamayı kapat
                Application.Exit();
            }
        }
    }
}
