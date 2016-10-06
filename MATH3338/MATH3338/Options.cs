using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MATH3338
{
    public partial class Options : Form
    {
        private ini config = new ini(Directory.GetCurrentDirectory().ToString() + "\\Math.ini");
        public Options()
        {
            InitializeComponent();
        }

        private void Options_Load(object sender, EventArgs e)
        {
            loadvar();
        }
        private void loadvar()
        {
            fl.Text = config.Read("CONSTANT", "LF");
            al.Text = config.Read("CONSTANT", "LA");
            fc.Text = config.Read("CONSTANT", "FULL");
            ac.Text = config.Read("CONSTANT", "ASSOCIATE");
        }

        private void saveVal()
        {
            config.Write("CONSTANT", "LF",fl.Text);
            config.Write("CONSTANT", "LA",al.Text);
            config.Write("CONSTANT", "FULL", fc.Text);
            config.Write("CONSTANT", "ASSOCIATE", ac.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            saveVal();
            this.Close();
        }
    }
}
