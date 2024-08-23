using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for VerIPs.xaml
    /// </summary>
    public partial class VerIPs : Window
    {
        private static readonly string txtCIO = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/CIO.txt";
        public VerIPs()
        {
            InitializeComponent();
            Icon = new BitmapImage(new Uri("pack://application:,,,/PumpController;component/LogoSurtidor.ico"));
            LeerConfiguracion();
        }

        private void BtnGUARDAR_Click(object sender, RoutedEventArgs e)
        {
            //Crea el archivo config.ini
            using (StreamWriter outputFile = new StreamWriter(txtCIO, false))
            {
                outputFile.WriteLine($"IP VOX: {ipVox_TextBox.Text.Trim()}");
                outputFile.WriteLine($"IP BRIDGE: {ipBridge_TextBox.Text.Trim()}");
                outputFile.WriteLine($"IP SERVER: {ipServer_TextBox.Text.Trim()}");
                outputFile.WriteLine($"IP LIBRE: {ipLibre_TextBox.Text.Trim()}");
                outputFile.WriteLine($"RUTEO ESTATICO: {RuteoEstatico_TextBox.Text.Trim()}");
            }
            Close();
        }

        private bool ExisteConfiguracion()
        {
            return File.Exists(txtCIO);
        }

        public void LeerConfiguracion()
        {
            if (ExisteConfiguracion())
            {
                string content = File.ReadAllText(txtCIO);
                StreamReader reader;
                try
                {
                    reader = new StreamReader(txtCIO);

                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        ipVox_TextBox.Text = reader.ReadLine().Trim().Substring(8);
                        ipBridge_TextBox.Text = reader.ReadLine().Trim().Substring(11);
                        ipServer_TextBox.Text = reader.ReadLine().Trim().Substring(11);
                        ipLibre_TextBox.Text = reader.ReadLine().Trim().Substring(10);
                        RuteoEstatico_TextBox.Text = reader.ReadLine().Trim().Substring(16);
                    }
                    reader.Close();
                }
                catch (Exception e)
                {
                    _ = Log.Instance.WriteLog($"Error al leer {txtCIO}. Excepción: " + e.Message, Log.LogType.t_error);
                }
            }
        }
    }
}
