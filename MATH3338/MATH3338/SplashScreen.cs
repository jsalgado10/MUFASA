using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace MATH3338
{
    public partial class SplashScreen : Form
    {
        //Timer
        private System.Windows.Forms.Timer sTimer;
        private IContainer components;

        public SplashScreen()
        {
            InitializeComponent();
        }

        private void beginSScreen()
        {
            this.Opacity = 0.0;
            sTimer = new System.Windows.Forms.Timer();
            sTimer.Start();
        }

        private void sTimer_Tick(object sender, EventArgs e)
        {
            sTimer.Stop();
            this.Close();
        }
        
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashScreen));
            this.sTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // sTimer
            // 
            this.sTimer.Enabled = true;
            this.sTimer.Interval = 3000;
            this.sTimer.Tick += new System.EventHandler(this.sTimer_Tick);
            // 
            // SplashScreen
            // 
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = global::MATH3338.Properties.Resources.StatsCalLogo;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(411, 394);
            this.ControlBox = false;
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SplashScreen";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TransparencyKey = System.Drawing.Color.Black;
            this.ResumeLayout(false);

        }


    }
}
