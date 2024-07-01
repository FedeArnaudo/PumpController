using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

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
            if (!Controlador.Init(Configuracion.LeerConfiguracion()))
            {

                // TODO: Borrar archivo config para que no abra de vuelta.

                //btnCerrar_Click(null, null); // Cerrar
                _ = Log.Instance.WriteLog("Error en la lectura del archivo inicial", Log.LogType.t_info);
            }
            else
            {
                _ = Log.Instance.WriteLog("Configuración leída correctamente", Log.LogType.t_info);
            }
        }
        private void VerConfig_Closed(object sender, EventArgs e)
        {
            IsEnabled = true;
            Show();
            _ = Activate();
            Init();
        }
        private void btnVerDespacho_Click(object sender, RoutedEventArgs e)
        {
            VerDespacho verDespacho = new VerDespacho();
            verDespacho.Show();
        }
        private void btnCambiarConfig_Click(object sender, RoutedEventArgs e)
        {
            VerConfig verConfig = new VerConfig();
            verConfig.Show();
        }
        private void btnVerConfigEstacion_Click(object sender, RoutedEventArgs e)
        {
            VerSurtidores verSurtidores = new VerSurtidores();
            verSurtidores.Show();
        }
        private void btnVerTanques_Click(object sender, RoutedEventArgs e)
        {
            VerTanques verTanques = new VerTanques();
            verTanques.Show();
        }
        private void SetupNotifyIcon()
        {
            notifyIcon = new NotifyIcon
            {
                Icon = new Icon("LogoSurtidor.ico"),
                Visible = true,
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

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Controlador.Stop();
            notifyIcon.Dispose();
            base.OnClosed(e);
            Close();
        }
    }
}
