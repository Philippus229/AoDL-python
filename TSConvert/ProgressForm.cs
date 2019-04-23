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

namespace TSConvert {
    public partial class ProgressForm : Form {
        public ProgressForm() {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e) {
            this.BeginInvoke(new MethodInvoker(() => {
                progressBar1.Maximum = Program.Chunks;
                progressBar1.Value = Program.currentChunks;
            }));
        }
    }
}
