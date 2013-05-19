namespace UniTerm
{
    partial class frmProp
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lstProp = new System.Windows.Forms.ListBox();
            this.grdProp = new System.Windows.Forms.DataGridView();
            this.btnRefresh = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.grdProp)).BeginInit();
            this.SuspendLayout();
            // 
            // lstProp
            // 
            this.lstProp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lstProp.FormattingEnabled = true;
            this.lstProp.Location = new System.Drawing.Point(13, 13);
            this.lstProp.Name = "lstProp";
            this.lstProp.Size = new System.Drawing.Size(139, 147);
            this.lstProp.TabIndex = 0;
            this.lstProp.SelectedIndexChanged += new System.EventHandler(this.lstProp_SelectedIndexChanged);
            // 
            // grdProp
            // 
            this.grdProp.AllowUserToAddRows = false;
            this.grdProp.AllowUserToDeleteRows = false;
            this.grdProp.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdProp.Location = new System.Drawing.Point(169, 13);
            this.grdProp.Name = "grdProp";
            this.grdProp.ReadOnly = true;
            this.grdProp.Size = new System.Drawing.Size(381, 440);
            this.grdProp.TabIndex = 1;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(13, 167);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(139, 27);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "Перечитать данные";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // frmProp
            // 
            this.ClientSize = new System.Drawing.Size(564, 468);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.grdProp);
            this.Controls.Add(this.lstProp);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "frmProp";
            this.Load += new System.EventHandler(this.frmProp_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grdProp)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lstProp;
        private System.Windows.Forms.DataGridView grdProp;
        private System.Windows.Forms.Button btnRefresh;

    }
}

