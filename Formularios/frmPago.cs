using System;
using System.Windows.Forms;

namespace ProyectoAerolinea.Formularios
{
    public partial class frmPago : Form
    {
        public bool PagoConfirmado { get; private set; } = false;
        public string MetodoPago { get; private set; }

        public frmPago(decimal totalAPagar)
        {
            InitializeComponent();

            // Configurar métodos de pago
            cmbMetodoPago.Items.AddRange(new string[] {
                "Tarjeta de Crédito",
                "Tarjeta de Débito",
                "Transferencia Bancaria",
                "Efectivo"
            });
            cmbMetodoPago.SelectedIndex = 0;

            lblTotal.Text = "$" + totalAPagar.ToString("N0");
        }

        private void cmbMetodoPago_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool esTarjeta = cmbMetodoPago.Text == "Tarjeta de Crédito" ||
                 cmbMetodoPago.Text == "Tarjeta de Débito";

            txtNumeroTarjeta.Enabled = esTarjeta;
            txtTitular.Enabled = esTarjeta;
            txtVencimiento.Enabled = esTarjeta;
            txtCVV.Enabled = esTarjeta;
        }

        private void btnConfirmarPago_Click(object sender, EventArgs e)
        {
            if (cmbMetodoPago.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione un método de pago.");
                return;
            }

            bool esTarjeta = cmbMetodoPago.Text == "Tarjeta de Crédito" ||
                             cmbMetodoPago.Text == "Tarjeta de Débito";

            if (esTarjeta)
            {
                if (txtNumeroTarjeta.Text.Trim().Length < 16)
                {
                    MessageBox.Show("Ingrese un número de tarjeta válido (16 dígitos).");
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtTitular.Text))
                {
                    MessageBox.Show("Ingrese el nombre del titular.");
                    return;
                }
                if (txtVencimiento.Text.Trim().Length < 5)
                {
                    MessageBox.Show("Ingrese la fecha de vencimiento (MM/AA).");
                    return;
                }
                if (txtCVV.Text.Trim().Length < 3)
                {
                    MessageBox.Show("Ingrese el CVV.");
                    return;
                }
            }

            PagoConfirmado = true;
            MetodoPago = cmbMetodoPago.Text;
            MessageBox.Show("¡Pago procesado exitosamente!\nMétodo: " + MetodoPago);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            PagoConfirmado = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnConfirmarPago_Click_1(object sender, EventArgs e)
        {
            if (cmbMetodoPago.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione un método de pago.");
                return;
            }

            bool esTarjeta = cmbMetodoPago.Text == "Tarjeta de Crédito" ||
                             cmbMetodoPago.Text == "Tarjeta de Débito";

            if (esTarjeta)
            {
                if (txtNumeroTarjeta.Text.Trim().Length < 16)
                {
                    MessageBox.Show("Ingrese un número de tarjeta válido (16 dígitos).");
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtTitular.Text))
                {
                    MessageBox.Show("Ingrese el nombre del titular.");
                    return;
                }
                if (txtVencimiento.Text.Trim().Length < 5)
                {
                    MessageBox.Show("Ingrese la fecha de vencimiento (MM/AA).");
                    return;
                }
                if (txtCVV.Text.Trim().Length < 3)
                {
                    MessageBox.Show("Ingrese el CVV.");
                    return;
                }
            }

            PagoConfirmado = true;
            MetodoPago = cmbMetodoPago.Text;
            MessageBox.Show("¡Pago procesado exitosamente!\nMétodo: " + MetodoPago);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancelar_Click_1(object sender, EventArgs e)
        {
            PagoConfirmado = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}