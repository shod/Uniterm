using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using UniTerm.Sys;
using System.Windows.Forms;

namespace UniTerm
{
    public partial class frmRepForm : Form
    {
        public frmRepForm()
        {
            InitializeComponent();
        }

        public string DateFrom
        {
            get
            {
                if (dtFrom.Enabled)
                {
                    DateTime dt = dtFrom.Value.Date;
                    return dt.Day + "." + dt.Month + "." + dt.Year;                    
                }
                else
                {
                    return "";
                }
            }
        }

        public string DateTo
        {
            get
            {
                if (dtTo.Enabled)
                {
                    DateTime dt = dtTo.Value.Date;
                    return dt.Day + "." + dt.Month + "." + dt.Year;
                }
                else
                {
                    return "";
                }
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        public void ShowElement(String Name)
        {
            switch (Name)
            {
                case "from":
                    {
                        lblFrom.Visible = true;
                        dtFrom.Visible = true;

                        lblFrom.Enabled = true;
                        dtFrom.Enabled = true;
                        break;
                    }
                case "to":
                    {
                        lblTo.Visible = true;
                        dtTo.Visible = true;

                        lblTo.Enabled = true;
                        dtTo.Enabled = true;
                        break;
                    }
            }
        }

        private void frmRepForm_Load(object sender, EventArgs e)
        {
            dtFrom.CustomFormat = "MMMM dd, yyyy - dddd";

        }
    }
}