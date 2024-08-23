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
            LeerConfiguracion();
        }

        private void BtnGUARDAR_Click(object sender, RoutedEventArgs e)
        {
            //Crea el archivo config.ini
            using (StreamWriter outputFile = new StreamWriter(txtCIO, false))
            {
                outputFile.WriteLine($"VOX: {vox_TextBox.Text.Trim()}");
                outputFile.WriteLine($"BRIDGE: {bridge_TextBox.Text.Trim()}");
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
                        vox_TextBox.Text = reader.ReadLine().Trim().Substring(5);
                        bridge_TextBox.Text = reader.ReadLine().Trim().Substring(8);
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
