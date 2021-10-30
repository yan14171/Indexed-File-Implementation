using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using laba2;

namespace visualisation
{
    public partial class Form1 : Form
    {
        LABFile file = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        { 
            file = new LABFile("labfile.lab", new FileSettings()
            {
                LZ = 2,
                LK = 3,
                KZ = 100,
                LB = 41
            });
        }

        private IEnumerable<string> getRawFileString() {
            if (file == null)
                return null;
            return new List<String> { "Index Block: \n" }
                .Concat(file
                         .GetAllIndexLines()
                         .Select(n => n.ToString()))
                .Concat(new List<String> { "__________\nMain Block: \n" })
                .Concat(file
                         .GetAllLines()
                         .Select(n => n.ToString()));
        }

        private void richTextBox1_Enter(object sender, EventArgs e)
        {
            try
            {
                (sender as RichTextBox).ResetText();
                foreach (var text in getRawFileString())
                    (sender as RichTextBox).AppendText(text + "\n");
            }
            catch(NullReferenceException ex) { 
                MessageBox.Show("Error occured. \nFile probably wasn't initialized." +
                "\nLook down for details \n" + ex.Message);
            }
        }

        private void button4_Click(object sender, System.EventArgs e)
        {
            byte[] bts = new byte[2];
            var r = new Random();
            for (int i = 0; i < bts.Length; i++)
            {
                bts[i] = (byte)r.Next(27, 51);
            }
            string symbols = Encoding.UTF8.GetString(bts,0,2);

            file.AddLine(new Line(symbols));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            file?.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == string.Empty || textBox3.Text == string.Empty)
            {
                MessageBox.Show("Give full line key to delete");
                return;
            }

            var blockKey = Convert.ToByte(textBox3.Text);
            var lineKey = Convert.ToByte(textBox1.Text);
            var bts = new byte[3] { blockKey, Encoding.UTF8.GetBytes("-")[0], lineKey };

            if (!file.DeleteLine(bts))
                MessageBox.Show("Wrong index");
            else
                MessageBox.Show("Deletion completed");

        }

        private void button3_Click(object sender, EventArgs e)
        {
            var blockKey = Convert.ToByte(textBox2.Text);
            var lineKey = Convert.ToByte(textBox4.Text);
            var bts = new byte[3] { blockKey, Encoding.UTF8.GetBytes("-")[0], lineKey };

            var line = file.GetLine(bts);

            MessageBox.Show($"Line: {line.ToString()}");
        }
    }
}
