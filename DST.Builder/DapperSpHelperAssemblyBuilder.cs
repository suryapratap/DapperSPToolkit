using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using DST.Builder.Assembly;

namespace DST.Builder
{
    public partial class DapperSpHelperAssemblyBuilder : Form
    {
        public DapperSpHelperAssemblyBuilder()
        {
            InitializeComponent();
        }

        private void btnBuildAssembly_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNamespace.Text)) return;
            if (string.IsNullOrEmpty(txtDbConn.Text)) return;
            var codeBuilder = new ProcCodeBuilder(txtNamespace.Text, txtDbConn.Text);
            var code = codeBuilder.ToCode();
            // System.IO.File.WriteAllText($"{txtNamespace.Text}.cs",code);
            AssemblyBuilder.GenerateAssembly(code, $"{txtNamespace.Text}.dll");
        }

        private void txtDbConn_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // ReSharper disable once HeapView.ObjectAllocation.Evident
                var conn = new SqlConnection(txtDbConn.Text);
                txtNamespace.Text = conn.Database;
            }
            catch
            {
                txtNamespace.Text = "";
            }
        }
    }
}