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
using System.Net;
using System.Net.Mail;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace ProyectoAerolinea.Formularios
{
    public partial class frmReservas : Form
    {
        cConexion cn;
        SqlCommand cmd;
        SqlDataAdapter da;
        DataTable dt, dtBusqueda;
        int i, contador, boton;
        private bool pagoCompletado = false;
        private string metodoPagoSeleccionado = "";
        private string cedulaClienteActual = "";

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
            CargarVuelos();
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
            txtCedula.Enabled = true;
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
            txtCedula.Enabled = false;
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
            txtCedula.Clear();
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

            // Resetear estado del pago
            pagoCompletado = false;
            metodoPagoSeleccionado = "";
            btnPagar.Text = "💳 Pagar";
            btnPagar.BackColor = System.Drawing.Color.FromArgb(0, 114, 198);
            btnPagar.ForeColor = System.Drawing.Color.White;
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

                txtCedula.Text = tabla.Rows[indice]["cedula"].ToString();
                    cedulaClienteActual = txtCedula.Text;

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



        void BuscarClientePorCedula(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
            {
                MessageBox.Show("Ingrese una cédula para buscar.");
                return;
            }

            try
            {
                cn = new cConexion();
                cmd = new SqlCommand(@"
            SELECT nombre, apellido, email 
            FROM tblCliente 
            WHERE cedula = @cedula",
                    cn.AbrirConexion());

                cmd.Parameters.AddWithValue("@cedula", cedula);
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    cedulaClienteActual = cedula;
                    txtNombreCliente.Text = dr["nombre"].ToString() + " " + dr["apellido"].ToString();
                    txtEmailCliente.Text = dr["email"].ToString();
                    dr.Close();
                    cn.CerrarConexion();
                }
                else
                {
                    dr.Close();
                    cn.CerrarConexion();
                    cedulaClienteActual = "";
                    txtNombreCliente.Clear();
                    txtEmailCliente.Clear();
                    MessageBox.Show("No se encontró ningún cliente con la cédula: " + cedula,
                                    "Cliente no encontrado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCedula.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar cliente:\n" + ex.Message);
            }
        }

        byte[] GenerarPDFReservaIndividual(string codigoReserva)
        {
            try
            {
                // Consultar datos completos de esta reserva
                cn = new cConexion();
                cmd = new SqlCommand(@"
            SELECT 
                r.codigoReserva,
                r.fechaReserva,
                r.cantidadPasajeros,
                r.tipoTiquete,
                r.equipaje,
                r.precioTotal,
                r.estado,
                r.idaVuelta,
                c.nombre + ' ' + c.apellido AS cliente,
                c.cedula,
                c.email,
                c.telefono,
                v.numeroVuelo,
                v.fechaSalida,
                v.fechaLlegada,
                v.precioBase,
                d1.ciudad AS origen,
                d1.pais AS paisOrigen,
                d2.ciudad AS destino,
                d2.pais AS paisDestino
            FROM tblReserva r
            INNER JOIN tblCliente c ON r.cedula = c.cedula
            INNER JOIN tblVuelo v ON r.numeroVuelo = v.numeroVuelo
            INNER JOIN tblDestino d1 ON v.origen = d1.codigoAeropuerto
            INNER JOIN tblDestino d2 ON v.destino = d2.codigoAeropuerto
            WHERE r.codigoReserva = @codigo",
                    cn.AbrirConexion());

                cmd.Parameters.AddWithValue("@codigo", codigoReserva);
                da = new SqlDataAdapter(cmd);
                DataTable dtR = new DataTable();
                da.Fill(dtR);
                cn.CerrarConexion();

                if (dtR.Rows.Count == 0) return null;

                DataRow r2 = dtR.Rows[0];

                // Generar PDF en memoria (byte[]) para poder adjuntarlo al correo
                using (MemoryStream ms = new MemoryStream())
                {
                    iTextSharp.text.Document doc = new iTextSharp.text.Document(
                        iTextSharp.text.PageSize.A4, 40, 40, 40, 40);

                    iTextSharp.text.pdf.PdfWriter.GetInstance(doc, ms);
                    doc.Open();

                    // ── Fuentes ──────────────────────────────────────────
                    iTextSharp.text.Font fTitulo = new iTextSharp.text.Font(
                        iTextSharp.text.Font.HELVETICA, 20f, iTextSharp.text.Font.BOLD,
                        new iTextSharp.text.BaseColor(31, 73, 125));

                    iTextSharp.text.Font fSubtitulo = new iTextSharp.text.Font(
                        iTextSharp.text.Font.HELVETICA, 11f, iTextSharp.text.Font.BOLD,
                        new iTextSharp.text.BaseColor(31, 73, 125));

                    iTextSharp.text.Font fLabel = new iTextSharp.text.Font(
                        iTextSharp.text.Font.HELVETICA, 9f, iTextSharp.text.Font.BOLD,
                        iTextSharp.text.BaseColor.White);

                    iTextSharp.text.Font fValor = new iTextSharp.text.Font(
                        iTextSharp.text.Font.HELVETICA, 10f, iTextSharp.text.Font.NORMAL);

                    iTextSharp.text.Font fValorBold = new iTextSharp.text.Font(
                        iTextSharp.text.Font.HELVETICA, 10f, iTextSharp.text.Font.BOLD);

                    iTextSharp.text.Font fPeque = new iTextSharp.text.Font(
                        iTextSharp.text.Font.HELVETICA, 8f, iTextSharp.text.Font.NORMAL,
                        iTextSharp.text.BaseColor.Gray);

                    iTextSharp.text.BaseColor azul = new iTextSharp.text.BaseColor(31, 73, 125);
                    iTextSharp.text.BaseColor azulClaro = new iTextSharp.text.BaseColor(220, 230, 241);

                    // ── Encabezado ────────────────────────────────────────
                    iTextSharp.text.Paragraph encabezado = new iTextSharp.text.Paragraph(
                        "COMPROBANTE DE RESERVA\n", fTitulo);
                    encabezado.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                    doc.Add(encabezado);

                    iTextSharp.text.Font fAerolinea = new iTextSharp.text.Font(
                        iTextSharp.text.Font.HELVETICA, 10f, iTextSharp.text.Font.NORMAL,
                        iTextSharp.text.BaseColor.Gray);
                    iTextSharp.text.Paragraph subEnc = new iTextSharp.text.Paragraph(
                        "Aerolínea - Sistema de Gestión de Reservas\n\n", fAerolinea);
                    subEnc.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                    doc.Add(subEnc);

                    // ── Código y estado en una tabla de 2 columnas ────────
                    iTextSharp.text.pdf.PdfPTable tblCodigo =
                        new iTextSharp.text.pdf.PdfPTable(2);
                    tblCodigo.WidthPercentage = 100;
                    tblCodigo.SetWidths(new int[] { 50, 50 });
                    tblCodigo.SpacingAfter = 12f;

                    // Celda código
                    iTextSharp.text.pdf.PdfPCell celdaCodigo =
                        new iTextSharp.text.pdf.PdfPCell();
                    celdaCodigo.BackgroundColor = azul;
                    celdaCodigo.Padding = 8;
                    celdaCodigo.AddElement(new iTextSharp.text.Paragraph(
                        "CÓDIGO DE RESERVA", fLabel));
                    iTextSharp.text.Font fCodigoValor = new iTextSharp.text.Font(
                        iTextSharp.text.Font.HELVETICA, 16f, iTextSharp.text.Font.BOLD,
                        iTextSharp.text.BaseColor.White);
                    celdaCodigo.AddElement(new iTextSharp.text.Paragraph(
                        r2["codigoReserva"].ToString(), fCodigoValor));
                    tblCodigo.AddCell(celdaCodigo);

                    // Celda estado
                    string estado = r2["estado"].ToString();
                    iTextSharp.text.BaseColor colorEstado =
                        estado == "Confirmada" ? new iTextSharp.text.BaseColor(39, 119, 59) :
                        estado == "Cancelada" ? new iTextSharp.text.BaseColor(180, 30, 30) :
                                                 new iTextSharp.text.BaseColor(180, 130, 0);

                    iTextSharp.text.pdf.PdfPCell celdaEstado =
                        new iTextSharp.text.pdf.PdfPCell();
                    celdaEstado.BackgroundColor = colorEstado;
                    celdaEstado.Padding = 8;
                    celdaEstado.AddElement(new iTextSharp.text.Paragraph(
                        "ESTADO", fLabel));
                    iTextSharp.text.Font fEstadoValor = new iTextSharp.text.Font(
                        iTextSharp.text.Font.HELVETICA, 16f, iTextSharp.text.Font.BOLD,
                        iTextSharp.text.BaseColor.White);
                    celdaEstado.AddElement(new iTextSharp.text.Paragraph(
                        estado.ToUpper(), fEstadoValor));
                    tblCodigo.AddCell(celdaEstado);

                    doc.Add(tblCodigo);

                    // ── Sección: Datos del cliente ────────────────────────
                    doc.Add(new iTextSharp.text.Paragraph("DATOS DEL CLIENTE\n", fSubtitulo));

                    iTextSharp.text.pdf.PdfPTable tblCliente =
                        new iTextSharp.text.pdf.PdfPTable(4);
                    tblCliente.WidthPercentage = 100;
                    tblCliente.SetWidths(new int[] { 20, 30, 20, 30 });
                    tblCliente.SpacingAfter = 12f;

                    AgregarFilaTabla(tblCliente, "Nombre completo",
                        r2["cliente"].ToString(), "Cédula",
                        r2["cedula"].ToString(), azulClaro, fLabel, fValor);

                    AgregarFilaTabla(tblCliente, "Correo electrónico",
                        r2["email"].ToString(), "Fecha de reserva",
                        Convert.ToDateTime(r2["fechaReserva"]).ToString("dd/MM/yyyy HH:mm"),
                        iTextSharp.text.BaseColor.White, fLabel, fValor);

                    doc.Add(tblCliente);

                    // ── Sección: Datos del vuelo ──────────────────────────
                    doc.Add(new iTextSharp.text.Paragraph("DATOS DEL VUELO\n", fSubtitulo));

                    iTextSharp.text.pdf.PdfPTable tblVuelo =
                        new iTextSharp.text.pdf.PdfPTable(4);
                    tblVuelo.WidthPercentage = 100;
                    tblVuelo.SetWidths(new int[] { 20, 30, 20, 30 });
                    tblVuelo.SpacingAfter = 12f;

                    AgregarFilaTabla(tblVuelo, "Número de vuelo",
                        r2["numeroVuelo"].ToString(), "Tipo de viaje",
                        Convert.ToBoolean(r2["idaVuelta"]) ? "Ida y Vuelta" : "Solo ida",
                        azulClaro, fLabel, fValor);

                    AgregarFilaTabla(tblVuelo, "Origen",
                        r2["origen"].ToString(), "Destino",
                        r2["destino"].ToString(),
                        iTextSharp.text.BaseColor.White, fLabel, fValor);

                    AgregarFilaTabla(tblVuelo, "Fecha de salida",
                        Convert.ToDateTime(r2["fechaSalida"]).ToString("dd/MM/yyyy HH:mm"),
                        "Fecha de llegada",
                        Convert.ToDateTime(r2["fechaLlegada"]).ToString("dd/MM/yyyy HH:mm"),
                        azulClaro, fLabel, fValor);

                    doc.Add(tblVuelo);

                    // ── Sección: Detalles de la reserva ───────────────────
                    doc.Add(new iTextSharp.text.Paragraph("DETALLES DE LA RESERVA\n", fSubtitulo));

                    iTextSharp.text.pdf.PdfPTable tblDetalle =
                        new iTextSharp.text.pdf.PdfPTable(4);
                    tblDetalle.WidthPercentage = 100;
                    tblDetalle.SetWidths(new int[] { 20, 30, 20, 30 });
                    tblDetalle.SpacingAfter = 12f;

                    AgregarFilaTabla(tblDetalle, "Pasajeros",
                        r2["cantidadPasajeros"].ToString(), "Tipo de tiquete",
                        r2["tipoTiquete"].ToString(),
                        azulClaro, fLabel, fValor);

                    AgregarFilaTabla(tblDetalle, "Equipaje",
                        Convert.ToBoolean(r2["equipaje"]) ? "Incluido" : "No incluido",
                        "Precio base",
                        "$" + Convert.ToDecimal(r2["precioBase"]).ToString("N0"),
                        iTextSharp.text.BaseColor.White, fLabel, fValor);

                    doc.Add(tblDetalle);

                    // ── Total ─────────────────────────────────────────────
                    iTextSharp.text.pdf.PdfPTable tblTotal =
                        new iTextSharp.text.pdf.PdfPTable(1);
                    tblTotal.WidthPercentage = 40;
                    tblTotal.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    tblTotal.SpacingAfter = 20f;

                    iTextSharp.text.pdf.PdfPCell celdaTotal =
                        new iTextSharp.text.pdf.PdfPCell();
                    celdaTotal.BackgroundColor = azul;
                    celdaTotal.Padding = 10;
                    celdaTotal.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    celdaTotal.AddElement(new iTextSharp.text.Paragraph(
                        "TOTAL A PAGAR", fLabel));
                    iTextSharp.text.Font fTotalValor = new iTextSharp.text.Font(
                        iTextSharp.text.Font.HELVETICA, 18f, iTextSharp.text.Font.BOLD,
                        iTextSharp.text.BaseColor.White);
                    celdaTotal.AddElement(new iTextSharp.text.Paragraph(
                        "$" + Convert.ToDecimal(r2["precioTotal"]).ToString("N0"),
                        fTotalValor));
                    tblTotal.AddCell(celdaTotal);
                    doc.Add(tblTotal);

                    // ── Pie de página ─────────────────────────────────────
                    iTextSharp.text.Paragraph pie = new iTextSharp.text.Paragraph(
                        "Este documento es el comprobante oficial de su reserva. " +
                        "Preséntelo al momento del abordaje.\n" +
                        "Generado el " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                        fPeque);
                    pie.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                    doc.Add(pie);

                    doc.Close();
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar PDF de reserva:\n" + ex.Message);
                return null;
            }
        }

        // Método auxiliar para agregar filas de 4 columnas (label, valor, label, valor)
        void AgregarFilaTabla(
            iTextSharp.text.pdf.PdfPTable tabla,
            string label1, string valor1,
            string label2, string valor2,
            iTextSharp.text.BaseColor colorFondo,
            iTextSharp.text.Font fLabel,
            iTextSharp.text.Font fValor)
        {
            iTextSharp.text.BaseColor azul = new iTextSharp.text.BaseColor(31, 73, 125);

            iTextSharp.text.pdf.PdfPCell c1 = new iTextSharp.text.pdf.PdfPCell(
                new iTextSharp.text.Phrase(label1, fLabel));
            c1.BackgroundColor = azul;
            c1.Padding = 5;

            iTextSharp.text.pdf.PdfPCell c2 = new iTextSharp.text.pdf.PdfPCell(
                new iTextSharp.text.Phrase(valor1, fValor));
            c2.BackgroundColor = colorFondo;
            c2.Padding = 5;

            iTextSharp.text.pdf.PdfPCell c3 = new iTextSharp.text.pdf.PdfPCell(
                new iTextSharp.text.Phrase(label2, fLabel));
            c3.BackgroundColor = azul;
            c3.Padding = 5;

            iTextSharp.text.pdf.PdfPCell c4 = new iTextSharp.text.pdf.PdfPCell(
                new iTextSharp.text.Phrase(valor2, fValor));
            c4.BackgroundColor = colorFondo;
            c4.Padding = 5;

            tabla.AddCell(c1);
            tabla.AddCell(c2);
            tabla.AddCell(c3);
            tabla.AddCell(c4);
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
            txtCedula.Focus();
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

            if (string.IsNullOrWhiteSpace(cedulaClienteActual))
            {
                MessageBox.Show("Debe buscar un cliente por cédula");
                txtCedula.Focus();
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
                string cedula = cedulaClienteActual;
                string numeroVuelo = cmbVueloIda.SelectedValue.ToString();
                int cantidadPasajeros = (int)numPasajeros.Value;
                string tipoTiquete = rbEconomico.Checked ? "Económico" :
                                   rbEjecutivo.Checked ? "Ejecutivo" : "Primera";
                bool equipaje = chkEquipaje.Checked;
                decimal precioTotal = Convert.ToDecimal(lblPrecioTotal.Text.Replace("$", "").Replace(",", ""));
                string estado = cmbEstado.Text;

                if (boton == 1) // INGRESO
                {
                    if (!pagoCompletado)
                    {
                        MessageBox.Show("Debe completar el pago antes de guardar la reserva.",
                                        "Pago pendiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Forzar estado Confirmada porque el pago ya se hizo
                    estado = "Confirmada";

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

                    // Generar PDF del comprobante
                    byte[] pdfBytes = GenerarPDFReservaIndividual(codigoReserva);

                    // Enviar correo con el PDF adjunto
                    EnviarCorreoConfirmacion(
                        txtEmailCliente.Text,
                        txtNombreCliente.Text,
                        codigoReserva,
                        cmbVueloIda.Text,
                        txtOrigen.Text,
                        txtDestino.Text,
                        txtFechaSalida.Text,
                        cantidadPasajeros,
                        tipoTiquete,
                        precioTotal,
                        pdfBytes   // <-- nuevo parámetro
                    );
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

        private void btnGenerarPDF_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCodigoReserva.Text))
            {
                MessageBox.Show("No hay ninguna reserva cargada. Busque o ingrese una reserva primero.");
                return;
            }

            byte[] pdfBytes = GenerarPDFReservaIndividual(txtCodigoReserva.Text);

            if (pdfBytes == null)
            {
                MessageBox.Show("No se pudo generar el PDF. Verifique que la reserva exista.");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PDF (*.pdf)|*.pdf";
            sfd.FileName = "Reserva_" + txtCodigoReserva.Text;

            if (sfd.ShowDialog() != DialogResult.OK) return;

            File.WriteAllBytes(sfd.FileName, pdfBytes);
            MessageBox.Show("PDF generado correctamente.");
            System.Diagnostics.Process.Start(sfd.FileName);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Validar que haya un precio calculado antes de pagar
            if (string.IsNullOrWhiteSpace(lblPrecioTotal.Text) ||
                lblPrecioTotal.Text == "$0")
            {
                MessageBox.Show("Primero complete los datos de la reserva para calcular el precio.");
                return;
            }

            decimal totalAPagar = Convert.ToDecimal(
                lblPrecioTotal.Text.Replace("$", "").Replace(".", "").Replace(",", ""));

            frmPago formPago = new frmPago(totalAPagar);

            if (formPago.ShowDialog() == DialogResult.OK)
            {
                pagoCompletado = true;
                metodoPagoSeleccionado = formPago.MetodoPago;

                // Feedback visual al usuario
                btnPagar.Text = "✔ Pago completado";
                btnPagar.BackColor = System.Drawing.Color.FromArgb(39, 119, 59);
                btnPagar.ForeColor = System.Drawing.Color.White;

                MessageBox.Show("Pago registrado con " + metodoPagoSeleccionado +
                                ".\nAhora puede guardar la reserva.");
            }
        }

        private void txtCedula_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtCedula.Text.Trim()))
                BuscarClientePorCedula(txtCedula.Text.Trim());
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

        void EnviarCorreoConfirmacion(string emailCliente, string nombreCliente,
            string codigoReserva, string vuelo, string origen, string destino,
            string fechaSalida, int pasajeros, string tipo, decimal precioTotal,
            byte[] pdfAdjunto)
        {
            try
            {
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(
                    "aerolink9097@gmail.com",
                    "rgod uggg cygz qqiu"
                );

                MailMessage mensaje = new MailMessage();
                mensaje.From = new MailAddress("aerolink9097@gmail.com", "Aerolínea");
                mensaje.To.Add(emailCliente);
                mensaje.Subject = "Confirmación de Reserva - " + codigoReserva;
                mensaje.IsBodyHtml = true;

                mensaje.Body = $@"
            <h2>¡Reserva Confirmada!</h2>
            <p>Estimado/a <b>{nombreCliente}</b>,</p>
            <p>Su reserva ha sido registrada exitosamente. Encontrará el comprobante adjunto en este correo.</p>
            <hr/>
            <table border='1' cellpadding='6' cellspacing='0'>
                <tr><td><b>Código de Reserva</b></td><td>{codigoReserva}</td></tr>
                <tr><td><b>Vuelo</b></td><td>{vuelo}</td></tr>
                <tr><td><b>Origen</b></td><td>{origen}</td></tr>
                <tr><td><b>Destino</b></td><td>{destino}</td></tr>
                <tr><td><b>Fecha de Salida</b></td><td>{fechaSalida}</td></tr>
                <tr><td><b>Pasajeros</b></td><td>{pasajeros}</td></tr>
                <tr><td><b>Tipo de Tiquete</b></td><td>{tipo}</td></tr>
                <tr><td><b>Precio Total</b></td><td>${precioTotal:N0}</td></tr>
            </table>
            <br/>
            <p>Gracias por volar con nosotros.</p>";

                // Adjuntar el PDF si se generó correctamente
                if (pdfAdjunto != null)
                {
                    MemoryStream msPDF = new MemoryStream(pdfAdjunto);
                    mensaje.Attachments.Add(new Attachment(msPDF,
                        "Reserva_" + codigoReserva + ".pdf", "application/pdf"));
                }

                smtp.Send(mensaje);
                MessageBox.Show("Correo de confirmación enviado a: " + emailCliente);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo enviar el correo:\n" + ex.Message);
            }
        }
        #endregion
    }
}