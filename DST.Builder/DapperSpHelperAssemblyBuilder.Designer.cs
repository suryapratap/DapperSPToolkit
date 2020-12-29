namespace DST.Builder
{
    partial class DapperSpHelperAssemblyBuilder
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
            this.lblDbConn = new System.Windows.Forms.Label();
            this.txtDbConn = new System.Windows.Forms.TextBox();
            this.btnBuildAssembly = new System.Windows.Forms.Button();
            this.txtNamespace = new System.Windows.Forms.TextBox();
            this.lblNameSpace = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblDbConn
            // 
            this.lblDbConn.Location = new System.Drawing.Point(20, 13);
            this.lblDbConn.Name = "lblDbConn";
            this.lblDbConn.Size = new System.Drawing.Size(110, 19);
            this.lblDbConn.TabIndex = 0;
            this.lblDbConn.Text = "Connection String\r\n";
            this.lblDbConn.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtDbConn
            // 
            this.txtDbConn.Location = new System.Drawing.Point(136, 12);
            this.txtDbConn.Multiline = true;
            this.txtDbConn.Name = "txtDbConn";
            this.txtDbConn.Size = new System.Drawing.Size(612, 188);
            this.txtDbConn.TabIndex = 1;
            this.txtDbConn.TextChanged += new System.EventHandler(this.txtDbConn_TextChanged);
            // 
            // btnBuildAssembly
            // 
            this.btnBuildAssembly.Location = new System.Drawing.Point(636, 258);
            this.btnBuildAssembly.Name = "btnBuildAssembly";
            this.btnBuildAssembly.Size = new System.Drawing.Size(112, 33);
            this.btnBuildAssembly.TabIndex = 1;
            this.btnBuildAssembly.Text = "Build Assembly";
            this.btnBuildAssembly.UseVisualStyleBackColor = true;
            this.btnBuildAssembly.Click += new System.EventHandler(this.btnBuildAssembly_Click);
            // 
            // txtNamespace
            // 
            this.txtNamespace.Location = new System.Drawing.Point(136, 206);
            this.txtNamespace.Name = "txtNamespace";
            this.txtNamespace.Size = new System.Drawing.Size(612, 20);
            this.txtNamespace.TabIndex = 3;
            // 
            // lblNameSpace
            // 
            this.lblNameSpace.Location = new System.Drawing.Point(20, 206);
            this.lblNameSpace.Name = "lblNameSpace";
            this.lblNameSpace.Size = new System.Drawing.Size(110, 19);
            this.lblNameSpace.TabIndex = 2;
            this.lblNameSpace.Text = "Namespace";
            this.lblNameSpace.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // DapperSpHelperAssemblyBuilder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(760, 304);
            this.Controls.Add(this.txtNamespace);
            this.Controls.Add(this.lblNameSpace);
            this.Controls.Add(this.btnBuildAssembly);
            this.Controls.Add(this.txtDbConn);
            this.Controls.Add(this.lblDbConn);
            this.Location = new System.Drawing.Point(15, 15);
            this.Name = "DapperSpHelperAssemblyBuilder";
            this.Text = "Dapper SP Helper Assemby Builder";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblNameSpace;
        private System.Windows.Forms.TextBox txtNamespace;


        private System.Windows.Forms.TextBox txtDbConn;

        private System.Windows.Forms.Label lblDbConn;
        
        private System.Windows.Forms.Button btnBuildAssembly;

        #endregion
    }
}