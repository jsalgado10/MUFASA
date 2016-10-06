using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MATH3338
{
    public partial class AddEmployee : Form
    {
        private SQLiteData sqlData;

        public AddEmployee()
        {
            InitializeComponent();
            sqlData = new SQLiteData();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((jobTitleBox.Text == "") || (salaryBox.Text == "") || (idBox.Text == ""))
            {
                MessageBox.Show("All Required Fields have not been filled.");
            }
            else
            {
                try
                {
                    sqlData.addData(jobTitleBox.Text, salaryBox.Text, idBox.Text);
                    jobTitleBox.Text = "";
                    salaryBox.Text = "";
                    idBox.Text = "";
                    MessageBox.Show("New Data was Added Successfully");
                }
                catch(Exception addErr)
                {
                    MessageBox.Show(addErr.Message);
                }

            }
        }


    }
}
