using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SQLite;

namespace PumpController
{
    /// <summary>
    /// Lógica de interacción para VerDespachos.xaml
    /// </summary>
    public partial class VerDespachos : Window
    {
        public VerDespachos()
        {
            InitializeComponent();
            Icon = new BitmapImage(new Uri("pack://application:,,,/PumpController;component/Images/LogoSurtidor.ico"));

            DataTable result = ConectorSQLite.Dt_query("SELECT * FROM Despachos ORDER BY fecha DESC");

            List<InfoDespacho> infoDespachos = new List<InfoDespacho>();

            foreach (DataRow row in result.Rows)
            {
                //string fact = Encoding.ASCII.GetString((byte[])row["facturado"]);
                InfoDespacho infoDespacho = new InfoDespacho
                {
                    ID = Convert.ToInt32(row["id"]),
                    Surtidor = Convert.ToInt32(row["surtidor"]),
                    Manguera = row["manguera"].ToString(),
                    Producto = Convert.ToInt32(row["producto"]),
                    Monto = Convert.ToDouble(row["monto"]),
                    Volumen = Convert.ToDouble(row["volumen"]),
                    PPU = Convert.ToDouble(row["PPU"]),
                    Facturado = Convert.ToBoolean(Convert.ToInt16(Encoding.ASCII.GetString((byte[])row["facturado"]))),
                    YPFRuta = Convert.ToBoolean(Convert.ToInt16(Encoding.ASCII.GetString((byte[])row["YPFruta"]))),
                    Desc = row["descripcion"].ToString(),
                    Fecha = Convert.ToString(Convert.ToDateTime(row["fecha"]))
                };
                infoDespachos.Add(infoDespacho);
            }
            DataGridDatos.ItemsSource = infoDespachos;
        }
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
