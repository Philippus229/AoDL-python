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
    public partial class ChEpi : Form {
        public ChEpi(string AnimeTitle, Dictionary<string, string> episodes) {
            InitializeComponent();
            label1.Text = AnimeTitle;
            label1.Left = this.Width / 2 - label1.Width / 2;
            Episodes = episodes;
            checkedListBox1.Items.Clear();
            foreach (var item in episodes) {
                checkedListBox1.Items.Add(item.Key);
            }
        }

        Dictionary<string, string> Episodes;
        public Dictionary<string, string> EpisodesToDownload = new Dictionary<string, string>();
        //public List<string> EpisodesToDownload;

        private void button2_Click(object sender, EventArgs e) {
            for (int i = 0; i < checkedListBox1.Items.Count; i++) {
                checkedListBox1.SetItemChecked(i, false);
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            for (int i = 0; i < checkedListBox1.Items.Count; i++) {
                checkedListBox1.SetItemChecked(i, true);
            }
        }
        
        private void ChEpi_FormClosing(object sender, FormClosingEventArgs e) {

        }

        private void button3_Click(object sender, EventArgs e) {
            for (int i = 0; i < checkedListBox1.Items.Count; i++) {
                if (checkedListBox1.GetItemChecked(i)) {
                    EpisodesToDownload.Add(Episodes.ElementAt(i).Key, Episodes.ElementAt(i).Value);
                }
            }
            Close();
        }
    }
}
