using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace PumpController
{
    internal abstract class Controlador
    {
        // Instancia de Singleton
        private static Controlador instancia = null;
        // Hilo para manejar el proceso principal de consulta al controlador en paralelo al resto de la ejecución
        private static Task procesoPrincipal = null;
        //private static 

        // Tiempo de espera entre cada procesamiento en segundos.
        private static int loopDelaySeconds;

        public static bool endWork = false;
        public static bool pedirCierreAnterior = false;
        private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public Controlador() { }
        public static StatusForm StatusForm { get; set; }

        /// <summary>
        /// Este método estático es el encargado de configurar la estructura de la estacion,
        /// para obtener los productos, los tanques, las mangueras y surtidores, etc.
        /// Y se guarda la informacion en la tabla de la base de datos correspondiente.
        /// </summary>
        public abstract void GrabarConfigEstacion();

        /// <summary>
        /// Este método estático es el encargado de procesar la informacion de los surtidores
        /// y guardarla en la tabla de la base de datos correspondiente.
        /// </summary>
        public abstract void GrabarDespachos();
        /// <summary>
        /// Este método estático es el encargado de procesar la informacion de los tanques
        /// y guardarla en la tabla de la base de datos correspondiente.
        /// </summary>
        public abstract void GrabarTanques();
        /// <summary>
        /// Este método estático es el encargado de grabar los productos que trae el controlador
        /// y guardarla en la tabla de la base de datos correspondiente.
        /// </summary>
        public abstract void GrabarProductos();
        /// <summary>
        /// Este método estático es el encargado de procesar la informacion del ultimo turno,
        /// sin realizar el corte y guardarla en la tabla de la base de datos correspondiente.
        /// </summary>
        public abstract void GrabarTurnoEnCurso();
        /// <summary>
        /// Este método estático es el encargado de procesar la informacion del corte del ultimo turno
        /// y guardarla en la base de datos correspondiente.
        /// </summary>
        public abstract void GrabarCierreDeTurno();
        /// <summary>
        /// Este método estático es el encargado de procesar la informacion del cierre del turno anterior
        /// y guardarla en la base de datos correspondiente.
        /// </summary>
        public abstract void GrabarCierreAnterior();

        /// <summary>
        /// Este método estático es el encargado de crear la instancia del controlador
        /// correspondiente y ejecutar el hilo del proceso automático
        /// </summary>
        /// <param name="config"> La configuración extraída del archivo de configuración </param>
        /// <returns> true si se pudo inicializar correctamente </returns>
        public static bool Init(Configuracion.InfoConfig infoConfig)
        {
            if (instancia == null && infoConfig != null)
            {
                ConectorSQLite.CrearBBDD();
                Log.Instance.SetLogLevel(Log.LogType.t_info);
                if (infoConfig.InfoLog.Equals(Log.LogType.t_debug.ToString()))
                {
                    Log.Instance.SetLogLevel(Log.LogType.t_debug);
                }
                switch (infoConfig.TipoControlador)
                {
                    case "CEM-44":
                        instancia = new ControladorCEM();
                        loopDelaySeconds = infoConfig.Timer;
                        break;
                    default:
                        break;
                }
            }
            else if (infoConfig == null)
            {
                return false;
            }
            else
            {
                loopDelaySeconds = infoConfig.Timer;
            }
            if (procesoPrincipal == null)
            {
                procesoPrincipal = Task.Run(() => Run(cancellationTokenSource.Token));
            }
            return true;
        }
        private static void Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    instancia.GrabarConfigEstacion();
                    instancia.GrabarTanques();
                    instancia.GrabarProductos();
                    while (!ConectorSQLite.ComprobarCierre())
                    {
                        instancia.GrabarDespachos();

                        /// Espera para procesar nuevamente
                        Thread.Sleep(loopDelaySeconds * 1000);

                        if (token.IsCancellationRequested)
                        {
                            break;
                        }
                        else if (pedirCierreAnterior)
                        {
                            instancia.GrabarCierreAnterior();
                            pedirCierreAnterior = false;
                        }
                    }
                    instancia.GrabarCierreDeTurno();

                    /// Espera para procesar nuevamente
                    Thread.Sleep(loopDelaySeconds * 1000);
                }
                catch (Exception e)
                {
                    _ = Log.Instance.WriteLog($" Estado del hilo: {procesoPrincipal.Status} - Error en el loop del controlador.\n\t  Excepción: {e.Message}\n", Log.LogType.t_error);
                }
            }
        }
        public static void Stop()
        {
            cancellationTokenSource.Cancel();
        }

        public static void CheckConexion(int conexion)
        {
            _ = ConectorSQLite.Query($"UPDATE CheckConexion SET isConnected = {conexion}, fecha = datetime('now', 'localtime') WHERE idConexion = 1");
            if (conexion == 0)// conexion == 0 => exitosa
            {
                StatusForm.LabelState.Dispatcher.Invoke(() =>
                {
                    StatusForm.LabelState.Content = "Controlador\nOnLine";
                    StatusForm.LabelState.Background = new SolidColorBrush(Colors.ForestGreen);
                });
            }
            else// conexion == 1 => fallida
            {
                StatusForm.LabelState.Dispatcher.Invoke(() =>
                {
                    StatusForm.LabelState.Content = "Controlador\nDesconectado";
                    StatusForm.LabelState.Background = new SolidColorBrush(Color.FromRgb(214, 40, 40));
                });
            }
        }
    }
}
