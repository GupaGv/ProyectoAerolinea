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
    public partial class frmVuelo : Form
    {
        cConexion cn;
        SqlCommand cmd;
        SqlDataAdapter da;
        DataTable dt, dtBusqueda;
        int i, contador, boton;

        public frmVuelo()
        {
            InitializeComponent();
            boton = 0;
            CargarDestinos();
            CargarVuelos();
        }

        // Metodo para cargar los datos de la tabla tblVuelo en el DataTable y llenar los campos de texto
        void CargarVuelos()
        {
            i = 0; 
            boton = 0;
            cn = new cConexion();
            cmd = new SqlCommand("select * from tblVuelo", cn.AbrirConexion());
            da = new SqlDataAdapter(cmd);
            dt = new DataTable();
            da.Fill(dt);
            if (dt.Rows.Count > 0)
                llenar(dt, i);
        }

        // Metodo para cargar los datos de la tabla tblDestino en el DataTable y llenar los ComboBox
        void CargarDestinos()
        {
            cn = new cConexion();
            cmd = new SqlCommand("select codigoAeropuerto, ciudad from tblDestino", cn.AbrirConexion());
            da = new SqlDataAdapter(cmd);
            DataTable dtDestinos = new DataTable();
            da.Fill(dtDestinos);

            cmbOrigen.DataSource = dtDestinos.Copy();
            cmbOrigen.DisplayMember = "ciudad";
            cmbOrigen.ValueMember = "codigoAeropuerto";
            cmbOrigen.SelectedIndex = -1;

            cmbDestino.DataSource = dtDestinos.Copy();
            cmbDestino.DisplayMember = "ciudad";
            cmbDestino.ValueMember = "codigoAeropuerto";
            cmbDestino.SelectedIndex = -1;

            cn.CerrarConexion();
        }

        // Metodo para habilitar los campos de texto
        void habilita()
        {
            txtNumeroVuelo.Enabled = true;
            cmbOrigen.Enabled = true;
            cmbDestino.Enabled = true;
            dtpFechaSalida.Enabled = true;
            dtpFechaLlegada.Enabled = true;
            txtAsientosTotales.Enabled = true;
            txtAsientosDisponibles.Enabled = true;
            txtPrecioBase.Enabled = true;
        }

        // Metodo para deshabilitar los campos de texto
        void deshabilitar()
        {
            txtNumeroVuelo.Enabled = false;
            cmbOrigen.Enabled = false;
            cmbDestino.Enabled = false;
            dtpFechaSalida.Enabled = false;
            dtpFechaLlegada.Enabled = false;
            txtAsientosTotales.Enabled = false;
            txtAsientosDisponibles.Enabled = false;
            txtPrecioBase.Enabled = false;
        }

        // Metodo para limpiar los campos de texto
        void limpiar()
        {
            txtNumeroVuelo.Clear();
            cmbOrigen.SelectedIndex = -1;
            cmbDestino.SelectedIndex = -1;
            dtpFechaSalida.Value = DateTime.Now;
            dtpFechaLlegada.Value = DateTime.Now;
            txtAsientosTotales.Clear();
            txtAsientosDisponibles.Clear();
            txtPrecioBase.Clear();

        }

        // Metodo para llenar los campos de texto con los datos del DataTable
        void llenar(DataTable dt, int i)
        {
            contador = dt.Rows.Count;
            if (contador > 0)
            {
                txtNumeroVuelo.Text = dt.Rows[i][0].ToString();

                // Seleccionar origen
                string codigoOrigen = dt.Rows[i][1].ToString();
                cmbOrigen.SelectedValue = codigoOrigen;

                // Seleccionar destino
                string codigoDestino = dt.Rows[i][2].ToString();
                cmbDestino.SelectedValue = codigoDestino;

                dtpFechaSalida.Value = Convert.ToDateTime(dt.Rows[i][3]);
                dtpFechaLlegada.Value = Convert.ToDateTime(dt.Rows[i][4]);
                txtAsientosTotales.Text = dt.Rows[i][5].ToString();
                txtAsientosDisponibles.Text = dt.Rows[i][6].ToString();
                txtPrecioBase.Text = dt.Rows[i][7].ToString();
            }
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

        // Botones de acción CRUD
        private void btnIngreso_Click(object sender, EventArgs e)
        {
            boton = 1;
            limpiar();
            habilita();
            btnGuardar.Visible = true;
            txtNumeroVuelo.Focus();
        }

        private void btnModificiacion_Click(object sender, EventArgs e)
        {
            if (dt.Rows.Count > 0)
            {
                boton = 2;
                habilita();
                txtNumeroVuelo.Enabled = false; 
                btnGuardar.Visible = true;
                cmbOrigen.Focus();
            }
            else
            {
                MessageBox.Show("No hay vuelos registrados para modificar");
            }
        }

        private void btnConsulta_Click(object sender, EventArgs e)
        {
            boton = 3;
            limpiar();
            deshabilitar();
            txtNumeroVuelo.Enabled = true;
            txtNumeroVuelo.Clear();
            txtNumeroVuelo.Focus();
        }

        private void btnRetiro_Click(object sender, EventArgs e)
        {
            if (dt.Rows.Count > 0)
            {
                DialogResult resultado = MessageBox.Show(
                    "¿Está seguro de eliminar el vuelo " + txtNumeroVuelo.Text + "?",
                    "Confirmar eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (resultado == DialogResult.Yes)
                {
                    try
                    {
                        cn = new cConexion();
                        cmd = new SqlCommand("delete from tblVuelo where numeroVuelo=@numero", cn.AbrirConexion());
                        cmd.Parameters.AddWithValue("@numero", txtNumeroVuelo.Text);
                        cmd.ExecuteNonQuery();
                        cn.CerrarConexion();

                        MessageBox.Show("Vuelo eliminado correctamente");
                        CargarVuelos();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al eliminar: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("No hay vuelos para eliminar");
            }
        }

        private void txtNumeroVuelo_Leave(object sender, EventArgs e)
        {
            // SOLO buscar cuando esté en modo CONSULTA
            if (boton != 3)
                return;

            if (string.IsNullOrWhiteSpace(txtNumeroVuelo.Text))
                return;

            try
            {
                cn = new cConexion();
                cmd = new SqlCommand("select * from tblVuelo where numeroVuelo = @numero", cn.AbrirConexion());
                cmd.Parameters.AddWithValue("@numero", txtNumeroVuelo.Text.Trim());

                da = new SqlDataAdapter(cmd);
                dtBusqueda = new DataTable();
                da.Fill(dtBusqueda);

                cn.CerrarConexion();

                if (dtBusqueda.Rows.Count > 0)
                {
                    llenar(dtBusqueda, 0);
                }
                else
                {
                    MessageBox.Show("Vuelo no encontrado");
                    limpiar();
                    txtNumeroVuelo.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar: " + ex.Message);
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(txtNumeroVuelo.Text))
            {
                MessageBox.Show("Debe ingresar el número de vuelo");
                txtNumeroVuelo.Focus();
                return;
            }

            if (cmbOrigen.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar el origen");
                cmbOrigen.Focus();
                return;
            }

            if (cmbDestino.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar el destino");
                cmbDestino.Focus();
                return;
            }

            if (cmbOrigen.SelectedValue.ToString() == cmbDestino.SelectedValue.ToString())
            {
                MessageBox.Show("El origen y destino no pueden ser iguales");
                cmbDestino.Focus();
                return;
            }

            if (dtpFechaLlegada.Value <= dtpFechaSalida.Value)
            {
                MessageBox.Show("La fecha de llegada debe ser posterior a la fecha de salida");
                dtpFechaLlegada.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAsientosTotales.Text))
            {
                MessageBox.Show("Debe ingresar la cantidad de asientos totales");
                txtAsientosTotales.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPrecioBase.Text))
            {
                MessageBox.Show("Debe ingresar el precio base");
                txtPrecioBase.Focus();
                return;
            }

            try
            {
                cn = new cConexion();

                switch (boton)
                {
                    case 1: // INGRESO
                        // Verificar si el vuelo ya existe
                        cmd = new SqlCommand("select * from tblVuelo where numeroVuelo=@numero", cn.AbrirConexion());
                        cmd.Parameters.AddWithValue("@numero", txtNumeroVuelo.Text);
                        da = new SqlDataAdapter(cmd);
                        dtBusqueda = new DataTable();
                        da.Fill(dtBusqueda);

                        if (dtBusqueda.Rows.Count > 0)
                        {
                            MessageBox.Show("El número de vuelo ya existe");
                            txtNumeroVuelo.Focus();
                            return;
                        }

                        // Insertar nuevo vuelo
                        cmd = new SqlCommand(@"insert into tblVuelo 
                                             (numeroVuelo, origen, destino, fechaSalida, fechaLlegada, 
                                              asientosTotales, asientosDisponibles, precioBase) 
                                             values 
                                             (@numero, @origen, @destino, @salida, @llegada, 
                                              @totales, @disponibles, @precio)", cn.AbrirConexion());

                        cmd.Parameters.AddWithValue("@numero", txtNumeroVuelo.Text);
                        cmd.Parameters.AddWithValue("@origen", cmbOrigen.SelectedValue.ToString());
                        cmd.Parameters.AddWithValue("@destino", cmbDestino.SelectedValue.ToString());
                        cmd.Parameters.AddWithValue("@salida", dtpFechaSalida.Value);
                        cmd.Parameters.AddWithValue("@llegada", dtpFechaLlegada.Value);
                        cmd.Parameters.AddWithValue("@totales", Convert.ToInt32(txtAsientosTotales.Text));
                        cmd.Parameters.AddWithValue("@disponibles", Convert.ToInt32(txtAsientosDisponibles.Text));
                        cmd.Parameters.AddWithValue("@precio", Convert.ToDecimal(txtPrecioBase.Text.Replace(",", ".")));

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Vuelo registrado correctamente");
                        break;

                    case 2: // MODIFICACIÓN
                        cmd = new SqlCommand(@"update tblVuelo set 
                                             origen=@origen, destino=@destino, 
                                             fechaSalida=@salida, fechaLlegada=@llegada,
                                             asientosTotales=@totales, asientosDisponibles=@disponibles, 
                                             precioBase=@precio 
                                             where numeroVuelo=@numero", cn.AbrirConexion());

                        cmd.Parameters.AddWithValue("@numero", txtNumeroVuelo.Text);
                        cmd.Parameters.AddWithValue("@origen", cmbOrigen.SelectedValue.ToString());
                        cmd.Parameters.AddWithValue("@destino", cmbDestino.SelectedValue.ToString());
                        cmd.Parameters.AddWithValue("@salida", dtpFechaSalida.Value);
                        cmd.Parameters.AddWithValue("@llegada", dtpFechaLlegada.Value);
                        cmd.Parameters.AddWithValue("@totales", Convert.ToInt32(txtAsientosTotales.Text));
                        cmd.Parameters.AddWithValue("@disponibles", Convert.ToInt32(txtAsientosDisponibles.Text));
                        cmd.Parameters.AddWithValue("@precio", Convert.ToDecimal(txtPrecioBase.Text.Replace(",", ".")));

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Vuelo modificado correctamente");
                        break;

                    case 3: // CONSULTA
                        cmd = new SqlCommand("select * from tblVuelo where numeroVuelo=@numero", cn.AbrirConexion());
                        cmd.Parameters.AddWithValue("@numero", txtNumeroVuelo.Text);
                        da = new SqlDataAdapter(cmd);
                        dtBusqueda = new DataTable();
                        da.Fill(dtBusqueda);

                        if (dtBusqueda.Rows.Count > 0)
                        {
                            llenar(dtBusqueda, 0);
                        }
                        else
                        {
                            MessageBox.Show("Vuelo no encontrado");
                            txtNumeroVuelo.Clear();
                            txtNumeroVuelo.Focus();
                            return;
                        }
                        break;
                }

                cn.CerrarConexion();
                CargarVuelos();
                deshabilitar();
                btnGuardar.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        // Autocompletar asientos disponibles
        private void txtAsientosTotales_Leave(object sender, EventArgs e)
        {
            if (boton == 1 && !string.IsNullOrWhiteSpace(txtAsientosTotales.Text))
            {
                txtAsientosDisponibles.Text = txtAsientosTotales.Text;
            }
        }

        private void frmVuelo_Load(object sender, EventArgs e)
        {
            deshabilitar();
            btnGuardar.Visible = false;
        }
    }
}
