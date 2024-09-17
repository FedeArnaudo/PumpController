using System;
using System.IO;

namespace PumpController
{
    internal class Configuracion
    {
        public class InfoConfig
        {
            public InfoConfig()
            {
                ProyNuevoRuta = "";
                IpControlador = "";
                TipoControlador = "";
                Protocolo = 0;
                Timer = 0;
                InfoLog = "";
            }
            public string ProyNuevoRuta { get; set; }
            public string IpControlador { get; set; }
            public string TipoControlador { get; set; }
            public int Protocolo { get; set; }
            public int Timer { get; set; }
            public string InfoLog { get; set; }
        }
        private static readonly string configFile = Environment.CurrentDirectory + "/config.ini";
        public static InfoConfig LeerConfiguracion()
        {
            InfoConfig infoConfig;
            try
            {
                StreamReader reader;
                try
                {
                    reader = new StreamReader(configFile);
                }
                catch (Exception e)
                {
                    _ = Log.Instance.WriteLog("Error al leer archivo de configuración. Excepción: " + e.Message, Log.LogType.t_error);
                    return null;
                }

                infoConfig = new InfoConfig
                {
                    ProyNuevoRuta = reader.ReadLine().Trim(),
                    IpControlador = reader.ReadLine().Trim(),
                    TipoControlador = reader.ReadLine(),
                    Protocolo = Convert.ToInt32(reader.ReadLine()),
                    Timer = Convert.ToInt32(reader.ReadLine()),
                    InfoLog = reader.ReadLine()
                };

                reader.Close();
            }
            catch (Exception e)
            {
                _ = Log.Instance.WriteLog("Error al leer archivo de configuración. Formato incorrecto. Excepción: " + e.Message, Log.LogType.t_error);
                return null;
            }
            return infoConfig;
        }
        public static bool GuardarConfiguracion(InfoConfig infoConfig)
        {
            try
            {
                //Crea el archivo config.ini
                using (StreamWriter outputFile = new StreamWriter(configFile, false))
                {
                    outputFile.WriteLine(infoConfig.ProyNuevoRuta.Trim());
                    outputFile.WriteLine(infoConfig.IpControlador.Trim());
                    outputFile.WriteLine(infoConfig.TipoControlador.ToString());
                    outputFile.WriteLine(infoConfig.Protocolo.ToString());
                    outputFile.WriteLine(infoConfig.Timer.ToString());
                    outputFile.WriteLine(infoConfig.InfoLog.ToString());
                }
            }
            catch (Exception e)
            {
                _ = Log.Instance.WriteLog("Error al guardar la configuración. Excepción: " + e.Message, Log.LogType.t_error);
                return false;
            }
            return true;
        }
        public static bool ExisteConfiguracion()
        {
            return File.Exists(configFile);
        }

    }
}
