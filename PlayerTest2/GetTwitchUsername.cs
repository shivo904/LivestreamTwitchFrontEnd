using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace PlayerTest2
{
    public partial class GetTwitchUsername : Form
    {
        public GetTwitchUsername()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ConfigurationManager.AppSettings["username"] = textBox1.Text.Trim().ToLower();
            this.Hide();
        }
    }
}
