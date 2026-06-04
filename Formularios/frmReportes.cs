using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using ProyectoAerolinea.Clases;

namespace ProyectoAerolinea.Formularios
{
    public partial class frmReportes : Form
    {
        cConexion cn;
        SqlCommand cmd;
        SqlDataAdapter da;
        DataTable dt;
        string tipoReporteActual = "";
        Chart chartActual;

        public frmReportes()
        {
            InitializeComponent();
            cn = new cConexion();
        }

        #region Eventos del Formulario

        private void frmReportes_Load(object sender, EventArgs e)
        {
            ConfigurarDataGridView();
            EstablecerEventosBotones();
            ConfigurarGraficas();

            // Mostrar logo al inicio
            picLogo.Visible = true;
            dgvDatos.Visible = false;
            pnlResumen.Visible = false;
        }

        void MostrarDatos()
        {
            picLogo.Visible = false;
            dgvDatos.Visible = true;
        }

        #endregion

        #region Configuración Inicial

        void ConfigurarDataGridView()
        {
            dgvDatos.AllowUserToAddRows = false;
            dgvDatos.AllowUserToDeleteRows = false;
            dgvDatos.AllowUserToOrderColumns = true;
            dgvDatos.ReadOnly = true;
            dgvDatos.RowHeadersVisible = false;
            dgvDatos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvDatos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDatos.MultiSelect = false;
            dgvDatos.BorderStyle = BorderStyle.None;

            // Fondo general del grid (azul oscuro)
            dgvDatos.BackgroundColor = Color.FromArgb(15, 30, 55);
            dgvDatos.GridColor = Color.FromArgb(40, 65, 100);

            // Encabezados — azul más claro que el fondo para diferenciar
            dgvDatos.EnableHeadersVisualStyles = false;
            dgvDatos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(26, 55, 100);
            dgvDatos.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(180, 210, 255);
            dgvDatos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgvDatos.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(26, 55, 100);
            dgvDatos.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDatos.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgvDatos.ColumnHeadersHeight = 36;

            // Filas normales — azul oscuro con texto blanco
            dgvDatos.DefaultCellStyle.BackColor = Color.FromArgb(20, 40, 70);
            dgvDatos.DefaultCellStyle.ForeColor = Color.FromArgb(220, 235, 255);
            dgvDatos.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
            dgvDatos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(65, 120, 200);
            dgvDatos.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvDatos.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);

            // Filas alternas — un tono ligeramente diferente para distinguirlas
            dgvDatos.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(28, 52, 88);
            dgvDatos.AlternatingRowsDefaultCellStyle.ForeColor = Color.FromArgb(220, 235, 255);

            dgvDatos.RowTemplate.Height = 28;
            dgvDatos.Visible = false;
        }

        void ConfigurarGraficas()
        {
            chartActual = new Chart();
            chartActual.Dock = DockStyle.Fill;
            chartActual.Visible = false;
        }

        void EstablecerEventosBotones()
        {
            // Botones de herramientas
            btnImprimir.Click += BtnImprimir_Click;
            btnExportarExcel.Click += BtnExportarExcel_Click;
            btnExportarPDF.Click += BtnExportarPDF_Click;
            btnActualizar.Click += BtnActualizar_Click;

            // Botones de LISTADOS
            btnListadoClientes.Click += (s, e) => CargarReporteClientes();
            btnListadoDestinos.Click += (s, e) => CargarReporteDestinos();
            btnListadoVuelos.Click += (s, e) => CargarReporteVuelos();
            btnListadoReservas.Click += (s, e) => CargarReporteReservas();

            // Botones de ANÁLISIS
            btnReporteVentas.Click += (s, e) => CargarReporteVentas();
            btnOcupacion.Click += (s, e) => CargarReporteOcupacion();
            btnDestinosPopulares.Click += (s, e) => CargarReporteDestinosPopulares();

            // Botones de OPERATIVO
            btnVuelosDia.Click += (s, e) => CargarReporteVuelosDia();
            btnAlertas.Click += (s, e) => CargarReporteAlertas();
        }

        #endregion

        #region REPORTES - LISTADOS

        void CargarReporteClientes()
        {
            tipoReporteActual = "CLIENTES";
            lblEstado.Text = "Cargando listado de clientes...";
            this.Cursor = Cursors.WaitCursor;

            try
            {
                MostrarDatos();
                string query = @"
                    SELECT 
                        c.cedula AS 'Cédula',
                        c.nombre + ' ' + c.apellido AS 'Nombre Completo',
                        c.telefono AS 'Teléfono',
                        c.email AS 'Email',
                        c.pasaporte AS 'Pasaporte',
                        COUNT(r.idReserva) AS 'Total Reservas',
                        ISNULL(MAX(CONVERT(VARCHAR(10), v.fechaSalida, 103)), 'N/A') AS 'Último Vuelo'
                    FROM tblCliente c
                    LEFT JOIN tblReserva r ON c.cedula = r.cedula
                    LEFT JOIN tblVuelo v ON r.numeroVuelo = v.numeroVuelo
                    GROUP BY c.cedula, c.nombre, c.apellido, c.telefono, c.email, c.pasaporte
                    ORDER BY c.nombre ASC";

                cmd = new SqlCommand(query, cn.AbrirConexion());
                da = new SqlDataAdapter(cmd);
                dt = new DataTable();
                da.Fill(dt);
                cn.CerrarConexion();

                dgvDatos.DataSource = dt;
                pnlResumen.Visible = false;
                MostrarResumenClientes();
                lblEstado.Text = $"✓ Reporte de {dt.Rows.Count} clientes registrados";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar reporte de clientes: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblEstado.Text = "✗ Error al cargar datos";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        void CargarReporteDestinos()
        {
            tipoReporteActual = "DESTINOS";
            lblEstado.Text = "Cargando listado de destinos...";
            this.Cursor = Cursors.WaitCursor;

            try
            {
                MostrarDatos();
                string query = @"
                    SELECT 
                        d.codigoAeropuerto AS 'Código',
                        d.ciudad AS 'Ciudad',
                        d.pais AS 'País',
                        ISNULL(COUNT(DISTINCT CASE WHEN v.origen = d.codigoAeropuerto OR v.destino = d.codigoAeropuerto THEN v.numeroVuelo END), 0) AS 'Vuelos',
                        ISNULL(COUNT(DISTINCT p.codigoPaquete), 0) AS 'Paquetes'
                    FROM tblDestino d
                    LEFT JOIN tblVuelo v ON d.codigoAeropuerto IN (v.origen, v.destino)
                    LEFT JOIN tblPaquete p ON d.codigoAeropuerto = p.destino
                    GROUP BY d.codigoAeropuerto, d.ciudad, d.pais
                    ORDER BY d.ciudad ASC";

                cmd = new SqlCommand(query, cn.AbrirConexion());
                da = new SqlDataAdapter(cmd);
                dt = new DataTable();
                da.Fill(dt);
                cn.CerrarConexion();

                dgvDatos.DataSource = dt;
                pnlResumen.Visible = false;
                MostrarResumenDestinos();
                lblEstado.Text = $"✓ Reporte de {dt.Rows.Count} destinos";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar reporte de destinos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblEstado.Text = "✗ Error al cargar datos";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        void CargarReporteVuelos()
        {
            tipoReporteActual = "VUELOS";
            lblEstado.Text = "Cargando listado de vuelos...";
            this.Cursor = Cursors.WaitCursor;

            try
            {
                MostrarDatos();
                string query = @"
                    SELECT 
                        v.numeroVuelo AS 'Vuelo',
                        d1.ciudad + ' → ' + d2.ciudad AS 'Ruta',
                        CONVERT(VARCHAR(16), v.fechaSalida, 120) AS 'Salida',
                        CONVERT(VARCHAR(16), v.fechaLlegada, 120) AS 'Llegada',
                        DATEDIFF(MINUTE, v.fechaSalida, v.fechaLlegada) AS 'Duración (min)',
                        v.asientosTotales AS 'Asientos Totales',
                        v.asientosDisponibles AS 'Disponibles',
                        CASE 
                            WHEN v.asientosTotales > 0 THEN CAST((v.asientosTotales - v.asientosDisponibles) * 100.0 / v.asientosTotales AS DECIMAL(5,2))
                            ELSE 0
                        END AS 'Ocupación %',
                        v.precioBase AS 'Precio Base'
                    FROM tblVuelo v
                    INNER JOIN tblDestino d1 ON v.origen = d1.codigoAeropuerto
                    INNER JOIN tblDestino d2 ON v.destino = d2.codigoAeropuerto
                    ORDER BY v.fechaSalida DESC";

                cmd = new SqlCommand(query, cn.AbrirConexion());
                da = new SqlDataAdapter(cmd);
                dt = new DataTable();
                da.Fill(dt);
                cn.CerrarConexion();

                dgvDatos.DataSource = dt;
                pnlResumen.Visible = false;
                MostrarResumenVuelos();
                lblEstado.Text = $"✓ Reporte de {dt.Rows.Count} vuelos";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar reporte de vuelos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblEstado.Text = "✗ Error al cargar datos";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        void CargarReporteReservas()
        {
            tipoReporteActual = "RESERVAS";
            lblEstado.Text = "Cargando historial de reservas...";
            this.Cursor = Cursors.WaitCursor;

            try
            {
                MostrarDatos();
                string query = @"
                    SELECT 
                        r.codigoReserva AS 'Código',
                        CONVERT(VARCHAR(10), r.fechaReserva, 103) AS 'Fecha Reserva',
                        c.nombre + ' ' + c.apellido AS 'Cliente',
                        v.numeroVuelo AS 'Vuelo',
                        d1.ciudad + ' → ' + d2.ciudad AS 'Ruta',
                        r.cantidadPasajeros AS 'Pasajeros',
                        r.tipoTiquete AS 'Tipo',
                        r.precioTotal AS 'Precio Total',
                        r.estado AS 'Estado',
                        CONVERT(VARCHAR(10), v.fechaSalida, 103) AS 'Fecha Vuelo'
                    FROM tblReserva r
                    INNER JOIN tblCliente c ON r.cedula = c.cedula
                    INNER JOIN tblVuelo v ON r.numeroVuelo = v.numeroVuelo
                    INNER JOIN tblDestino d1 ON v.origen = d1.codigoAeropuerto
                    INNER JOIN tblDestino d2 ON v.destino = d2.codigoAeropuerto
                    ORDER BY r.fechaReserva DESC";

                cmd = new SqlCommand(query, cn.AbrirConexion());
                da = new SqlDataAdapter(cmd);
                dt = new DataTable();
                da.Fill(dt);
                cn.CerrarConexion();

                dgvDatos.DataSource = dt;
                pnlResumen.Visible = false;
                MostrarResumenReservas();
                lblEstado.Text = $"✓ Historial de {dt.Rows.Count} reservas";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar reporte de reservas: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblEstado.Text = "✗ Error al cargar datos";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        #endregion

        #region REPORTES - ANÁLISIS CON GRÁFICAS

        void CargarReporteVentas()
        {
            tipoReporteActual = "VENTAS";
            lblEstado.Text = "Calculando reporte de ventas...";
            this.Cursor = Cursors.WaitCursor;

            try
            {
                MostrarDatos();
                string query = @"
            SELECT 
                CONVERT(VARCHAR(10), r.fechaReserva, 103) AS 'Fecha',
                COUNT(r.idReserva) AS 'Total Reservas',
                ISNULL(SUM(r.precioTotal), 0) AS 'Ingresos',
                ISNULL(AVG(r.precioTotal), 0) AS 'Promedio',
                ISNULL(SUM(r.cantidadPasajeros), 0) AS 'Pasajeros'
            FROM tblReserva r
            WHERE r.estado = 'Confirmada'
            GROUP BY CONVERT(VARCHAR(10), r.fechaReserva, 103)
            ORDER BY CONVERT(VARCHAR(10), r.fechaReserva, 103) DESC";

                cmd = new SqlCommand(query, cn.AbrirConexion());
                da = new SqlDataAdapter(cmd);
                dt = new DataTable();
                da.Fill(dt);
                cn.CerrarConexion();

                dgvDatos.DataSource = dt;
                MostrarGraficaVentas();
                MostrarResumenVentas();
                lblEstado.Text = "✓ Reporte de ventas calculado";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar reporte de ventas: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblEstado.Text = "✗ Error al cargar datos";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        void CargarReporteOcupacion()
        {
            tipoReporteActual = "OCUPACION";
            lblEstado.Text = "Analizando ocupación de vuelos...";
            this.Cursor = Cursors.WaitCursor;

            try
            {
                MostrarDatos();
                string query = @"
                    SELECT 
                        v.numeroVuelo AS 'Vuelo',
                        d1.ciudad + ' → ' + d2.ciudad AS 'Ruta',
                        CONVERT(VARCHAR(10), v.fechaSalida, 103) AS 'Fecha',
                        v.asientosTotales AS 'Totales',
                        (v.asientosTotales - v.asientosDisponibles) AS 'Vendidos',
                        v.asientosDisponibles AS 'Disponibles',
                        CASE 
                            WHEN v.asientosTotales > 0 THEN CAST((v.asientosTotales - v.asientosDisponibles) * 100.0 / v.asientosTotales AS DECIMAL(5,1))
                            ELSE 0
                        END AS 'Ocupación %'
                    FROM tblVuelo v
                    INNER JOIN tblDestino d1 ON v.origen = d1.codigoAeropuerto
                    INNER JOIN tblDestino d2 ON v.destino = d2.codigoAeropuerto
                    ORDER BY 'Ocupación %' DESC";

                cmd = new SqlCommand(query, cn.AbrirConexion());
                da = new SqlDataAdapter(cmd);
                dt = new DataTable();
                da.Fill(dt);
                cn.CerrarConexion();

                dgvDatos.DataSource = dt;
                MostrarGraficaOcupacion();
                MostrarResumenOcupacion();
                lblEstado.Text = "✓ Análisis de ocupación completado";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar reporte de ocupación: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblEstado.Text = "✗ Error al cargar datos";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        void CargarReporteDestinosPopulares()
        {
            tipoReporteActual = "DESTINOS_POPULARES";
            lblEstado.Text = "Identificando destinos más vendidos...";
            this.Cursor = Cursors.WaitCursor;

            try
            {
                MostrarDatos();
                string query = @"
                    SELECT TOP 10
                        d.codigoAeropuerto AS 'Código',
                        d.ciudad AS 'Ciudad',
                        d.pais AS 'País',
                        COUNT(r.idReserva) AS 'Reservas',
                        CASE 
                            WHEN (SELECT COUNT(*) FROM tblReserva) > 0 THEN CAST(COUNT(r.idReserva) * 100.0 / (SELECT COUNT(*) FROM tblReserva) AS DECIMAL(5,2))
                            ELSE 0
                        END AS 'Porcentaje %',
                        ISNULL(SUM(r.precioTotal), 0) AS 'Ingresos Totales',
                        ISNULL(AVG(r.precioTotal), 0) AS 'Ingreso Promedio'
                    FROM tblDestino d
                    LEFT JOIN tblVuelo v ON d.codigoAeropuerto = v.destino
                    LEFT JOIN tblReserva r ON v.numeroVuelo = r.numeroVuelo
                    WHERE r.idReserva IS NOT NULL
                    GROUP BY d.codigoAeropuerto, d.ciudad, d.pais
                    ORDER BY COUNT(r.idReserva) DESC";

                cmd = new SqlCommand(query, cn.AbrirConexion());
                da = new SqlDataAdapter(cmd);
                dt = new DataTable();
                da.Fill(dt);
                cn.CerrarConexion();

                dgvDatos.DataSource = dt;
                MostrarGraficaDestinosPopulares();
                MostrarResumenDestinosPopulares();
                lblEstado.Text = $"✓ Top {dt.Rows.Count} destinos más vendidos";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar reporte de destinos populares: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblEstado.Text = "✗ Error al cargar datos";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        #endregion

        #region REPORTES - OPERATIVO

        void CargarReporteVuelosDia()
        {
            tipoReporteActual = "VUELOS_DIA";
            this.Cursor = Cursors.WaitCursor;

            // Preguntar qué fecha consultar
            Form frmFecha = new Form();
            frmFecha.Text = "Seleccionar Fecha";
            frmFecha.Size = new Size(320, 150);
            frmFecha.StartPosition = FormStartPosition.CenterParent;
            frmFecha.FormBorderStyle = FormBorderStyle.FixedDialog;
            frmFecha.MaximizeBox = false;

            Label lbl = new Label();
            lbl.Text = "Fecha a consultar:";
            lbl.Location = new Point(20, 20);
            lbl.AutoSize = true;

            DateTimePicker dtp = new DateTimePicker();
            dtp.Format = DateTimePickerFormat.Short;
            dtp.Value = DateTime.Today;
            dtp.Location = new Point(20, 45);
            dtp.Width = 200;

            Button btnOk = new Button();
            btnOk.Text = "Consultar";
            btnOk.Location = new Point(200, 75);
            btnOk.DialogResult = DialogResult.OK;

            frmFecha.Controls.AddRange(new Control[] { lbl, dtp, btnOk });
            frmFecha.AcceptButton = btnOk;

            DateTime fechaConsulta = DateTime.Today;
            if (frmFecha.ShowDialog() == DialogResult.OK)
                fechaConsulta = dtp.Value.Date;
            else
            {
                this.Cursor = Cursors.Default;
                return;
            }

            lblEstado.Text = "Cargando vuelos del " + fechaConsulta.ToString("dd/MM/yyyy") + "...";

            try
            {
                string query = @"
            SELECT 
                CONVERT(VARCHAR(5), v.fechaSalida, 108) AS 'Hora Salida',
                v.numeroVuelo AS 'Vuelo',
                d1.ciudad + ' → ' + d2.ciudad AS 'Ruta',
                CASE 
                    WHEN v.fechaSalida > GETDATE() THEN 'Por despegar'
                    WHEN v.fechaLlegada < GETDATE() THEN 'Aterrizó'
                    ELSE 'En curso'
                END AS 'Estado',
                (v.asientosTotales - v.asientosDisponibles) AS 'Pasajeros',
                v.asientosTotales AS 'Capacidad',
                v.asientosDisponibles AS 'Disponibles'
            FROM tblVuelo v
            INNER JOIN tblDestino d1 ON v.origen = d1.codigoAeropuerto
            INNER JOIN tblDestino d2 ON v.destino = d2.codigoAeropuerto
            WHERE CONVERT(DATE, v.fechaSalida) = @fecha
            ORDER BY v.fechaSalida ASC";

                cmd = new SqlCommand(query, cn.AbrirConexion());
                cmd.Parameters.AddWithValue("@fecha", fechaConsulta);
                da = new SqlDataAdapter(cmd);
                dt = new DataTable();
                da.Fill(dt);
                cn.CerrarConexion();

                dgvDatos.DataSource = dt;
                pnlResumen.Visible = false;

                if (dt.Rows.Count == 0)
                    lblEstado.Text = "No hay vuelos programados para el " + fechaConsulta.ToString("dd/MM/yyyy");
                else
                    lblEstado.Text = $"✓ {dt.Rows.Count} vuelos para el {fechaConsulta:dd/MM/yyyy}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar vuelos del día: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblEstado.Text = "✗ Error al cargar datos";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        void CargarReporteAlertas()
        {
            tipoReporteActual = "ALERTAS";
            lblEstado.Text = "Analizando alertas del sistema...";
            this.Cursor = Cursors.WaitCursor;

            try
            {
                DataTable dtAlertas = new DataTable();
                dtAlertas.Columns.Add("Tipo Alerta", typeof(string));
                dtAlertas.Columns.Add("Descripción", typeof(string));
                dtAlertas.Columns.Add("Cantidad", typeof(int));
                dtAlertas.Columns.Add("Severidad", typeof(string));

                // Alerta 1: Vuelos con baja ocupación
                cn.AbrirConexion();
                cmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM tblVuelo 
                    WHERE asientosTotales > 0 AND (asientosTotales - asientosDisponibles) * 100.0 / asientosTotales < 30",
                    cn.AbrirConexion());
                int vuelosBajaOcupacion = (int)cmd.ExecuteScalar();
                cn.CerrarConexion();

                if (vuelosBajaOcupacion > 0)
                    dtAlertas.Rows.Add("⚠️ Baja Ocupación", $"{vuelosBajaOcupacion} vuelos con ocupación < 30%", vuelosBajaOcupacion, "Moderada");

                // Alerta 2: Clientes sin email
                cmd = new SqlCommand("SELECT COUNT(*) FROM tblCliente WHERE email IS NULL OR email = ''", cn.AbrirConexion());
                int clientesSinEmail = (int)cmd.ExecuteScalar();
                cn.CerrarConexion();

                if (clientesSinEmail > 0)
                    dtAlertas.Rows.Add("ℹ️ Datos Incompletos", $"{clientesSinEmail} clientes sin email", clientesSinEmail, "Baja");

                // Alerta 3: Destinos sin vuelos
                cmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM tblDestino d 
                    WHERE NOT EXISTS (SELECT 1 FROM tblVuelo WHERE origen = d.codigoAeropuerto OR destino = d.codigoAeropuerto)",
                    cn.AbrirConexion());
                int destinosSinVuelos = (int)cmd.ExecuteScalar();
                cn.CerrarConexion();

                if (destinosSinVuelos > 0)
                    dtAlertas.Rows.Add("ℹ️ Destinos Inactivos", $"{destinosSinVuelos} destinos sin vuelos", destinosSinVuelos, "Baja");

                // Alerta 4: Asientos negativos
                cmd = new SqlCommand("SELECT COUNT(*) FROM tblVuelo WHERE asientosDisponibles < 0", cn.AbrirConexion());
                int asientosNegativos = (int)cmd.ExecuteScalar();
                cn.CerrarConexion();

                if (asientosNegativos > 0)
                    dtAlertas.Rows.Add("🔴 Error Crítico", $"{asientosNegativos} vuelos con asientos negativos", asientosNegativos, "Crítica");

                if (dtAlertas.Rows.Count == 0)
                    dtAlertas.Rows.Add("✓ Sin Alertas", "El sistema está funcionando correctamente", 0, "OK");

                dgvDatos.DataSource = dtAlertas;
                pnlResumen.Visible = false;
                lblEstado.Text = $"✓ Análisis completado: {dtAlertas.Rows.Count} elementos";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar alertas: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblEstado.Text = "✗ Error al cargar datos";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        #endregion

        #region GRÁFICAS

        void MostrarGraficaVentas()
        {
            if (dt.Rows.Count == 0) return;

            pnlResumen.Controls.Clear();
            pnlResumen.Size = new Size(pnlResumen.Parent.ClientSize.Width - 20, 350);
            pnlResumen.BackColor = Color.FromArgb(15, 30, 55);

            Chart chart = new Chart();
            chart.Dock = DockStyle.Fill;
            chart.BackColor = Color.FromArgb(15, 30, 55);
            chart.BorderlineColor = Color.FromArgb(40, 65, 100);

            ChartArea chartArea = new ChartArea("Default");
            chartArea.BackColor = Color.FromArgb(20, 40, 70);
            chartArea.BorderColor = Color.FromArgb(65, 120, 200);
            chartArea.BorderWidth = 1;

            chartArea.AxisX.Title = "Fecha";
            chartArea.AxisX.TitleForeColor = Color.FromArgb(180, 210, 255);
            chartArea.AxisX.LabelStyle.ForeColor = Color.FromArgb(180, 210, 255);
            chartArea.AxisX.LabelStyle.Angle = -45;
            chartArea.AxisX.LabelStyle.Font = new Font("Segoe UI", 7f); // <-- fuente pequeña para que quepan
            chartArea.AxisX.LineColor = Color.FromArgb(65, 120, 200);
            chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(40, 65, 100);
            chartArea.AxisX.MajorTickMark.LineColor = Color.FromArgb(65, 120, 200);
            chartArea.AxisX.Interval = 1;                        // <-- forzar todas las etiquetas
            chartArea.AxisX.IsLabelAutoFit = false;              // <-- desactivar ajuste automático
            chartArea.AxisX.LabelStyle.IsEndLabelVisible = true; // <-- mostrar etiqueta final

            chartArea.AxisY.Title = "Ingresos ($)";
            chartArea.AxisY.TitleForeColor = Color.FromArgb(180, 210, 255);
            chartArea.AxisY.LabelStyle.ForeColor = Color.FromArgb(180, 210, 255);
            chartArea.AxisY.LabelStyle.Font = new Font("Segoe UI", 8f);
            chartArea.AxisY.LineColor = Color.FromArgb(65, 120, 200);
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(40, 65, 100);
            chartArea.AxisY.MajorTickMark.LineColor = Color.FromArgb(65, 120, 200);

            chart.ChartAreas.Add(chartArea);

            Series series = new Series("Ingresos");
            series.ChartType = SeriesChartType.Column;
            series.Color = Color.FromArgb(65, 145, 255);
            series.BorderColor = Color.FromArgb(100, 180, 255);
            series.BorderWidth = 1;
            series.IsValueShownAsLabel = true;
            series.Font = new Font("Segoe UI", 7f);
            series.LabelForeColor = Color.FromArgb(220, 235, 255);

            // Contador para limitar a 10 fechas
            int contadorFechas = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (contadorFechas >= 10) break;
                string fecha = row["Fecha"].ToString();
                decimal ingresos = Convert.ToDecimal(row["Ingresos"]);
                series.Points.AddXY(fecha, ingresos);
                contadorFechas++;
            }

            chart.Series.Add(series);

            Title titulo = new Title();
            titulo.Text = "Ingresos por Fecha";
            titulo.Docking = Docking.Top;
            titulo.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            titulo.ForeColor = Color.FromArgb(180, 210, 255);
            chart.Titles.Add(titulo);

            pnlResumen.Controls.Add(chart);
            pnlResumen.Visible = true;
            chartActual = chart;
        }

        void MostrarGraficaOcupacion()
        {
            if (dt.Rows.Count == 0) return;

            pnlResumen.Controls.Clear();
            pnlResumen.Size = new Size(pnlResumen.Parent.ClientSize.Width - 20, 400);
            pnlResumen.BackColor = Color.FromArgb(15, 30, 55);

            Chart chart = new Chart();
            chart.Dock = DockStyle.Fill;
            chart.BackColor = Color.FromArgb(15, 30, 55);

            ChartArea chartArea = new ChartArea("Default");
            chartArea.BackColor = Color.FromArgb(20, 40, 70);
            chartArea.BorderColor = Color.FromArgb(65, 120, 200);
            chartArea.BorderWidth = 1;

            chartArea.AxisX.Title = "Vuelo";
            chartArea.AxisX.TitleForeColor = Color.FromArgb(180, 210, 255);
            chartArea.AxisX.LabelStyle.ForeColor = Color.FromArgb(180, 210, 255);
            chartArea.AxisX.LabelStyle.Font = new Font("Segoe UI", 7f); // <-- fuente pequeña
            chartArea.AxisX.LineColor = Color.FromArgb(65, 120, 200);
            chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(40, 65, 100);
            chartArea.AxisX.MajorTickMark.LineColor = Color.FromArgb(65, 120, 200);
            chartArea.AxisX.Interval = 1;                        // <-- forzar todas las etiquetas
            chartArea.AxisX.IsLabelAutoFit = false;              // <-- desactivar ajuste automático
            chartArea.AxisX.LabelStyle.IsEndLabelVisible = true; // <-- mostrar etiqueta final

            chartArea.AxisY.Title = "Ocupación (%)";
            chartArea.AxisY.TitleForeColor = Color.FromArgb(180, 210, 255);
            chartArea.AxisY.LabelStyle.ForeColor = Color.FromArgb(180, 210, 255);
            chartArea.AxisY.LabelStyle.Font = new Font("Segoe UI", 8f);
            chartArea.AxisY.Maximum = 100;
            chartArea.AxisY.Interval = 10;
            chartArea.AxisY.LineColor = Color.FromArgb(65, 120, 200);
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(40, 65, 100);
            chartArea.AxisY.MajorTickMark.LineColor = Color.FromArgb(65, 120, 200);

            chart.ChartAreas.Add(chartArea);

            Series series = new Series("Ocupación");
            series.ChartType = SeriesChartType.Bar;
            series.IsValueShownAsLabel = true;
            series.Font = new Font("Segoe UI", 8f);
            series.LabelForeColor = Color.FromArgb(220, 235, 255);
            series.LabelFormat = "{0:F1}%";

            // <-- Limitado a 10 vuelos
            int contador = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (contador >= 10) break;
                string vuelo = row["Vuelo"].ToString();
                decimal ocupacion = Convert.ToDecimal(row["Ocupación %"]);
                int idx = series.Points.AddXY(vuelo, ocupacion);

                if (ocupacion >= 70)
                    series.Points[idx].Color = Color.FromArgb(50, 200, 100);
                else if (ocupacion >= 40)
                    series.Points[idx].Color = Color.FromArgb(255, 200, 50);
                else
                    series.Points[idx].Color = Color.FromArgb(255, 80, 80);

                contador++;
            }

            chart.Series.Add(series);

            Title titulo = new Title();
            titulo.Text = "Ocupación de Vuelos (%)";
            titulo.Docking = Docking.Top;
            titulo.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            titulo.ForeColor = Color.FromArgb(180, 210, 255);
            chart.Titles.Add(titulo);

            pnlResumen.Controls.Add(chart);
            pnlResumen.Visible = true;
            chartActual = chart;
        }

        void MostrarGraficaDestinosPopulares()
        {
            if (dt.Rows.Count == 0) return;

            pnlResumen.Controls.Clear();
            pnlResumen.Size = new Size(pnlResumen.Parent.ClientSize.Width - 20, 400);
            pnlResumen.BackColor = Color.FromArgb(15, 30, 55);

            Chart chart = new Chart();
            chart.Dock = DockStyle.Fill;
            chart.BackColor = Color.FromArgb(15, 30, 55);

            ChartArea chartArea = new ChartArea("Default");
            chartArea.BackColor = Color.FromArgb(20, 40, 70);
            chartArea.BorderColor = Color.FromArgb(65, 120, 200);
            chartArea.BorderWidth = 1;
            chart.ChartAreas.Add(chartArea);

            Legend leyenda = new Legend();
            leyenda.Docking = Docking.Right;
            leyenda.Font = new Font("Segoe UI", 9f);
            leyenda.BackColor = Color.FromArgb(20, 40, 70);
            leyenda.ForeColor = Color.FromArgb(220, 235, 255);
            leyenda.BorderColor = Color.FromArgb(65, 120, 200);
            chart.Legends.Add(leyenda);

            Series series = new Series("Reservas");
            series.ChartType = SeriesChartType.Pie;
            series["PieLabelStyle"] = "Outside";
            series["PieLineColor"] = "Silver";

            Color[] colores = {
        Color.FromArgb(65,  145, 255),
        Color.FromArgb(50,  200, 100),
        Color.FromArgb(255, 160,  50),
        Color.FromArgb(255,  80,  80),
        Color.FromArgb(180,  80, 220),
        Color.FromArgb(0,   200, 210),
        Color.FromArgb(255, 220,  50),
        Color.FromArgb(120, 160, 200),
        Color.FromArgb(255, 120, 160),
        Color.FromArgb(100, 180, 130)
    };

            int i = 0;
            foreach (DataRow row in dt.Rows)
            {
                string ciudad = row["Ciudad"].ToString();
                int reservas = Convert.ToInt32(row["Reservas"]);
                DataPoint point = new DataPoint();
                point.SetValueXY(ciudad, reservas);
                point.Label = $"{ciudad} ({reservas})";
                point.LabelForeColor = Color.FromArgb(220, 235, 255);
                point.LegendText = ciudad;
                if (i < colores.Length) point.Color = colores[i];
                series.Points.Add(point);
                i++;
            }

            chart.Series.Add(series);

            Title titulo = new Title();
            titulo.Text = "Destinos Más Populares";
            titulo.Docking = Docking.Top;
            titulo.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            titulo.ForeColor = Color.FromArgb(180, 210, 255);
            chart.Titles.Add(titulo);

            pnlResumen.Controls.Add(chart);
            pnlResumen.Visible = true;
            chartActual = chart;
        }

        #endregion

        #region RESÚMENES DE ESTADÍSTICAS

        void MostrarResumenClientes()
        {
            if (dt.Rows.Count == 0)
            {
                lblEstado.Text = "No hay clientes registrados";
                return;
            }

            int totalClientes = dt.Rows.Count;
            int clientesConReservas = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (int.TryParse(row["Total Reservas"].ToString(), out int reservas) && reservas > 0)
                    clientesConReservas++;
            }

            MessageBox.Show(
                $"═══════════════════════════════════════════\n" +
                $"📊 RESUMEN DE CLIENTES\n" +
                $"═══════════════════════════════════════════\n\n" +
                $"👥 Total de Clientes: {totalClientes}\n" +
                $"🎫 Clientes con Reservas: {clientesConReservas}\n" +
                $"❌ Clientes sin Reservas: {totalClientes - clientesConReservas}",
                "Resumen Estadístico",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        void MostrarResumenDestinos()
        {
            if (dt.Rows.Count == 0)
            {
                lblEstado.Text = "No hay destinos registrados";
                return;
            }

            int totalDestinos = dt.Rows.Count;
            int totalVuelos = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (int.TryParse(row["Vuelos"].ToString(), out int vuelos))
                    totalVuelos += vuelos;
            }

            MessageBox.Show(
                $"═══════════════════════════════════════════\n" +
                $"📍 RESUMEN DE DESTINOS\n" +
                $"═══════════════════════════════════════════\n\n" +
                $"🌍 Total de Destinos: {totalDestinos}\n" +
                $"✈️ Total de Vuelos: {totalVuelos}",
                "Resumen Estadístico",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        void MostrarResumenVuelos()
        {
            if (dt.Rows.Count == 0)
            {
                lblEstado.Text = "No hay vuelos registrados";
                return;
            }

            int totalVuelos = dt.Rows.Count;
            decimal ocupacionPromedio = 0;
            int vuelosAltos = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (decimal.TryParse(row["Ocupación %"].ToString(), out decimal ocupacion))
                {
                    ocupacionPromedio += ocupacion;
                    if (ocupacion >= 80) vuelosAltos++;
                }
            }

            ocupacionPromedio /= totalVuelos;

            MessageBox.Show(
                $"═══════════════════════════════════════════\n" +
                $"✈️ RESUMEN DE VUELOS\n" +
                $"═══════════════════════════════════════════\n\n" +
                $"📈 Total de Vuelos: {totalVuelos}\n" +
                $"📊 Ocupación Promedio: {ocupacionPromedio:F1}%\n" +
                $"🚀 Vuelos Llenos (≥80%): {vuelosAltos}",
                "Resumen Estadístico",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        void MostrarResumenReservas()
        {
            if (dt.Rows.Count == 0)
            {
                lblEstado.Text = "No hay reservas registradas";
                return;
            }

            int totalReservas = dt.Rows.Count;
            decimal ingresoTotal = 0;
            int confirmadas = 0;
            int canceladas = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (decimal.TryParse(row["Precio Total"].ToString(), out decimal precio))
                    ingresoTotal += precio;

                string estado = row["Estado"].ToString();
                if (estado == "Confirmada") confirmadas++;
                else if (estado == "Cancelada") canceladas++;
            }

            MessageBox.Show(
                $"═══════════════════════════════════════════\n" +
                $"🎫 RESUMEN DE RESERVAS\n" +
                $"═══════════════════════════════════════════\n\n" +
                $"📋 Total de Reservas: {totalReservas}\n" +
                $"✓ Confirmadas: {confirmadas}\n" +
                $"✗ Canceladas: {canceladas}\n" +
                $"💰 Ingresos Totales: ${ingresoTotal:N0}",
                "Resumen Estadístico",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        void MostrarResumenVentas()
        {
            if (dt.Rows.Count == 0)
            {
                lblEstado.Text = "No hay datos de ventas";
                return;
            }

            decimal ingresoTotal = 0;
            int totalReservas = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (decimal.TryParse(row["Ingresos"].ToString(), out decimal ingresos))
                    ingresoTotal += ingresos;
                if (int.TryParse(row["Total Reservas"].ToString(), out int reservas))
                    totalReservas += reservas;
            }

            decimal promedio = totalReservas > 0 ? ingresoTotal / totalReservas : 0;

            MessageBox.Show(
                $"═══════════════════════════════════════════\n" +
                $"💰 RESUMEN DE VENTAS\n" +
                $"═══════════════════════════════════════════\n\n" +
                $"📅 Período: {dt.Rows.Count} días\n" +
                $"📊 Total de Reservas: {totalReservas}\n" +
                $"💵 Ingresos Totales: ${ingresoTotal:N0}\n" +
                $"📈 Promedio por Reserva: ${promedio:N0}",
                "Resumen Estadístico",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        void MostrarResumenOcupacion()
        {
            if (dt.Rows.Count == 0)
            {
                lblEstado.Text = "No hay datos de ocupación";
                return;
            }

            decimal ocupacionPromedio = 0;
            int alta = 0, media = 0, baja = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (decimal.TryParse(row["Ocupación %"].ToString(), out decimal ocupacion))
                {
                    ocupacionPromedio += ocupacion;
                    if (ocupacion > 70) alta++;
                    else if (ocupacion >= 40) media++;
                    else baja++;
                }
            }

            ocupacionPromedio /= dt.Rows.Count;

            MessageBox.Show(
                $"═══════════════════════════════════════════\n" +
                $"📊 RESUMEN DE OCUPACIÓN\n" +
                $"═══════════════════════════════════════════\n\n" +
                $"✈️ Vuelos Analizados: {dt.Rows.Count}\n" +
                $"📈 Ocupación Promedio: {ocupacionPromedio:F1}%\n\n" +
                $"🟢 Alta Ocupación (>70%): {alta}\n" +
                $"🟡 Media Ocupación (40-70%): {media}\n" +
                $"🔴 Baja Ocupación (<40%): {baja}",
                "Resumen Estadístico",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        void MostrarResumenDestinosPopulares()
        {
            if (dt.Rows.Count == 0)
            {
                lblEstado.Text = "No hay destinos populares";
                return;
            }

            decimal totalReservas = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (int.TryParse(row["Reservas"].ToString(), out int res))
                    totalReservas += res;
            }

            string destino = dt.Rows[0]["Ciudad"].ToString();
            int reservas = int.TryParse(dt.Rows[0]["Reservas"].ToString(), out int r) ? r : 0;
            decimal ingresos = decimal.TryParse(dt.Rows[0]["Ingresos Totales"].ToString(), out decimal ing) ? ing : 0;

            MessageBox.Show(
                $"═══════════════════════════════════════════\n" +
                $"⭐ DESTINO MÁS POPULAR\n" +
                $"═══════════════════════════════════════════\n\n" +
                $"🌍 Destino: {destino}\n" +
                $"🎫 Total de Reservas: {reservas}\n" +
                $"📊 % del Total: {(reservas / totalReservas * 100):F1}%\n" +
                $"💰 Ingresos Generados: ${ingresos:N0}\n\n" +
                $"(Top {dt.Rows.Count} destinos)",
                "Resumen Estadístico",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        #endregion

        #region EXPORTACIÓN

        private void BtnExportarExcel_Click(object sender, EventArgs e)
        {
            if (dgvDatos.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel CSV|*.csv|Excel Workbook|*.xlsx";
            sfd.FileName = $"Reporte_{tipoReporteActual}_{DateTime.Now:yyyyMMdd_HHmmss}";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ExportarACSV(sfd.FileName);
                    MessageBox.Show($"✓ Archivo exportado correctamente\n\n{sfd.FileName}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(Path.GetDirectoryName(sfd.FileName)) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al exportar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void ExportarACSV(string rutaArchivo)
        {
            StringBuilder sb = new StringBuilder();

            // Encabezados
            for (int i = 0; i < dgvDatos.Columns.Count; i++)
            {
                sb.Append("\"" + dgvDatos.Columns[i].HeaderText.Replace("\"", "\"\"") + "\"");
                if (i < dgvDatos.Columns.Count - 1) sb.Append(",");
            }
            sb.AppendLine();

            // Datos
            foreach (DataGridViewRow row in dgvDatos.Rows)
            {
                for (int i = 0; i < dgvDatos.Columns.Count; i++)
                {
                    string valor = row.Cells[i].Value?.ToString() ?? "";
                    valor = valor.Replace("\"", "\"\"");
                    sb.Append("\"" + valor + "\"");
                    if (i < dgvDatos.Columns.Count - 1) sb.Append(",");
                }
                sb.AppendLine();
            }

            if (!rutaArchivo.EndsWith(".csv"))
                rutaArchivo += ".csv";

            File.WriteAllText(rutaArchivo, sb.ToString(), Encoding.UTF8);
        }

        private void BtnExportarPDF_Click(object sender, EventArgs e)
        {
            if (dgvDatos.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PDF Document|*.pdf";
            sfd.FileName = $"Reporte_{tipoReporteActual}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ExportarAPDF(sfd.FileName);
                    MessageBox.Show($"✓ PDF generado correctamente\n\n{sfd.FileName}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al generar PDF: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void ExportarAPDF(string rutaArchivo)
        {
            try
            {
                iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate());
                iTextSharp.text.pdf.PdfWriter.GetInstance(doc, new FileStream(rutaArchivo, FileMode.Create));
                doc.Open();

                // Título - CORREGIDO
                iTextSharp.text.Font titleFont = iTextSharp.text.FontFactory.GetFont("Arial", 18, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Paragraph title = new iTextSharp.text.Paragraph($"REPORTE: {tipoReporteActual}", titleFont);
                title.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                doc.Add(title);

                doc.Add(new iTextSharp.text.Paragraph("\n"));

                // Fecha - CORREGIDO
                iTextSharp.text.Font dateFont = iTextSharp.text.FontFactory.GetFont("Arial", 10);
                iTextSharp.text.Paragraph date = new iTextSharp.text.Paragraph($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", dateFont);
                date.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;
                doc.Add(date);

                doc.Add(new iTextSharp.text.Paragraph("\n"));

                // Tabla
                iTextSharp.text.pdf.PdfPTable table = new iTextSharp.text.pdf.PdfPTable(dgvDatos.Columns.Count);
                table.WidthPercentage = 100;

                // Encabezados
                foreach (DataGridViewColumn col in dgvDatos.Columns)
                {
                    iTextSharp.text.pdf.PdfPCell cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(col.HeaderText, iTextSharp.text.FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD)));
                    cell.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.LightGray);
                    cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    table.AddCell(cell);
                }

                // Datos
                foreach (DataGridViewRow row in dgvDatos.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        string texto = cell.Value?.ToString() ?? "";
                        iTextSharp.text.pdf.PdfPCell pdfCell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(texto, dateFont));
                        pdfCell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                        table.AddCell(pdfCell);
                    }
                }

                doc.Add(table);
                doc.Add(new iTextSharp.text.Paragraph("\n" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));

                doc.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error en PDF: " + ex.Message);
            }
        }

        private void BtnImprimir_Click(object sender, EventArgs e)
        {
            if (dgvDatos.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para imprimir", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PrintDialog pd = new PrintDialog();

            if (pd.ShowDialog() == DialogResult.OK)
            {
                System.Drawing.Printing.PrintDocument printDoc =
                    new System.Drawing.Printing.PrintDocument();

                printDoc.PrintPage += (s, ev) => ImprimirPagina(s, ev);

                printDoc.Print();

                MessageBox.Show(
                    "✓ Documento enviado a la impresora",
                    "Éxito",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        void ImprimirPagina(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            System.Drawing.Font titleFont = new System.Drawing.Font("Arial", 16, FontStyle.Bold);
            System.Drawing.Font headerFont = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
            System.Drawing.Font dataFont = new System.Drawing.Font("Arial", 9);

            int y = 50;
            int x = 50;
            int columnWidth = 120;

            // Título
            e.Graphics.DrawString($"REPORTE: {tipoReporteActual}", titleFont, Brushes.Black, new PointF(x, y));
            y += 40;

            // Fecha
            e.Graphics.DrawString($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}", dataFont, Brushes.Black, new PointF(x, y));
            y += 30;

            // Encabezados
            foreach (DataGridViewColumn col in dgvDatos.Columns)
            {
                e.Graphics.DrawString(col.HeaderText, headerFont, Brushes.Black, new PointF(x, y));
                x += columnWidth;
            }

            y += 20;

            // Datos (primeras 30 filas)
            int maxRows = Math.Min(30, dgvDatos.Rows.Count);
            for (int i = 0; i < maxRows; i++)
            {
                x = 50;
                foreach (DataGridViewCell cell in dgvDatos.Rows[i].Cells)
                {
                    string texto = cell.Value?.ToString() ?? "";
                    e.Graphics.DrawString(texto, dataFont, Brushes.Black, new PointF(x, y));
                    x += columnWidth;
                }
                y += 20;

                if (y > 700)
                {
                    e.HasMorePages = true;
                    return;
                }
            }

            e.HasMorePages = false;
        }

        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            if (tipoReporteActual == "")
            {
                MessageBox.Show("Selecciona un reporte para actualizar", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            switch (tipoReporteActual)
            {
                case "CLIENTES":
                    CargarReporteClientes();
                    break;
                case "DESTINOS":
                    CargarReporteDestinos();
                    break;
                case "VUELOS":
                    CargarReporteVuelos();
                    break;
                case "RESERVAS":
                    CargarReporteReservas();
                    break;
                case "VENTAS":
                    CargarReporteVentas();
                    break;
                case "OCUPACION":
                    CargarReporteOcupacion();
                    break;
                case "DESTINOS_POPULARES":
                    CargarReporteDestinosPopulares();
                    break;
                case "VUELOS_DIA":
                    CargarReporteVuelosDia();
                    break;
                case "ALERTAS":
                    CargarReporteAlertas();
                    break;
            }
        }

        #endregion
    }
}