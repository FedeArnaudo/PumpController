using System;
using System.Data;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PumpController
{
    /// <summary>
    /// Lógica de interacción para VerTanques.xaml
    /// </summary>
    public partial class VerTanques : Window
    {
        public VerTanques()
        {
            InitializeComponent();
            Icon = new BitmapImage(new Uri("pack://application:,,,/PumpController;component/Images/LogoSurtidor.ico"));
            DataTable result = ConectorSQLite.Dt_query("SELECT * FROM Tanques");
            DataGridDatos.ItemsSource = result.AsDataView();
        }
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
