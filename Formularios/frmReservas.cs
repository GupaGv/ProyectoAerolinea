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
    public partial class frmReservas : Form
    {
        cConexion cn;
        SqlCommand cmd;
        SqlDataAdapter da;
        DataTable dt, dtBusqueda;
        int i, contador, boton;

        public frmReservas()
        {
            InitializeComponent();
            cn = new cConexion();
            boton = 0;
            ConfigurarControles();
            Deshabilitar();
        }

        void CargarCatalogos()
        {
            CargarClientes();
            CargarVuelos();
        }

        void CargarClientes()
        {
            try
            {
                cmd = new SqlCommand(@"SELECT cedula,cedula + ' - ' + nombre + ' ' + apellido AS cliente FROM tblCliente ORDER BY nombre", cn.AbrirConexion());
                da = new SqlDataAdapter(cmd);
                DataTable dtClientes = new DataTable();
                da.Fill(dtClientes);

                cmbCliente.DataSource = dtClientes;
                cmbCliente.DisplayMember = "cliente";
                cmbCliente.ValueMember = "cedula";
                cmbCliente.SelectedIndex = -1;

                cn.CerrarConexion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar clientes: " + ex.Message);
            }
        }

        void CargarVuelos()
        {
            try
            {
                cmd = new SqlCommand(@"
            SELECT 
                v.numeroVuelo,

                v.numeroVuelo + ' - ' + 
                d1.ciudad + ' → ' + 
                d2.ciudad + ' (' + 
                CONVERT(VARCHAR, v.fechaSalida, 103) + ')' 
                AS descripcion,

                v.origen,
                v.destino,

                d1.ciudad AS ciudadOrigen,
                d2.ciudad AS ciudadDestino,

                v.fechaSalida,
                v.fechaLlegada,
                v.precioBase,
                v.asientosDisponibles

            FROM tblVuelo v

            INNER JOIN tblDestino d1 
                ON v.origen = d1.codigoAeropuerto

            INNER JOIN tblDestino d2 
                ON v.destino = d2.codigoAeropuerto

            WHERE v.asientosDisponibles > 0
            AND v.fechaSalida >= GETDATE()

            ORDER BY v.fechaSalida",

                    cn.AbrirConexion());

                da = new SqlDataAdapter(cmd);

                DataTable dtVuelos = new DataTable();

                da.Fill(dtVuelos);

                // =========================
                // VUELO IDA
                // =========================

                cmbVueloIda.DataSource = dtVuelos.Copy();
                cmbVueloIda.DisplayMember = "descripcion";
                cmbVueloIda.ValueMember = "numeroVuelo";
                cmbVueloIda.SelectedIndex = -1;

                // =========================
                // VUELO VUELTA
                // =========================

                cmbVueloVuelta.DataSource = dtVuelos.Copy();
                cmbVueloVuelta.DisplayMember = "descripcion";
                cmbVueloVuelta.ValueMember = "numeroVuelo";
                cmbVueloVuelta.SelectedIndex = -1;
                cmbVueloVuelta.Enabled = false;

                cn.CerrarConexion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar vuelos: " + ex.Message);
            }
        }

        void CargarReservas()
        {
            try
            {
                i = 0;
                boton = 0;
                cn = new cConexion();
                cmd = new SqlCommand(@"
                    SELECT 
                        r.idReserva,
                        r.codigoReserva,
                        r.cedula,
                        r.numeroVuelo,
                        r.numeroVueloVuelta,
                        r.idaVuelta,
                        r.cantidadPasajeros,
                        r.tipoTiquete,
                        r.equipaje,
                        r.precioTotal,
                        r.estado,
                        r.fechaReserva
                    FROM tblReserva r
                    ORDER BY r.fechaReserva DESC",
                    cn.AbrirConexion());

                da = new SqlDataAdapter(cmd);
                dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                    Llenar(dt, i);

                cn.CerrarConexion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar reservas: " + ex.Message);
            }
        }

        #region Métodos de Control de Interfaz

        void ConfigurarControles()
        {
            // Configurar NumericUpDown de pasajeros
            numPasajeros.Minimum = 1;
            numPasajeros.Maximum = 10;
            numPasajeros.Value = 1;

            // Configurar ComboBox de estado
            cmbEstado.Items.Clear();
            cmbEstado.Items.Add("Pendiente");
            cmbEstado.Items.Add("Confirmada");
            cmbEstado.Items.Add("Cancelada");
            cmbEstado.SelectedIndex = 0;

            // Configurar RadioButtons de tipo de tiquete
            rbEconomico.Checked = true;

            // Ocultar botón guardar
            btnGuardar.Visible = false;

            Deshabilitar();
        }

        void Habilitar()
        {
            cmbCliente.Enabled = true;
            cmbVueloIda.Enabled = true;
            chkIdaVuelta.Enabled = true;
            numPasajeros.Enabled = true;
            rbEconomico.Enabled = true;
            rbEjecutivo.Enabled = true;
            rbPrimera.Enabled = true;
            chkEquipaje.Enabled = true;
            cmbEstado.Enabled = true;
        }

        void Deshabilitar()
        {
            cmbCliente.Enabled = false;
            cmbVueloIda.Enabled = false;
            cmbVueloVuelta.Enabled = false;
            chkIdaVuelta.Enabled = false;
            numPasajeros.Enabled = false;
            rbEconomico.Enabled = false;
            rbEjecutivo.Enabled = false;
            rbPrimera.Enabled = false;
            chkEquipaje.Enabled = false;
            cmbEstado.Enabled = false;

            // Limpiar campos readonly
            txtNombreCliente.Clear();
            txtEmailCliente.Clear();
            txtOrigen.Clear();
            txtDestino.Clear();
            txtFechaSalida.Clear();
            txtPrecioBase.Clear();
            lblDesglosePrecioBase.Text = "$0";
            lblDesgloseTipo.Text = "";
            lblDesglosePasajeros.Text = "";
            lblDesgloseEquipaje.Text = "";
            lblPrecioTotal.Text = "$0";
        }

        void Limpiar()
        {
            txtCodigoReserva.Clear();
            cmbCliente.SelectedIndex = -1;
            cmbVueloIda.SelectedIndex = -1;
            cmbVueloVuelta.SelectedIndex = -1;
            chkIdaVuelta.Checked = false;
            numPasajeros.Value = 1;
            rbEconomico.Checked = true;
            chkEquipaje.Checked = false;
            cmbEstado.SelectedIndex = 0;

            txtNombreCliente.Clear();
            txtEmailCliente.Clear();
            txtOrigen.Clear();
            txtDestino.Clear();
            txtFechaSalida.Clear();
            txtPrecioBase.Clear();
            lblPrecioTotal.Text = "$0";
        }

        void Llenar(DataTable tabla, int indice)
        {
            contador = tabla.Rows.Count;

            if (contador > 0)
            {
                // =========================
                // DATOS BÁSICOS RESERVA
                // =========================

                txtCodigoReserva.Text =
                    tabla.Rows[indice]["codigoReserva"].ToString();

                cmbCliente.SelectedValue =
                    tabla.Rows[indice]["cedula"].ToString();

                cmbVueloIda.SelectedValue =
                    tabla.Rows[indice]["numeroVuelo"].ToString();

                numPasajeros.Value =
                    Convert.ToInt32(tabla.Rows[indice]["cantidadPasajeros"]);

                // =========================
                // TIPO TIQUETE
                // =========================

                string tipoTiquete =
                    tabla.Rows[indice]["tipoTiquete"].ToString();

                rbEconomico.Checked = false;
                rbEjecutivo.Checked = false;
                rbPrimera.Checked = false;

                if (tipoTiquete == "Económico")
                    rbEconomico.Checked = true;

                else if (tipoTiquete == "Ejecutivo")
                    rbEjecutivo.Checked = true;

                else if (tipoTiquete == "Primera")
                    rbPrimera.Checked = true;

                // =========================
                // EQUIPAJE
                // =========================

                chkEquipaje.Checked =
                    Convert.ToBoolean(tabla.Rows[indice]["equipaje"]);

                // =========================
                // ESTADO
                // =========================

                cmbEstado.Text =
                    tabla.Rows[indice]["estado"].ToString();

                // =========================
                // PRECIO TOTAL
                // =========================

                decimal precioTotal =
                    Convert.ToDecimal(tabla.Rows[indice]["precioTotal"]);

                lblPrecioTotal.Text =
                    "$" + precioTotal.ToString("N0");

                // =========================
                // CARGAR CLIENTE
                // =========================

                try
                {
                    cmd = new SqlCommand(
                        "SELECT nombre, apellido, email FROM tblCliente WHERE cedula=@cedula",
                        cn.AbrirConexion());

                    cmd.Parameters.AddWithValue(
                        "@cedula",
                        tabla.Rows[indice]["cedula"].ToString());

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        txtNombreCliente.Text =
                            reader["nombre"].ToString() + " " +
                            reader["apellido"].ToString();

                        txtEmailCliente.Text =
                            reader["email"].ToString();
                    }

                    reader.Close();
                    cn.CerrarConexion();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                // =========================
                // CARGAR DATOS VUELO
                // =========================

                try
                {
                    DataRowView rowVuelo =
                        (DataRowView)cmbVueloIda.SelectedItem;

                    if (rowVuelo != null)
                    {
                        txtOrigen.Text =
                            rowVuelo["ciudadOrigen"].ToString();

                        txtDestino.Text =
                            rowVuelo["ciudadDestino"].ToString();

                        txtFechaSalida.Text =
                            Convert.ToDateTime(rowVuelo["fechaSalida"])
                            .ToString("dd/MM/yyyy HH:mm");

                        txtPrecioBase.Text =
                            "$" +
                            Convert.ToDecimal(rowVuelo["precioBase"])
                            .ToString("N0");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                bool idaVuelta = false;

                if (tabla.Rows[indice]["idaVuelta"] != DBNull.Value)
                {
                    idaVuelta =
                        Convert.ToBoolean(
                            tabla.Rows[indice]["idaVuelta"]);
                }

                chkIdaVuelta.Checked = idaVuelta;

                if (idaVuelta)
                {
                    cmbVueloVuelta.Enabled = true;

                    if (tabla.Rows[indice]["numeroVueloVuelta"] != DBNull.Value)
                    {
                        string vueloVuelta =
                            tabla.Rows[indice]["numeroVueloVuelta"]
                            .ToString();

                        cmbVueloVuelta.SelectedValue =
                            vueloVuelta;
                    }
                }
                else
                {
                    cmbVueloVuelta.Enabled = false;
                    cmbVueloVuelta.SelectedIndex = -1;
                }
            }
            CalcularPrecioTotal();
        }

        void CalcularPrecioTotal()
        {
            try
            {
                // Validar selección
                if (cmbVueloIda.SelectedIndex == -1 ||
                    cmbVueloIda.SelectedItem == null)
                {
                    MessageBox.Show("Debe seleccionar un vuelo");
                    return;
                }

                DataRowView rowIda =
                    cmbVueloIda.SelectedItem as DataRowView;

                // Validar fila
                if (rowIda == null)
                {
                    MessageBox.Show("Error al obtener datos del vuelo");
                    return;
                }

                decimal precioBase =
                    Convert.ToDecimal(rowIda["precioBase"]);

                // =========================
                // IDA Y VUELTA
                // =========================

                if (chkIdaVuelta.Checked)
                {
                    if (cmbVueloVuelta.SelectedIndex == -1 ||
                        cmbVueloVuelta.SelectedItem == null)
                    {
                        MessageBox.Show("Seleccione el vuelo de vuelta");
                        return;
                    }

                    DataRowView rowVuelta =
                        cmbVueloVuelta.SelectedItem as DataRowView;

                    if (rowVuelta == null)
                    {
                        MessageBox.Show("Error en vuelo de vuelta");
                        return;
                    }

                    precioBase +=
                        Convert.ToDecimal(rowVuelta["precioBase"]);
                }

                // =========================
                // TIPO
                // =========================

                decimal multiplicador = 1.0m;
                string tipoTexto = "";

                if (rbEconomico.Checked)
                {
                    multiplicador = 1.0m;
                    tipoTexto = "Económico";
                }
                else if (rbEjecutivo.Checked)
                {
                    multiplicador = 1.5m;
                    tipoTexto = "Ejecutivo";
                }
                else if (rbPrimera.Checked)
                {
                    multiplicador = 2.0m;
                    tipoTexto = "Primera Clase";
                }

                decimal subtotal =
                    precioBase * multiplicador;

                decimal totalPasajeros =
                    subtotal * numPasajeros.Value;

                decimal totalEquipaje = 0;

                if (chkEquipaje.Checked)
                {
                    totalEquipaje =
                        50000 * numPasajeros.Value;
                }

                decimal total =
                    totalPasajeros + totalEquipaje;

                // =========================
                // RESUMEN
                // =========================

                lblDesglosePrecioBase.Text =
                    "Precio Base: $" +
                    precioBase.ToString("N0");

                lblDesgloseTipo.Text =
                    "Tipo (" + tipoTexto + "): $" +
                    subtotal.ToString("N0");

                lblDesglosePasajeros.Text =
                    "Pasajeros (" + numPasajeros.Value + "): $" +
                    totalPasajeros.ToString("N0");

                lblDesgloseEquipaje.Text =
                    "Equipaje: $" +
                    totalEquipaje.ToString("N0");

                lblPrecioTotal.Text =
                    "$" + total.ToString("N0");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al calcular total:\n\n" +
                    ex.Message);
            }
        }

        // Eventos 

        private void cmbCliente_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCliente.SelectedIndex != -1)
            {
                try
                {
                    string cedula = cmbCliente.SelectedValue.ToString();
                    cmd = new SqlCommand("SELECT nombre, apellido, email FROM tblCliente WHERE cedula=@cedula", cn.AbrirConexion());
                    cmd.Parameters.AddWithValue("@cedula", cedula);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        txtNombreCliente.Text = reader["nombre"].ToString() + " " + reader["apellido"].ToString();
                        txtEmailCliente.Text = reader["email"].ToString();
                    }
                    reader.Close();
                    cn.CerrarConexion();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar datos del cliente: " + ex.Message);
                }
            }
        }

        private void cmbVueloIda_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbVueloIda.SelectedIndex != -1)
            {
                try
                {
                    DataRowView row = (DataRowView)cmbVueloIda.SelectedItem;

                    txtOrigen.Text = row["ciudadOrigen"].ToString();
                    txtDestino.Text = row["ciudadDestino"].ToString();
                    txtFechaSalida.Text = Convert.ToDateTime(row["fechaSalida"]).ToString("dd/MM/yyyy HH:mm");
                    txtPrecioBase.Text = "$" + Convert.ToDecimal(row["precioBase"]).ToString("N0");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar datos del vuelo: " + ex.Message);
                }
            }
        }

        private void chkIdaVuelta_CheckedChanged(object sender, EventArgs e)
        {
            cmbVueloVuelta.Enabled = chkIdaVuelta.Checked;

            if (!chkIdaVuelta.Checked)
            {
                cmbVueloVuelta.DataSource = null;
                return;
            }

            if (cmbVueloIda.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione primero el vuelo de ida");
                chkIdaVuelta.Checked = false;
                return;
            }

            try
            {
                DataRowView rowIda =
                    (DataRowView)cmbVueloIda.SelectedItem;

                string origenIda =
                    rowIda["origen"].ToString();

                string destinoIda =
                    rowIda["destino"].ToString();

                DateTime fechaIda =
                    Convert.ToDateTime(rowIda["fechaSalida"]);

                // Buscar vuelos inversos
                cmd = new SqlCommand(@"
            SELECT
                v.numeroVuelo,

                v.numeroVuelo + ' - ' +
                d1.ciudad + ' → ' +
                d2.ciudad + ' (' +
                CONVERT(VARCHAR, v.fechaSalida, 103) + ')'
                AS descripcion,

                v.origen,
                v.destino,
                d1.ciudad AS ciudadOrigen,
                d2.ciudad AS ciudadDestino,
                v.fechaSalida,
                v.precioBase,
                v.asientosDisponibles

            FROM tblVuelo v

            INNER JOIN tblDestino d1
                ON v.origen = d1.codigoAeropuerto

            INNER JOIN tblDestino d2
                ON v.destino = d2.codigoAeropuerto

            WHERE
                v.origen = @destinoIda
                AND v.destino = @origenIda
                AND v.fechaSalida > @fechaIda

            ORDER BY v.fechaSalida",

                    cn.AbrirConexion());

                cmd.Parameters.AddWithValue("@destinoIda", destinoIda);
                cmd.Parameters.AddWithValue("@origenIda", origenIda);
                cmd.Parameters.AddWithValue("@fechaIda", fechaIda);

                da = new SqlDataAdapter(cmd);

                DataTable dtVuelta = new DataTable();

                da.Fill(dtVuelta);

                cmbVueloVuelta.DataSource = dtVuelta;
                cmbVueloVuelta.DisplayMember = "descripcion";
                cmbVueloVuelta.ValueMember = "numeroVuelo";

                cn.CerrarConexion();

                if (dtVuelta.Rows.Count == 0)
                {
                    MessageBox.Show(
                        "No hay vuelos de regreso disponibles");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar vuelos de vuelta:\n" +
                    ex.Message);
            }
        }


        string GenerarCodigoReserva()
        {
            Random rnd = new Random();
            return "R" + DateTime.Now.ToString("yyyyMMdd") + rnd.Next(100, 999).ToString();
        }

        // Botones CRUD
        private void btnIngreso_Click(object sender, EventArgs e)
        {
            boton = 1;
            CargarCatalogos();
            Limpiar();
            Habilitar();
            txtCodigoReserva.Text = GenerarCodigoReserva();
            btnGuardar.Visible = true;
            cmbCliente.Focus();
        }

        private void btnModificiacion_Click(object sender, EventArgs e)
        {
            boton = 2;
            CargarCatalogos();
            Limpiar();
            Habilitar();
            txtCodigoReserva.Enabled = true;
            txtCodigoReserva.Focus();
            btnGuardar.Visible = true;
            MessageBox.Show("Ingrese el código de reserva y presione Enter");
        }

        private void btnConsulta_Click(object sender, EventArgs e)
        {
            boton = 3;
            Limpiar();
            Deshabilitar();
            CargarCatalogos();
            txtCodigoReserva.Enabled = true;
            txtCodigoReserva.Focus();
            MessageBox.Show("Ingrese el código de reserva y presione Enter");
        }

        private void btnRetiro_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCodigoReserva.Text))
            {
                MessageBox.Show("No hay reserva seleccionada");
                return;
            }

            DialogResult resultado = MessageBox.Show(
                "¿Está seguro de eliminar la reserva " +
                txtCodigoReserva.Text + "?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                try
                {
                    cn = new cConexion();

                    // =========================
                    // OBTENER DATOS RESERVA
                    // =========================

                    cmd = new SqlCommand(@"
                SELECT
                    numeroVuelo,
                    numeroVueloVuelta,
                    cantidadPasajeros,
                    idaVuelta
                FROM tblReserva
                WHERE codigoReserva=@codigo",
                        cn.AbrirConexion());

                    cmd.Parameters.AddWithValue(
                        "@codigo",
                        txtCodigoReserva.Text);

                    SqlDataReader reader =
                        cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string vueloIda =
                            reader["numeroVuelo"].ToString();

                        int pasajeros =
                            Convert.ToInt32(
                                reader["cantidadPasajeros"]);

                        bool idaVuelta = false;

                        if (reader["idaVuelta"] != DBNull.Value)
                        {
                            idaVuelta =
                                Convert.ToBoolean(
                                    reader["idaVuelta"]);
                        }

                        string vueloVuelta = "";

                        if (reader["numeroVueloVuelta"] != DBNull.Value)
                        {
                            vueloVuelta =
                                reader["numeroVueloVuelta"]
                                .ToString();
                        }

                        reader.Close();

                        // =========================
                        // DEVOLVER ASIENTOS IDA
                        // =========================

                        cmd = new SqlCommand(@"
                    UPDATE tblVuelo
                    SET asientosDisponibles =
                        asientosDisponibles + @cantidad
                    WHERE numeroVuelo=@vuelo",
                            cn.AbrirConexion());

                        cmd.Parameters.AddWithValue(
                            "@cantidad",
                            pasajeros);

                        cmd.Parameters.AddWithValue(
                            "@vuelo",
                            vueloIda);

                        cmd.ExecuteNonQuery();

                        // =========================
                        // DEVOLVER ASIENTOS VUELTA
                        // =========================

                        if (idaVuelta &&
                            !string.IsNullOrWhiteSpace(vueloVuelta))
                        {
                            cmd = new SqlCommand(@"
                        UPDATE tblVuelo
                        SET asientosDisponibles =
                            asientosDisponibles + @cantidad
                        WHERE numeroVuelo=@vuelo",
                                cn.AbrirConexion());

                            cmd.Parameters.AddWithValue(
                                "@cantidad",
                                pasajeros);

                            cmd.Parameters.AddWithValue(
                                "@vuelo",
                                vueloVuelta);

                            cmd.ExecuteNonQuery();
                        }

                        // =========================
                        // ELIMINAR RESERVA
                        // =========================

                        cmd = new SqlCommand(@"
                    DELETE FROM tblReserva
                    WHERE codigoReserva=@codigo",
                            cn.AbrirConexion());

                        cmd.Parameters.AddWithValue(
                            "@codigo",
                            txtCodigoReserva.Text);

                        cmd.ExecuteNonQuery();

                        cn.CerrarConexion();

                        MessageBox.Show(
                            "Reserva eliminada correctamente");

                        // =========================
                        // RECARGAR DATOS
                        // =========================

                        CargarReservas();
                        CargarVuelos();

                        Limpiar();
                        Deshabilitar();

                        btnGuardar.Visible = false;
                    }
                    else
                    {
                        reader.Close();

                        MessageBox.Show(
                            "No se encontró la reserva");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Error al eliminar reserva:\n" +
                        ex.Message);
                }
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (lblPrecioTotal.Text == "$0")
            {
                MessageBox.Show("Debe calcular el total antes de guardar");
                return;
            }

            if (cmbCliente.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar un cliente");
                cmbCliente.Focus();
                return;
            }

            if (cmbVueloIda.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar un vuelo de ida");
                cmbVueloIda.Focus();
                return;
            }

            if (chkIdaVuelta.Checked && cmbVueloVuelta.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar un vuelo de vuelta");
                cmbVueloVuelta.Focus();
                return;
            }

            if (numPasajeros.Value < 1)
            {
                MessageBox.Show("Debe haber al menos 1 pasajero");
                numPasajeros.Focus();
                return;
            }

            // Verificar disponibilidad de asientos
            DataRowView rowVuelo = (DataRowView)cmbVueloIda.SelectedItem;
            int asientosDisponibles = Convert.ToInt32(rowVuelo["asientosDisponibles"]);

            if (asientosDisponibles < numPasajeros.Value)
            {
                MessageBox.Show($"Solo hay {asientosDisponibles} asientos disponibles en este vuelo");
                numPasajeros.Focus();
                return;
            }

            try
            {
                cn = new cConexion();

                // Obtener datos
                string codigoReserva = txtCodigoReserva.Text;
                string cedula = cmbCliente.SelectedValue.ToString();
                string numeroVuelo = cmbVueloIda.SelectedValue.ToString();
                int cantidadPasajeros = (int)numPasajeros.Value;
                string tipoTiquete = rbEconomico.Checked ? "Económico" :
                                   rbEjecutivo.Checked ? "Ejecutivo" : "Primera";
                bool equipaje = chkEquipaje.Checked;
                decimal precioTotal = Convert.ToDecimal(lblPrecioTotal.Text.Replace("$", "").Replace(",", ""));
                string estado = cmbEstado.Text;

                if (boton == 1) // INGRESO
                {
                    // Insertar reserva
                    cmd = new SqlCommand(@"
                        INSERT INTO tblReserva 
                        (
                        codigoReserva,
                        cedula,
                        numeroVuelo,
                        numeroVueloVuelta,
                        idaVuelta,
                        cantidadPasajeros,
                        tipoTiquete,
                        equipaje,
                        precioTotal,
                        estado,
                        fechaReserva
                    )
                    VALUES
                    (
                        @codigo,
                        @cedula,
                        @vuelo,
                        @vueloVuelta,
                        @idaVuelta,
                        @pasajeros,
                        @tipo,
                        @equipaje,
                        @precio,
                        @estado,
                        @fecha
                    )", cn.AbrirConexion());

                    cmd.Parameters.AddWithValue("@codigo", codigoReserva);
                    cmd.Parameters.AddWithValue("@cedula", cedula);
                    cmd.Parameters.AddWithValue("@vuelo", numeroVuelo);
                    cmd.Parameters.AddWithValue("@pasajeros", cantidadPasajeros);
                    cmd.Parameters.AddWithValue("@tipo", tipoTiquete);
                    cmd.Parameters.AddWithValue("@equipaje", equipaje);
                    cmd.Parameters.AddWithValue("@precio", precioTotal);
                    cmd.Parameters.AddWithValue("@estado", estado);
                    cmd.Parameters.AddWithValue("@fecha", DateTime.Now);

                    string vueloVuelta = null;

                    if (chkIdaVuelta.Checked &&
                        cmbVueloVuelta.SelectedIndex != -1)
                    {
                        vueloVuelta =
                            cmbVueloVuelta.SelectedValue.ToString();
                    }

                    cmd.Parameters.AddWithValue(
                        "@vueloVuelta",
                        (object)vueloVuelta ?? DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@idaVuelta",
                        chkIdaVuelta.Checked);

                    cmd.ExecuteNonQuery();

                    // Actualizar asientos disponibles
                    cmd = new SqlCommand("UPDATE tblVuelo SET asientosDisponibles = asientosDisponibles - @cantidad WHERE numeroVuelo=@numero", cn.AbrirConexion());
                    cmd.Parameters.AddWithValue("@cantidad", cantidadPasajeros);
                    cmd.Parameters.AddWithValue("@numero", numeroVuelo);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Reserva registrada correctamente\nCódigo: " + codigoReserva);
                }
                else if (boton == 2) // MODIFICACIÓN
                {
                    // Primero devolver los asientos del vuelo anterior
                    cmd = new SqlCommand("SELECT numeroVuelo, cantidadPasajeros FROM tblReserva WHERE codigoReserva=@codigo", cn.AbrirConexion());
                    cmd.Parameters.AddWithValue("@codigo", codigoReserva);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string vueloAnterior = reader["numeroVuelo"].ToString();
                        int pasajerosAnterior = Convert.ToInt32(reader["cantidadPasajeros"]);
                        reader.Close();

                        // Devolver asientos al vuelo anterior
                        cmd = new SqlCommand("UPDATE tblVuelo SET asientosDisponibles = asientosDisponibles + @cantidad WHERE numeroVuelo=@numero", cn.AbrirConexion());
                        cmd.Parameters.AddWithValue("@cantidad", pasajerosAnterior);
                        cmd.Parameters.AddWithValue("@numero", vueloAnterior);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        reader.Close();
                    }

                    // Actualizar reserva
                    cmd = new SqlCommand(@"
                        UPDATE tblReserva SET
                        cedula=@cedula,
                        numeroVuelo=@vuelo,
                        numeroVueloVuelta=@vueloVuelta,
                        idaVuelta=@idaVuelta,
                        cantidadPasajeros=@pasajeros,
                        tipoTiquete=@tipo,
                        equipaje=@equipaje,
                        precioTotal=@precio,
                        estado=@estado
                        WHERE codigoReserva=@codigo", cn.AbrirConexion());

                    cmd.Parameters.AddWithValue("@codigo", codigoReserva);
                    cmd.Parameters.AddWithValue("@cedula", cedula);
                    cmd.Parameters.AddWithValue("@vuelo", numeroVuelo);
                    cmd.Parameters.AddWithValue("@pasajeros", cantidadPasajeros);
                    cmd.Parameters.AddWithValue("@tipo", tipoTiquete);
                    cmd.Parameters.AddWithValue("@equipaje", equipaje);
                    cmd.Parameters.AddWithValue("@precio", precioTotal);
                    cmd.Parameters.AddWithValue("@estado", estado);
                    string vueloVuelta = null;

                    if (chkIdaVuelta.Checked &&
                        cmbVueloVuelta.SelectedIndex != -1)
                    {
                        vueloVuelta =
                            cmbVueloVuelta.SelectedValue.ToString();
                    }

                    cmd.Parameters.AddWithValue(
                        "@vueloVuelta",
                        (object)vueloVuelta ?? DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@idaVuelta",
                        chkIdaVuelta.Checked);

                    cmd.ExecuteNonQuery();

                    // Restar asientos del nuevo vuelo
                    cmd = new SqlCommand("UPDATE tblVuelo SET asientosDisponibles = asientosDisponibles - @cantidad WHERE numeroVuelo=@numero", cn.AbrirConexion());
                    cmd.Parameters.AddWithValue("@cantidad", cantidadPasajeros);
                    cmd.Parameters.AddWithValue("@numero", numeroVuelo);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Reserva modificada correctamente");
                }

                cn.CerrarConexion();
                CargarReservas();
                CargarVuelos(); // Recargar vuelos para actualizar disponibilidad
                btnGuardar.Visible = false;
                Deshabilitar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar reserva: " + ex.Message);
            }
        }

        private void cmbVueloVuelta_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void btnCalcular_Click(object sender, EventArgs e)
        {
            CalcularPrecioTotal();
        }

        private void frmReservas_Load(object sender, EventArgs e)
        {
            Deshabilitar();
        }

        private void txtCodigoReserva_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && (boton == 2 || boton == 3))
            {
                try
                {
                    cmd = new SqlCommand(
                        "SELECT * FROM tblReserva WHERE codigoReserva=@codigo",
                        cn.AbrirConexion());

                    cmd.Parameters.AddWithValue("@codigo", txtCodigoReserva.Text);

                    da = new SqlDataAdapter(cmd);
                    dtBusqueda = new DataTable();
                    da.Fill(dtBusqueda);

                    cn.CerrarConexion();

                    if (dtBusqueda.Rows.Count > 0)
                    {
                        Llenar(dtBusqueda, 0);
                    }
                    else
                    {
                        MessageBox.Show("Reserva no encontrada");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error en la consulta: " + ex.Message);
                }
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            Limpiar();
            Deshabilitar();
            btnGuardar.Visible = false;
        }
        #endregion
    }
}