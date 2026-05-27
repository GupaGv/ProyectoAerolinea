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
    public partial class frmDestino : Form
    {
        cConexion cn;
        SqlCommand cmd;
        SqlDataAdapter da;
        DataTable dt, dtBusqueda;
        int i, contador, boton;

        public frmDestino()
        {
            InitializeComponent();
            boton = 0;
            cargarDestino();
        }

        // Metodo para llenar los campos de texto con los datos de la tabla
        void cargarDestino()
        {
            i = 0; boton = 0;
            cn = new cConexion();
            cmd = new SqlCommand("select * from tblDestino", cn.AbrirConexion());
            da = new SqlDataAdapter(cmd);
            dt = new DataTable();
            da.Fill(dt);
            if (dt.Rows.Count > 0)
                llenar(dt, i);
        }

        // Metodo para habilitar los campos de texto
        void habilita()
        {
            txtCodigo.Enabled = true;
            txtCiudad.Enabled = true;
            txtPais.Enabled = true;
        }

        // Metodo para deshabilitar los campos de texto
        void deshabilita()
        {
            txtCodigo.Enabled = false;
            txtCiudad.Enabled = false;
            txtPais.Enabled = false;
        }

        // Metodo para limpiar los campos de texto
        void limpiar()
        {
            txtCodigo.Clear();
            txtCiudad.Clear();
            txtPais.Clear();
        }

        // Metodo para llenar los campos de texto con los datos del DataTable
        void llenar(DataTable dt, int i)
        {
            txtCodigo.Text = dt.Rows[i][0].ToString();
            txtCiudad.Text = dt.Rows[i][1].ToString();
            txtPais.Text = dt.Rows[i][2].ToString();
            contador = dt.Rows.Count;
        }

        // Botones de navegación
        private void btnPrimero_Click(object sender, EventArgs e)
        {
            i = 0;
            llenar(dt, i);
        }

        private void btnAnterior_Click(object sender, EventArgs e)
        {
            i--;
            if (i == -1)
            {
                MessageBox.Show("Estás en el primer registro");
                i++;
            }
            llenar(dt, i);
        }

        private void btnSiguiente_Click(object sender, EventArgs e)
        {
            i++;
            if (i == contador)
            {
                MessageBox.Show("Estás en el último registro");
                i--;
            }
            llenar(dt, i);
        }

        private void btnUltimo_Click(object sender, EventArgs e)
        {
            i = contador - 1;
            llenar(dt, i);
        }

        // Botones de acción
        private void btnIngreso_Click(object sender, EventArgs e)
        {
            boton = 1;
            limpiar();
            habilita();
            txtCodigo.Focus();
        }

        private void btnModificiacion_Click(object sender, EventArgs e)
        {
            boton = 2;
            limpiar();
            habilita();
            txtCodigo.Focus();
        }

        private void btnConsulta_Click(object sender, EventArgs e)
        {
            boton = 3;
            limpiar();
            txtCodigo.Enabled = true;
            txtCodigo.Focus();
        }

        private void btnRetiro_Click(object sender, EventArgs e)
        {
            boton = 4;
            limpiar();
            txtCodigo.Enabled = true;
            txtCodigo.Focus();
        }

        // Evento para validar el código del destino al salir del campo de texto
        private void txtCodigo_Leave(object sender, EventArgs e)
        {
            cmd = new SqlCommand("select * from tblDestino where codigoAeropuerto = '" + txtCodigo.Text + "'", cn.AbrirConexion());
            da = new SqlDataAdapter(cmd);
            dtBusqueda = new DataTable();
            da.Fill(dtBusqueda);

            // Ingreso
            if (boton == 1)
            {
                if (dtBusqueda.Rows.Count > 0)
                {
                    MessageBox.Show("El Destino ya existe");
                    llenar(dtBusqueda, 0);
                }
                else
                {
                    btnGuardar.Visible = true;
                }
            }

            // Modificar
            if (boton == 2)
            {
                if (dtBusqueda.Rows.Count > 0)
                {
                    llenar(dtBusqueda, 0);
                    btnGuardar.Visible = true;
                }
                else
                {
                    MessageBox.Show("El Destino NO existe");
                }
            }

            // Consulta
            if (boton == 3)
            {
                if (dtBusqueda.Rows.Count > 0)
                {
                    llenar(dtBusqueda, 0);
                }
                else
                {
                    MessageBox.Show("El Destino NO existe");
                }
            }

            // Retiro
            if (boton == 4)
            {
                if (dtBusqueda.Rows.Count > 0)
                {
                    llenar(dtBusqueda, 0);
                    var result = MessageBox.Show("¿Realmente desea eliminar este destino?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        SqlCommand comando = new SqlCommand("delete from tblDestino where codigoAeropuerto = '" + txtCodigo.Text + "'", cn.AbrirConexion());
                        comando.ExecuteNonQuery();
                        MessageBox.Show("Destino eliminado");
                        limpiar();
                    }
                }
                else
                {
                    MessageBox.Show("El Destino NO existe");
                }
            }
        }

        // Evento para guardar o modificar el destino según la acción seleccionada
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (boton == 1) // Guardar nuevo destino
            {
                cmd = new SqlCommand("insert into tblDestino values('" + txtCodigo.Text + "', '" + txtCiudad.Text + "', '" + txtPais.Text + "')", cn.AbrirConexion());
                cmd.ExecuteNonQuery();
                MessageBox.Show("Destino guardado exitosamente");
            }

            if (boton == 2) // Modificar destino existente
            {
                cmd = new SqlCommand("update tblDestino set ciudad='" + txtCiudad.Text + "', pais='" + txtPais.Text + "' where codigoAeropuerto='" + txtCodigo.Text + "'", cn.AbrirConexion());
                cmd.ExecuteNonQuery();
                MessageBox.Show("Destino modificado exitosamente");
            }
            boton = 0;
            btnGuardar.Visible = false;
            deshabilita();
        }

    }
}
