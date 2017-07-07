using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace mimori
{
    public partial class Composer : Form
    {
        public Composer()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var mail = new Mimori.Mail();
            if (to_textBox.TextLength > 0)
                mail.toList.AddRange(to_textBox.Text.Split(';'));
            if (cc_textBox.TextLength > 0)
                mail.ccList.AddRange(cc_textBox.Text.Split(';'));
            if (bcc_textBox.TextLength > 0)
                mail.bccList.AddRange(bcc_textBox.Text.Split(';'));
            mail.subject = subject_textBox.Text;
            mail.message = message_textBox.Text;
            mail.SendMessage();
            this.Close();
        }

        private void subject_textBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
