using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mmc_production
{
    public partial class pwdwindow : Form
    {
        public pwdwindow()
        {
            InitializeComponent();

            cancel.DialogResult = DialogResult.Cancel;
            confirm.DialogResult = DialogResult.OK;
            //confirm.Visible = false;
        }

        private void PwdInput_Load(object sender, EventArgs e)
        {

        }

        private void cancel_Click(object sender, EventArgs e)
        {
            //this.DialogResult = false;
            this.Close();
        }

        private void confirm_Click(object sender, EventArgs e)
        {
            //IntPtr p = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(this.pwd.);
            //string pwdstr = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(p);
            //string pwdstr = this.pwd.Text;
            //
            this.Close();
        }

        private void verify_Click(object sender, EventArgs e)
        {
            if (ProdDataHandler.PRODUCTION_INFO_UPDATE_PWD.Equals(this.pwd.Text))
            {
                confirm.PerformClick();
            }else
            {
                this.prompt.Visible = true;
                this.pwd.Clear();
            }
        }
    }
}
