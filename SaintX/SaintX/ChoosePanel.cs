using SaintX.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaintX
{
    public partial class ChoosePanel : Form
    {
        public ChoosePanel()
        {
            InitializeComponent();
            this.FormClosing += ChoosePanel_FormClosing;
        }

        void ChoosePanel_FormClosing(object sender, FormClosingEventArgs e)
        {
            GlobalVars.Instance.PanelType = rdbDNA.Checked ? "DNA" : "RNA";
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            this.Close();
         
        }
    }
}
