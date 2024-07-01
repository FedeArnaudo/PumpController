using System;
using System.Data;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PumpController
{
    /// <summary>
    /// Lógica de interacción para Window1.xaml
    /// </summary>
    public partial class VerTanques : Window
    {
        public VerTanques()
        {
            InitializeComponent();
            Icon = new BitmapImage(new Uri("pack://application:,,,/PumpController;component/LogoSiges.ico"));
            DataTable result = ConectorSQLite.Dt_query("SELECT * FROM Tanques");
            DataGridDatos.ItemsSource = result.AsDataView();
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
