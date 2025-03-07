using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Mergen_GCSS
{
    public partial class loginForm : Form
    {
        string username, password;
        private Dictionary<string, string> users = new Dictionary<string, string>()
    {
        { "kgnybr", "elektronikbirimbaskani123" },
        { "salih", "123" },
        { "ikram", "password2" },
         { "yeo", "password3" },
         { "mek", "generalmahmut" },
         { "cio", "cio" }
        };
        public loginForm()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                // Enter tuşuna basıldığında Giriş butonunu tetikleyin
                button1.PerformClick();
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            username = textBox1.Text;
            password = textBox2.Text;
            if (users.ContainsKey(username) && users[username] == password)
            {
                label3.ForeColor = Color.Green;
                label3.Text = "Giriş Başarılı!";
                await Task.Delay(1000);
                this.DialogResult = DialogResult.OK; // Başarılı giriş
                this.Close();
            }
            else
            {
                label3.ForeColor = Color.Red;
                label3.Text = "Eksik ya da Yanlış Giriş Yaptınız!";
            }
        }
    }
}
