using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EPUBium
{
    public partial class FrmFileNameDialog : Form
    {
        public FrmFileNameDialog()
        {
            InitializeComponent();
        }

        private void FrmFileNameDialog_Load(object sender, EventArgs e)
        {
            Icon = Properties.Resources.ic_book;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.TextLength <= 0) {
                label1.Visible = true;
                button1.Enabled = false;
                return;
            }
            if (Path.GetInvalidFileNameChars().ToList().Any(l => textBox1.Text.Contains(l)))
            {
                label1.Visible = true;
                button1.Enabled = false;
                return;
            }
            label1.Visible = false;
            button1.Enabled = true;
        }

        public static bool show(out string filename) {
            FrmFileNameDialog fld = new FrmFileNameDialog();
            DialogResult dr = fld.ShowDialog();
            filename = fld.textBox1.Text;
            return dr == DialogResult.OK;
        }


    }
}
