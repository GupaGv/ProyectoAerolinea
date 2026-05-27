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
    public partial class frmInformeDestino : Form
    {
        cConexion cn;
        SqlCommand cmd;
        SqlDataAdapter da;
        DataTable dt;

        public frmInformeDestino()
        {
            InitializeComponent();
            cn = new cConexion();
        }

        private void frmInformeDestino_Load(object sender, EventArgs e)
        {
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM tblDestino", cn.AbrirConexion());
            DataTable dt = new DataTable();
            da.Fill(dt);
            dtgDestino.DataSource = dt;
        }
    }
}
