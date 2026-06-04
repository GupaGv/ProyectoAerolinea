using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProyectoAerolinea.Formularios
{
    public partial class Aerolink : Form
    {
        public Aerolink()
        {
            InitializeComponent();
            personalizarDiseño();
            tamanoOriginal = this.Size;
            this.WindowState = FormWindowState.Maximized;
        }
        Size tamanoOriginal;
        private void personalizarDiseño()
        {
            pnlSubMenuCliente.Visible = false;
            pnlSubMenuDestino.Visible = false;
            pnlSubMenuVuelos.Visible = false;
            pnlSubMenuReservas.Visible = false;
        }

        private void ocultarSubMenu()
        {
            if (pnlSubMenuCliente.Visible == true)
                pnlSubMenuCliente.Visible = false;
            if (pnlSubMenuDestino.Visible == true)
                pnlSubMenuDestino.Visible = false;
            if (pnlSubMenuVuelos.Visible == true)
                pnlSubMenuVuelos.Visible = false;
            if (pnlSubMenuReservas.Visible == true)
                pnlSubMenuReservas.Visible = false;
        }

        private void mostrarSubMenu(Panel subMenu)
        {
            if (subMenu.Visible == false)
            {
                ocultarSubMenu();
                subMenu.Visible = true;
            }
            else
                subMenu.Visible = false;
        }

        private Form activeForm = null;

        private void AbrirenPanel(Form frmHijo)
        {
            if (activeForm != null)
                activeForm.Close();

            activeForm = frmHijo;
            frmHijo.TopLevel = false;
            frmHijo.FormBorderStyle = FormBorderStyle.None;
            frmHijo.Dock = DockStyle.Fill;
            pnlCentral.Controls.Add(frmHijo);
            pnlCentral.Tag = frmHijo;
            frmHijo.BringToFront();
            frmHijo.Show();
        }

        // Botones principales
        private void btnCliente_Click(object sender, EventArgs e)
        {
            mostrarSubMenu(pnlSubMenuCliente);
        }
        private void btnDestino_Click(object sender, EventArgs e)
        {
            mostrarSubMenu(pnlSubMenuDestino);
        }

        private void btnVuelo_Click(object sender, EventArgs e)
        {
            mostrarSubMenu(pnlSubMenuVuelos);
        }

        private void btnReservas_Click(object sender, EventArgs e)
        {
            mostrarSubMenu(pnlSubMenuReservas);
        }
        private void btnReportes_Click(object sender, EventArgs e)
        {
            AbrirenPanel(new frmReportes());
            ocultarSubMenu();
        }


        // Ingreso de datos

        private void btnIngresoCl_Click(object sender, EventArgs e)
        {
            AbrirenPanel(new frmCliente());
            ocultarSubMenu();
        }

        private void btnIngresoDes_Click(object sender, EventArgs e)
        {
            AbrirenPanel(new frmDestino());
            ocultarSubMenu();  
        }

        private void btnIngresoVu_Click(object sender, EventArgs e)
        {
            AbrirenPanel(new frmVuelo());
            ocultarSubMenu();
        }

        private void btnIngresoRe_Click(object sender, EventArgs e)
        {
            AbrirenPanel(new frmReservas());
            ocultarSubMenu();
        }

        // Informes 
        private void btnInformeCl_Click(object sender, EventArgs e)
        {
            AbrirenPanel(new frmInformeCliente());
            ocultarSubMenu();
        }

        private void btnInformeDes_Click(object sender, EventArgs e)
        {
            AbrirenPanel(new frmInformeDestino());
            ocultarSubMenu();
        }

        private void btnInformeVu_Click(object sender, EventArgs e)
        {
            AbrirenPanel(new frmInformeVuelo());
            ocultarSubMenu();
        }

        private void btnInformeRe_Click(object sender, EventArgs e)
        {
            AbrirenPanel(new frmInformeReservas());
            ocultarSubMenu();
        }
    }
}
