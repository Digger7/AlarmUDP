using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class FormAlert : Form
    {
        public FormAlert(string userName="", string mashineName="", string date = "")
        {
            InitializeComponent();

            labelAuthor.Text = $"{userName}";
            labelMashineName.Text = $"{mashineName}";
            labelDate.Text = $"{date}";
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
