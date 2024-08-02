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

namespace PumpController
{
    /// <summary>
    /// Lógica de interacción para VerProductos.xaml
    /// </summary>
    public partial class VerProductos : Window
    {
        public VerProductos()
        {
            InitializeComponent();
            Icon = new BitmapImage(new Uri("pack://application:,,,/PumpController;component/LogoSiges.ico"));
            DataTable result = ConectorSQLite.Dt_query("SELECT numero_producto, numero_despacho, producto, precio FROM Productos");
            DataGridDatos.ItemsSource = result.AsDataView();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
