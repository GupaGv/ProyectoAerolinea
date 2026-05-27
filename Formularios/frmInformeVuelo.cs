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
    public partial class frmInformeVuelo : Form
    {
        cConexion cn;
        SqlCommand cmd;
        SqlDataAdapter da;
        DataTable dt;
        public frmInformeVuelo()
        {
            InitializeComponent();
            cn = new cConexion();
        }

        private void frmInformeVuelo_Load(object sender, EventArgs e)
        {
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM tblVuelo", cn.AbrirConexion());
            DataTable dt = new DataTable();
            da.Fill(dt);
            dtgVuelo.DataSource = dt;
        }
    }
}
