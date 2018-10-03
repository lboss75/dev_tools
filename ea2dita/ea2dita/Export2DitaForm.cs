using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ea2dita
{
    public partial class Export2DitaForm : Form
    {
        public Export2DitaForm()
        {
            InitializeComponent();
        }

        public string DitaMapFile { get; set; }

        private void ditamapSelectBtn_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Файлы DITA Map (*.ditamap)|*.ditamap|Все файлы|*.*",
                FileName = ditamapInput.Text
            };

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                ditamapInput.Text = dlg.FileName;
            }
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            DitaMapFile = this.ditamapInput.Text;
            HideEmptyElements = this.hideEmptyElementsCb.Checked;
            DialogResult = DialogResult.OK;
        }

        public bool HideEmptyElements { get; set; }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
