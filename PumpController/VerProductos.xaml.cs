using System;
using System.Data;
using System.Windows;
using System.Windows.Media.Imaging;

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
            Icon = new BitmapImage(new Uri("pack://application:,,,/PumpController;component/Images/LogoSurtidor.ico"));
            DataTable result = ConectorSQLite.Dt_query("SELECT numero_producto, numero_despacho, producto, precio FROM Productos");
            DataGridDatos.ItemsSource = result.AsDataView();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
