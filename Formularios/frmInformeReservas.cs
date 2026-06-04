using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using ProyectoAerolinea.Clases;

namespace ProyectoAerolinea.Formularios
{
    public partial class frmInformeReservas : Form
    {
        public frmInformeReservas()
        {
            InitializeComponent();
        }

        private void frmInformeReservas_Load(object sender, EventArgs e)
        {
            CargarReservas();
        }

        void CargarReservas()
        {
            try
            {
                cConexion cn = new cConexion();
                SqlCommand cmd = new SqlCommand(@"
            SELECT
                r.codigoReserva     AS 'Código',
                CONVERT(VARCHAR(10), r.fechaReserva, 103) AS 'Fecha',
                c.nombre + ' ' + c.apellido AS 'Cliente',
                c.cedula            AS 'Cédula',
                v.numeroVuelo       AS 'Vuelo',
                d1.ciudad + ' → ' + d2.ciudad AS 'Ruta',
                CONVERT(VARCHAR(10), v.fechaSalida, 103)  AS 'Fecha Vuelo',
                r.cantidadPasajeros AS 'Pasajeros',
                r.tipoTiquete       AS 'Tipo',
                CASE r.equipaje WHEN 1 THEN 'Sí' ELSE 'No' END AS 'Equipaje',
                r.precioTotal       AS 'Precio Total',
                r.estado            AS 'Estado'
            FROM tblReserva r
            INNER JOIN tblCliente c  ON r.cedula       = c.cedula
            INNER JOIN tblVuelo v    ON r.numeroVuelo   = v.numeroVuelo
            INNER JOIN tblDestino d1 ON v.origen        = d1.codigoAeropuerto
            INNER JOIN tblDestino d2 ON v.destino       = d2.codigoAeropuerto
            ORDER BY r.fechaReserva DESC",
                    cn.AbrirConexion());

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                cn.CerrarConexion();

                dtgReservas.DataSource = dt;
                AjustarColumnas();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar reservas:\n" + ex.Message);
            }
        }

        void AjustarColumnas()
        {
            if (dtgReservas.Columns.Count == 0) return;

            dtgReservas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            int anchoTotal = 0;
            foreach (DataGridViewColumn col in dtgReservas.Columns)
                anchoTotal += col.Width;

            dtgReservas.AutoSizeColumnsMode = anchoTotal < dtgReservas.ClientSize.Width
                ? DataGridViewAutoSizeColumnsMode.Fill
                : DataGridViewAutoSizeColumnsMode.AllCells;
        }

    }
}
