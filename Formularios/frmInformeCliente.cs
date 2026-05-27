using ProyectoAerolinea.Clases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProyectoAerolinea.Formularios
{
    public partial class frmInformeCliente : Form
    {
        cConexion cn;
        SqlCommand cmd;
        SqlDataAdapter da;
        DataTable dt;

        public frmInformeCliente()
        {
            InitializeComponent();
            cn = new cConexion();
        }

        private void frmInformeCliente_Load(object sender, EventArgs e)
        {
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM tblCliente", cn.AbrirConexion());
            DataTable dt = new DataTable();
            da.Fill(dt);
            dtgCliente.DataSource = dt;
        }
    }
}
