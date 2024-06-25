using System;
using System.Threading;
using System.Threading.Tasks;

namespace PumpController
{
    internal abstract class Controlador
    {
        public Controlador()
        {
            
        }

        // Instancia de Singleton
        private static Controlador instancia = null;
        // Hilo para manejar el proceso principal de consulta al controlador en paralelo
        // al resto de la ejecución
        private static Thread procesoPrincipal = null;

        // Mutex para control del hilo del proceso principal
        public static Mutex working = new Mutex();
        // Tiempo de espera entre cada procesamiento en segundos.
        private static readonly int loopDelaySeconds = 2;

        public static bool endWork = false;
        private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public abstract void GrabarConfigEstacion();

        /// <summary>
        /// Este método estático es el encargado de procesar la informacion de los surtidores
        /// y guardarla en latabla de la base de datos, correspondiente
        /// 
        public abstract void GrabarDespachos();

        /// <summary>
        /// Este método estático es el encargado de procesar la informacion de los tanques
        /// y guardarla en latabla de la base de datos, correspondiente
        /// 
        public abstract void GrabarTanques();

        public abstract void GrabarCierre();

        /// <summary>
        /// Este método estático es el encargado de crear la instancia del controlador
        /// correspondiente y ejecutar el hilo del proceso automático
        /// </summary>
        /// <param name="config"> La configuración extraída del archivo de configuración </param>
        /// <returns> true si se pudo inicializar correctamente </returns>
        public static bool Init(Configuracion.InfoConfig infoConfig)
        {
            if (instancia == null)
            {
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
                    //procesoPrincipal = new Thread(new ThreadStart(Run));
                    //procesoPrincipal.Start();
                }
            }
            return true;
        }
        private static void Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    //instancia.GrabarCierre();
                    instancia.GrabarConfigEstacion();
                    instancia.GrabarTanques();
                    while (!ConectorSQLite.ComprobarCierre())
                    {
                        instancia.GrabarDespachos();

                        /// Espera para procesar nuevamente
                        Thread.Sleep(loopDelaySeconds * 1000);

                        if (token.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                    /// Espera para procesar nuevamente
                    Thread.Sleep(loopDelaySeconds * 1000);
                }
                catch (Exception e)
                {
                    _ = Log.Instance.WriteLog("Error en el loop del controlador.\nExcepción: " + e.Message, Log.LogType.t_error);
                }
            }
        }
        public static void Stop()
        {
            cancellationTokenSource.Cancel();
        }
    }
}
