/*
 AOD-Downloader Copyright (C) 2019  BreakingBread0
 This program comes with ABSOLUTELY NO WARRANTY.
 This is free software, and you are welcome to redistribute it
 under certain conditions.
 
 Please read the license for further information:
 https://www.gnu.org/licenses/gpl-3.0.en.html (ENGLISH)
 https://www.gnu.org/licenses/gpl-3.0.de.html (GERMAN)
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace TSConvert {
    public partial class LoginForm : Form {
        public LoginForm() {
            InitializeComponent();
            if (File.Exists("data")) {
                var udata = File.ReadAllText("data").Split("|".ToCharArray());
                textBox1.Text = udata[0];
                textBox2.Text = udata[1];
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            File.WriteAllText("data", textBox1.Text + "|" + textBox2.Text);
            Program.user = textBox1.Text;
            Program.pass = textBox2.Text;
            this.Close();
        }
    }
}
