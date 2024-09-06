using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

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
            Icon = new BitmapImage(new Uri("pack://application:,,,/PumpController;component/Images/LogoSurtidor.ico"));
            Loaded += new RoutedEventHandler(LeerConfiguracion);
        }

        private void BtnGUARDAR_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                //Crea el archivo CIO.txt
                using (StreamWriter outputFile = new StreamWriter(txtCIO, false))
                {
                    /*if (string.IsNullOrEmpty(ipVox_TextBox.Text))
                    {
                        ipVox_TextBox.Text = "sin informacion";
                    }
                    else if (string.IsNullOrEmpty(ipBridge_TextBox.Text))
                    {
                        ipBridge_TextBox.Text = "sin informacion";
                    }
                    else if (string.IsNullOrEmpty(ipServer_TextBox.Text))
                    {
                        ipServer_TextBox.Text = "sin informacion";
                    }
                    else if (string.IsNullOrEmpty(ipLibre_TextBox.Text))
                    {
                        ipLibre_TextBox.Text = "sin informacion";
                    }
                    else if (string.IsNullOrEmpty(ruteoEstatico_TextBox.Text))
                    {
                        ruteoEstatico_TextBox.Text = "sin informacion";
                    }*/
                    outputFile.WriteLine($"IP VOX:        {ipVox_TextBox.Text.Trim()}");
                    outputFile.WriteLine($"IP BRIDGE:     {ipBridge_TextBox.Text.Trim()}");
                    outputFile.WriteLine($"IP SERVER:     {ipServer_TextBox.Text.Trim()}");
                    outputFile.WriteLine($"IP LIBRE:      {ipLibre_TextBox.Text.Trim()}");
                    outputFile.WriteLine($"RUTEO ESTATICO:{ruteoEstatico_TextBox.Text.Trim()}");
                }
                Close();
                _ = ConectorSQLite.Query("UPDATE Datos_CIO " +
                                              $"SET ip_vox = '{ipVox_TextBox.Text}', ip_bridge = '{ipBridge_TextBox.Text}', " +
                                              $"ip_server = '{ipServer_TextBox.Text}', ip_libre = '{ipLibre_TextBox.Text}', " +
                                              $"ruteo_estatico = '{ruteoEstatico_TextBox.Text}' " +
                                              $"WHERE id_cio = {1}");
            }
            catch (Exception e)
            {
                _ = Log.Instance.WriteLog("Error al guardar la configuración. Excepción: " + e.Message, Log.LogType.t_error);
            }
        }

        private bool ExisteConfiguracion()
        {
            return File.Exists(txtCIO);
        }

        public void LeerConfiguracion(object sender, EventArgs eventsArgs)
        {
            if (ExisteConfiguracion())
            {
                // Lee todas las líneas del archivo
                string[] lines = File.ReadAllLines(txtCIO);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line;
                    try
                    {
                        line = lines[i].Trim().Substring(15);
                        lines[i] = !string.IsNullOrEmpty(line) ? line : "";
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        line = "";
                    }
                    switch (i)
                    {
                        case 0:
                            ipVox_TextBox.Text = line;
                            break;
                        case 1:
                            ipBridge_TextBox.Text = line;
                            break;
                        case 2:
                            ipServer_TextBox.Text = line;
                            break;
                        case 3:
                            ipLibre_TextBox.Text = line;
                            break;
                        default:
                            ruteoEstatico_TextBox.Text = line;
                            break;
                    }
                }
            }
        }
    }
}
