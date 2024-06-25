using System;
using System.Data;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PumpController
{
    /// <summary>
    /// Lógica de interacción para VerDespacho.xaml
    /// </summary>
    public partial class VerDespacho : Window
    {
        public VerDespacho()
        {
            InitializeComponent();
            Icon = new BitmapImage(new Uri("pack://application:,,,/PumpController;component/LogoSiges.ico"));
            DataTable result = ConectorSQLite.Dt_query("SELECT * FROM despachos ORDER BY fecha DESC");
            DataGridDatos.ItemsSource = result.AsDataView();
        }
        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
