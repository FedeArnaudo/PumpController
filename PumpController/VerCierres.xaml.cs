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
    /// Lógica de interacción para VerCierres.xaml
    /// </summary>
    public partial class VerCierres : Window
    {
        public VerCierres()
        {
            InitializeComponent();
            Icon = new BitmapImage(new Uri("pack://application:,,,/PumpController;component/Images/LogoSurtidor.ico"));
            Loaded += VerCierres_Loaded;
        }

        private void VerCierres_Loaded(object sender, RoutedEventArgs e)
        {
            DataTable result = ConectorSQLite.Dt_query("SELECT id, fecha, monto_contado, volumen_contado, monto_YPFruta, volumen_YPFruta from Cierres");
            DataGridDatos.ItemsSource = result.AsDataView();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
