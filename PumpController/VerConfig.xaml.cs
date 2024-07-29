using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CheckBox = System.Windows.Controls.CheckBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.Forms.MessageBox;

namespace PumpController
{
    /// <summary>
    /// Lógica de interacción para VerConfig.xaml
    /// </summary>
    public partial class VerConfig : Window
    {
        private IShowInfo showInfo;
        public VerConfig()
        {
            InitializeComponent();
            Icon = new BitmapImage(new Uri("pack://application:,,,/PumpController;component/LogoSiges.ico"));
            //Loaded += new RoutedEventHandler(ConfiguracionForm_Load);
        }
        private void ConfiguracionForm_Load(object sender, EventArgs e)
        {
            if (Configuracion.ExisteConfiguracion())
            {
                Configuracion.InfoConfig infoConfig = Configuracion.LeerConfiguracion();

                TextBoxPryNuevo.Text = infoConfig.ProyNuevoRuta;
                TextBoxIP.Text = infoConfig.IpControlador;
                ComboBoxTipo.Text = infoConfig.TipoControlador;
                if (infoConfig.Protocolo == 16)
                {
                    CheckBoxProtocol16.IsChecked = true;
                }
                else
                {
                    CheckBoxProtocol32.IsChecked = true;
                }
                ComboBoxMode.Text = Log.LogType.t_info.ToString();
                if (infoConfig.InfoLog.Equals(Log.LogType.t_debug))
                {
                    ComboBoxMode.Text = infoConfig.InfoLog;
                }
                StatusForm statusForm = new StatusForm
                {
                    Owner = this
                };
                Hide();
                _ = statusForm.ShowDialog();
                statusForm.Show();
            }
        }
        private void BtnConfig_Click(object sender, RoutedEventArgs e)
        {
            Configuracion.InfoConfig infoConfig = new Configuracion.InfoConfig();
            bool completado = false;

            if (TextBoxPryNuevo.Text != "")
            {
                infoConfig.ProyNuevoRuta = TextBoxPryNuevo.Text;
                if (TextBoxIP.Text != "")
                {
                    infoConfig.IpControlador = TextBoxIP.Text;
                    if (ComboBoxTipo.Text != null && ComboBoxTipo.Text != "")
                    {
                        infoConfig.TipoControlador = ComboBoxTipo.Text;
                        infoConfig.InfoLog = ComboBoxMode.Text;
                        if (CheckBoxProtocol16.IsChecked != false)
                        {
                            infoConfig.Protocolo = 16;
                            completado = true;
                        }
                        else if (CheckBoxProtocol32.IsChecked != false)
                        {
                            infoConfig.Protocolo = 32;
                            completado = true;
                        }
                        else
                        {
                            _ = MessageBox.Show("Debe ingresar el Protocolo de la Estacion");
                        }

                        if (completado && Configuracion.GuardarConfiguracion(infoConfig))
                        {
                            _ = Log.Instance.WriteLog("La configuración se guardó correctamente", Log.LogType.t_info);

                            Log.Instance.SetLogLevel(Log.LogType.t_info);
                            if (ComboBoxMode.Text.Equals(Log.LogType.t_debug.ToString()))
                            {
                                Log.Instance.SetLogLevel(Log.LogType.t_debug);
                            }

                            _ = Log.Instance.WriteLog($"Modo: {Log.Instance.GetLogLevel()}\n", Log.LogType.t_info);
                            _ = MessageBox.Show("Configuracón guardada correctamente");
                            Close();
                        }
                    }
                    else
                    {
                        _ = MessageBox.Show("Debe seleccionar un Tipo de Controlador");
                    }
                }
                else
                {
                    _ = MessageBox.Show("Debe ingresar la IP del Controlador");
                }
            }
            else
            {
                _ = MessageBox.Show("Debe ingresar la ruta del proyecto");
            }
        }

        private void CheckBoxProtocol16_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox CheckBoxProtocol16 = (CheckBox)sender;
            CheckBoxProtocol16.IsChecked = true;
            CheckBoxProtocol32.IsChecked = false;
        }

        private void CheckBoxProtocol16_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox CheckBoxProtocol16 = (CheckBox)sender;
            CheckBoxProtocol16.IsChecked = false;
            CheckBoxProtocol32.IsChecked = true;
        }

        private void CheckBoxProtocol32_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox CheckBoxProtocol32 = (CheckBox)sender;
            CheckBoxProtocol32.IsChecked = true;
            CheckBoxProtocol16.IsChecked = false;
        }

        private void CheckBoxProtocol32_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox CheckBoxProtocol32 = (CheckBox)sender;
            CheckBoxProtocol32.IsChecked = false;
            CheckBoxProtocol16.IsChecked = true;
        }

        private void BtnRutaPN_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.MyComputer, // Carpeta raíz (opcional)
                Description = "Selecciona una carpeta" // Descripción del diálogo (opcional)
            };

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string folderPath = folderBrowserDialog.SelectedPath;
                TextBoxPryNuevo.Text = folderPath;
            }
        }

        private void BtnInfoIP_Click(object sender, RoutedEventArgs e)
        {
            showInfo = new IdControladorInfo();
            _ = MessageBox.Show(showInfo.ShowInfo());
        }

        private void BtnInfoProtocolo_Click(object sender, RoutedEventArgs e)
        {
            showInfo = new ProtocoloInfo();
            _ = MessageBox.Show(showInfo.ShowInfo());
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Combinación de teclas Ctrl + Shift + E
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    if (e.Key == Key.E)
                    {
                        ComboBoxMode.Visibility = Visibility.Visible;
                    }
                }
            }
        }
    }
}
