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
    public partial class frmMenu : Form
    {
        public frmMenu()
        {
            InitializeComponent();
            personalizarDiseño();
            tamanoOriginal = this.Size;
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

            int anchoTotal = pnlLogo.Width + frmHijo.Width + 20;
            int altoTotal = frmHijo.Height + 60;
            this.Size = new Size(anchoTotal, altoTotal);

            frmHijo.FormClosed += (s, args) =>
            {
                this.Size = tamanoOriginal;
            };
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
    }
}
