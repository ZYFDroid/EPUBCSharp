using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EPUBium
{
    public partial class FrmChapters : Form
    {
        public FrmChapters()
        {
            InitializeComponent();
        }

        private void FrmChapters_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.ic_book;
        }
    }
}
