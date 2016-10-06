using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MATH3338
{
    public partial class SpashScreen : Form
    {
        public SpashScreen()
        {
            InitializeComponent();
        }
        Timer tmr;
        private void SpashScreen_Shown(object sender, EventArgs e)
        {
            tmr = new Timer();
            tmr.Interval = 3000;
            tmr.Start();
            tmr.Tick +=tmr_Tick;    
           
        }

private void tmr_Tick(object sender, EventArgs e)
{
 	
    tmr.Stop();
    Form1 mf = new Form1();
    mf.Show();
    this.Hide();
}
    }
}
