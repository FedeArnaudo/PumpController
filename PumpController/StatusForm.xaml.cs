using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ContextMenu = System.Windows.Forms.ContextMenu;
using MessageBox = System.Windows.MessageBox;

namespace PumpController
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class StatusForm : Window
    {
        private NotifyIcon notifyIcon;


        public StatusForm()
        {
            BuscarPrograma("PumpController");
            InitializeComponent();
            Icon = new BitmapImage(new Uri("pack://application:,,,/PumpController;component/LogoSiges.ico"));
            SetupNotifyIcon();
            Loaded += new RoutedEventHandler(StatusForm_Load);
        }
        private void StatusForm_Load(object sender, EventArgs e)
        {
            if (!Configuracion.ExisteConfiguracion())
            {
                VerConfig verConfig = new VerConfig
                {
                    Owner = this
                };
                verConfig.Closed += VerConfig_Closed;
                verConfig.Show();
                Hide();

                IsEnabled = false;
            }
            else
            {
                Init();
            }
        }
        private void Init()
        {
            Controlador.StatusForm = this;
            if (!Controlador.Init(Configuracion.LeerConfiguracion()))
            {
                _ = Log.Instance.WriteLog("Error en la lectura del archivo inicial", Log.LogType.t_info);
                Close();
            }
            else
            {
                _ = Log.Instance.WriteLog("Configuración leída correctamente.\n", Log.LogType.t_info);
            }
        }
        private void VerConfig_Closed(object sender, EventArgs e)
        {
            IsEnabled = true;
            Show();
            _ = Activate();
            Init();
        }
        private void BtnVerDespacho_Click(object sender, RoutedEventArgs e)
        {
            VerDespachos verDespachos = new VerDespachos();
            verDespachos.Show();
        }
        private void BtnCambiarConfig_Click(object sender, RoutedEventArgs e)
        {
            VerConfig verConfig = new VerConfig();
            verConfig.Show();
            Init();
        }
        private void BtnVerConfigEstacion_Click(object sender, RoutedEventArgs e)
        {
            VerSurtidores verSurtidores = new VerSurtidores();
            verSurtidores.Show();
        }
        private void BtnVerTanques_Click(object sender, RoutedEventArgs e)
        {
            VerTanques verTanques = new VerTanques();
            verTanques.Show();
        }
        private void BtnVerProductos_Click(object sender, RoutedEventArgs e)
        {
            VerProductos verProductos = new VerProductos();
            verProductos.Show();
        }
        private void SetupNotifyIcon()
        {
            notifyIcon = new NotifyIcon
            {
                Icon = new Icon("LogoSurtidor.ico"),
                Visible = false,
                Text = "Controlador De Surtidores"
            };

            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            ContextMenu contextMenu = new ContextMenu();
            _ = contextMenu.MenuItems.Add("Restaurar", (s, e) => RestoreFromTray());
            _ = contextMenu.MenuItems.Add("Salir", (s, e) => ExitApplication());

            notifyIcon.ContextMenu = contextMenu;
        }
        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            RestoreFromTray();
        }
        private void RestoreFromTray()
        {
            Show();
            WindowState = WindowState.Normal;
            notifyIcon.Visible = false;
        }
        private void ExitApplication()
        {
            notifyIcon.Visible = false;
            System.Windows.Application.Current.Shutdown();
        }
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized)
            {
                Hide();
                notifyIcon.Visible = true;
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            notifyIcon.Dispose();
            base.OnClosed(e);
        }
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            // Mostrar un cuadro de diálogo de confirmación
            MessageBoxResult result = MessageBox.Show("¿Está seguro de que desea realizar esta acción?\n      El programa dejará de funcionar.",
                                                      "Confirmación",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);
            // Verificar la respuesta del usuario
            if (result == MessageBoxResult.Yes)
            {
                Controlador.Stop();
                notifyIcon.Dispose();
                base.OnClosed(e);
                Close();
            }
        }

        private void BtnCierre_Anterior_Click(object sender, RoutedEventArgs e)
        {
            Controlador.pedirCierreAnterior = true;
        }
        public void BuscarPrograma(string prog)
        {
            try
            {
                Process[] pname = Process.GetProcessesByName(prog);
                if (pname.Length > 1)
                {
                    _ = MessageBox.Show("El programa ya esta abierto");
                    Process[] procesos = Process.GetProcessesByName(prog);
                    procesos[0].Kill();
                }

            }
            catch (Exception e)
            {
                _ = Log.Instance.WriteLog($"Error buscando el programa existente. Excepcion: {e.Message}", Log.LogType.t_error);
            }
        }

        private void BtnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        public void UpdateLabel(string newLabelContent)
        {
            LabelState.Content = newLabelContent;
        }

        private void BtnIP_Click(object sender, RoutedEventArgs e)
        {
            VerIPs verIPs = new VerIPs();
            verIPs.Show();
        }
    }
}
