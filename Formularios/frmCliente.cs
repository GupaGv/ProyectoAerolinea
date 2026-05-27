using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;
using ProyectoAerolinea.Clases;
using System.Xml.Serialization;

namespace ProyectoAerolinea.Formularios
{
    public partial class frmCliente : Form
    {
        cConexion cn;
        SqlCommand cmd;
        SqlDataAdapter da;
        DataTable dt, dtBusqueda;
        int i, contador, boton;

        public frmCliente()
        {
            InitializeComponent();
            boton = 0;
            cargarCliente();
        }

        // Metodo para cargar los datos de la tabla tblCliente en el DataTable
        void cargarCliente()
        {
            i = 0; boton = 0;
            cn = new cConexion();
            cmd = new SqlCommand("select * from tblCliente", cn.AbrirConexion());
            da = new SqlDataAdapter(cmd);
            dt = new DataTable();
            da.Fill(dt);
            if (dt.Rows.Count > 0)
                llenar(dt, i);
        }

        // Metodo para habilitar los campos de texto
        void habilita()
        {
            txtCedula.Enabled = true;
            txtNombre.Enabled = true;
            txtApellido.Enabled = true;
            txtDireccion.Enabled = true;
            txtTelefono.Enabled = true;
            txtEmail.Enabled = true;
            txtPasaporte.Enabled = true;
        }

        // Metodo para deshabilitar los campos de texto
        void deshabilita()
        {
            txtCedula.Enabled = false;
            txtNombre.Enabled = false;
            txtApellido.Enabled = false;
            txtDireccion.Enabled = false;
            txtTelefono.Enabled = false;
            txtEmail.Enabled = false;
            txtPasaporte.Enabled = false;
        }

        // Metodo para limpiar los campos de texto
        void limpiar()
        {
            txtCedula.Clear();
            txtNombre.Clear();
            txtApellido.Clear();
            txtDireccion.Clear();
            txtTelefono.Clear();
            txtEmail.Clear();
            txtPasaporte.Clear();
        }

        // Metodo para llenar los campos de texto con los datos del DataTable
        void llenar(DataTable dt, int i)
        {
              txtCedula.Text = dt.Rows[i][0].ToString();
            txtNombre.Text = dt.Rows[i][1].ToString();
            txtApellido.Text = dt.Rows[i][2].ToString();
            txtTelefono.Text = dt.Rows[i][3].ToString();
            txtEmail.Text = dt.Rows[i][4].ToString();
            txtPasaporte.Text = dt.Rows[i][5].ToString();
            txtDireccion.Text = dt.Rows[i][6].ToString();
            contador = dt.Rows.Count;
        }

        //Botones de navegación
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
            txtCedula.Focus();
        }

        private void btnModificiacion_Click(object sender, EventArgs e)
        {
            boton = 2;
            limpiar();
            habilita();
            txtCedula.Focus();
        }

        private void txtCedula_Leave(object sender, EventArgs e)
        {
            cmd = new SqlCommand("select * from tblCliente where cedula = '" + txtCedula.Text + "'", cn.AbrirConexion());
            da = new SqlDataAdapter(cmd);
            dtBusqueda = new DataTable();
            da.Fill(dtBusqueda);

            // Ingreso
            if (boton == 1)
            {
                if (dtBusqueda.Rows.Count > 0)
                {
                    MessageBox.Show("El cliente ya existe");
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
                    MessageBox.Show("El cliente NO existe");
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
                    MessageBox.Show("El cliente NO existe");
                }
            }

            // Retiro
            if (boton == 4)
            {
                if (dtBusqueda.Rows.Count > 0)
                {
                    llenar(dtBusqueda, 0);
                    var result = MessageBox.Show("¿Realmente desea eliminar este cliente?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        SqlCommand comando = new SqlCommand("DELETE FROM tblCliente WHERE cedula = '" + txtCedula.Text + "'", cn.AbrirConexion());
                        comando.ExecuteNonQuery();
                        MessageBox.Show("Cliente eliminado");
                        limpiar();
                    }
                }
                else
                {
                    MessageBox.Show("El cliente NO existe");
                }
            }
        }

        private void btnConsulta_Click(object sender, EventArgs e)
        {
            boton = 3;
            limpiar();
            txtCedula.Enabled = true;
            txtCedula.Focus();
        }

        private void btnRetiro_Click(object sender, EventArgs e)
        {
            boton = 4;
            limpiar();
            txtCedula.Enabled = true;
            txtCedula.Focus();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (boton == 1) // Guardar nuevo cliente
            {
                cmd = new SqlCommand("insert into tblCliente values('" + txtCedula.Text + "', '" + txtNombre.Text + "', '" + txtApellido.Text + "', '" + txtTelefono.Text + "', '" + txtEmail.Text + "', '" + txtPasaporte.Text + "', '" + txtDireccion.Text + "')", cn.AbrirConexion());
                cmd.ExecuteNonQuery();
                MessageBox.Show("Cliente guardado exitosamente");
            }

            if (boton == 2) // Modificar cliente existente
            {
                cmd = new SqlCommand("update tblCliente set nombre='" + txtNombre.Text + "', apellido='" + txtApellido.Text + "', telefono='" + txtTelefono.Text + "', email='" + txtEmail.Text + "', pasaporte='" + txtPasaporte.Text + "', direccion='" + txtDireccion.Text + "' where cedula='" + txtCedula.Text + "'", cn.AbrirConexion());
                cmd.ExecuteNonQuery();
                MessageBox.Show("Cliente modificado exitosamente");
            }

            boton= 0;
            btnGuardar.Visible = false;
            deshabilita();
        }
    }
}
