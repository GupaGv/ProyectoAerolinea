using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace ProyectoAerolinea.Clases
{
    internal class cConexion
    {
        // Cadena de conexión a la base de datos
        static private string CadenaConexion = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='C:\Users\juanp\Desktop\Trabajos Universidad\Sexto Semestre\Programacion Orientada a Objetos\ProyectoAerolinea\dbAerolinea.mdf';Integrated Security=True;Connect Timeout=30";

        // Objeto de conexión a la base de datos
        private SqlConnection Conexion = new SqlConnection(CadenaConexion);

        // Metodo para abrir la base de datos
        public SqlConnection AbrirConexion()
        {
            if (Conexion.State == ConnectionState.Closed) Conexion.Open();
            return Conexion;
        }

        // Metodo para cerrar la base de datos
        public SqlConnection CerrarConexion()
        {
            if (Conexion.State == ConnectionState.Open) Conexion.Close();
            return Conexion;
        }
    }
}
