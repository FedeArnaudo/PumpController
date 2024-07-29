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
    /// Lógica de interacción para VerTanques.xaml
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
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
