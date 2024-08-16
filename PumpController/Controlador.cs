using System;
using System.Threading;
using System.Threading.Tasks;

namespace PumpController
{
    internal abstract class Controlador
    {
        // Instancia de Singleton
        private static Controlador instancia = null;
        // Hilo para manejar el proceso principal de consulta al controlador en paralelo
        // al resto de la ejecución
        private static readonly Thread procesoPrincipal = null;

        // Tiempo de espera entre cada procesamiento en segundos.
        private static readonly int loopDelaySeconds = 2;

        public static bool endWork = false;
        public static bool pedirCierreAnterior = false;
        private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public Controlador() { }
        public static StatusForm StatusForm { get; set; }
        private void UpdateLabelContent(string newContent)
        {
            //StatusForm.UpdateLabel(newContent);
            StatusForm.LabelState.Content = newContent;
        }
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
                Log.Instance.SetLogLevel(Log.LogType.t_info);
                if (infoConfig.InfoLog.Equals(Log.LogType.t_debug.ToString()))
                {
                    Log.Instance.SetLogLevel(Log.LogType.t_debug);
                }
                ConectorSQLite.CrearBBDD();
                switch (infoConfig.TipoControlador)
                {
                    case "CEM-44":
                        instancia = new ControladorCEM();
                        break;
                    default:
                        break;
                }
                if (procesoPrincipal == null || !procesoPrincipal.IsAlive)
                {
                    _ = Task.Run(() => Run(cancellationTokenSource.Token));
                }
            }
            else if (infoConfig == null)
            {
                return false;
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
                        StatusForm.LabelState.Dispatcher.Invoke(() =>
                        {
                            StatusForm.LabelState.Content = "Controlador\nOnLine";
                        });

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
                    _ = Log.Instance.WriteLog($"  Error en el loop del controlador.\n\t  Excepción: {e.Message}\n", Log.LogType.t_error);
                    StatusForm.LabelState.Dispatcher.Invoke(() =>
                    {
                        StatusForm.LabelState.Content = "Controlador\nDesconectado";
                    });

                }
            }
        }
        public static void Stop()
        {
            cancellationTokenSource.Cancel();
        }
    }
}
