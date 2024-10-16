﻿using Polly;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace PumpController
{
    public class ConectorCEM
    {
        private readonly byte separador = 0x7E;
        private readonly string nombreDelPipe = "CEM44POSPIPE";
        private string ipControlador = Configuracion.LeerConfiguracion().IpControlador;
        private int protocolo = Configuracion.LeerConfiguracion().Protocolo;
        private readonly CultureInfo culture;
        public ConectorCEM()
        {
            // Especifica la cultura que utiliza el punto como separador decimal
            culture = CultureInfo.InvariantCulture;
        }

        public Estacion ConfiguracionDeLaEstacion()
        {
            Estacion estacionTemp = Estacion.InstanciaEstacion;

            byte[] mensaje = protocolo == 16 ? (new byte[] { 0x65 }) : (new byte[] { 0xB5 });
            int confirmacion = 0;
            int surtidores = 1;
            int tanques = 3;
            int productos = 4;

            byte[] respuesta = Log.Instance.GetLogLevel().Equals(Log.LogType.t_debug) ? LeerArchivo("ConfigEstacion") : EnviarComando(mensaje);
            try
            {
                if (respuesta == null || respuesta[confirmacion] != 0x0)
                {
                    throw new Exception("No se recibió mensaje de confirmación al solicitar info de la estación.");
                }
                if (!File.Exists(Environment.CurrentDirectory + "/configEstacion.txt"))
                {
                    GuardarRespuesta(respuesta, "configEstacion.txt");
                }

                estacionTemp.NumeroDeSurtidores = respuesta[surtidores];

                estacionTemp.NumeroDeTanques = respuesta[tanques];

                estacionTemp.NumeroDeProductos = respuesta[productos];

                int posicion = productos + 1;
                List<Producto> tempProductos = new List<Producto>();
                for (int i = 0; i < estacionTemp.NumeroDeProductos; i++)
                {
                    Producto producto = new Producto
                    {
                        NumeroDeProducto = Convert.ToInt16(LeerCampoVariable(respuesta, ref posicion)),
                        PrecioUnitario = ConvertDouble(LeerCampoVariable(respuesta, ref posicion))
                    };
                    DescartarCampoVariable(respuesta, ref posicion);

                    switch (producto.NumeroDeProducto)
                    {
                        case 1:
                            producto.Descripcion = "NAFTA SUPER";
                            break;
                        case 2:
                            producto.Descripcion = "NAFTA NORMAL";
                            break;
                        case 3:
                            producto.Descripcion = "ULTRA DIESEL";
                            break;
                        case 4:
                            producto.Descripcion = "NAFTA INFINIA";
                            break;
                        case 5:
                            producto.Descripcion = "KEROSENE";
                            break;
                        case 6:
                            producto.Descripcion = "INFINIA DIESEL";
                            break;
                        case 7:
                            producto.Descripcion = "GNC";
                            break;
                        case 8:
                            producto.Descripcion = "DIESEL 500";
                            break;
                        case 9:
                            producto.Descripcion = "AZUL-32";
                            break;
                        default:
                            producto.Descripcion = "N/Utilizado";
                            break;
                    }
                    tempProductos.Add(producto);
                }
                estacionTemp.Productos = tempProductos;

                List<Surtidor> tempSurtidores = new List<Surtidor>();
                List<List<Surtidor>> tempNivelesDePrecio = new List<List<Surtidor>>();
                int tempNumeroDeMangueras = 0;
                for (int i = 0; i < estacionTemp.NumeroDeSurtidores; i++)
                {
                    Surtidor surtidor = new Surtidor
                    {
                        NivelDeSurtidor = respuesta[posicion]
                    };
                    posicion++;
                    surtidor.TipoDeSurtidor = respuesta[posicion] + 1;
                    tempNumeroDeMangueras += respuesta[posicion] + 1;
                    posicion++;

                    for (int j = 0; j < surtidor.TipoDeSurtidor; j++)
                    {
                        Manguera manguera = new Manguera
                        {
                            NumeroDeManguera = j + 1
                        };
                        int productoH = respuesta[posicion];
                        posicion++;
                        foreach (Producto producto in tempProductos)
                        {
                            if (producto.NumeroDeProducto.Equals(productoH))
                            {
                                manguera.Producto = producto;
                                break;
                            }
                        }
                        surtidor.Mangueras.Add(manguera);
                    }
                    surtidor.NumeroDeSurtidor = i + 1;
                    tempSurtidores.Add(surtidor);
                }
                tempNivelesDePrecio.Add(tempSurtidores);
                estacionTemp.NumeroDeMangueras = tempNumeroDeMangueras;
                estacionTemp.Surtidores = tempSurtidores;
                estacionTemp.NivelesDePrecio = tempNivelesDePrecio;

                List<Tanque> tempTanques = new List<Tanque>();
                for (int i = 0; i < estacionTemp.NumeroDeTanques; i++)
                {
                    Tanque tanque = new Tanque
                    {
                        NumeroDeTanque = i + 1,
                        ProductoTanque = respuesta[posicion]
                    };
                    posicion++;
                    tempTanques.Add(tanque);
                }
                estacionTemp.Tanques = tempTanques;
            }
            catch (Exception e)
            {
                throw new Exception($"No se pudo obtener información de la estación.\n\t  Excepción: {e.Message}");
            }
            return estacionTemp;
        }
        public List<Tanque> InfoTanques(int cantidadDeTanques)
        {
            byte[] mensaje = protocolo == 16 ? (new byte[] { 0x68 }) : (new byte[] { 0xB8 });
            int confirmacion = 0;

            byte[] respuesta = Log.Instance.GetLogLevel().Equals(Log.LogType.t_debug) ? LeerArchivo("infoTanques") : EnviarComando(mensaje);
            try
            {
                if (respuesta == null || respuesta[confirmacion] != 0x0)
                {
                    throw new Exception("No se recibió mensaje de confirmación al solicitar info del surtidor");
                }
                if (!File.Exists(Environment.CurrentDirectory + "/infoTanques.txt"))
                {
                    GuardarRespuesta(respuesta, "infoTanques.txt");
                }

                int posicion = confirmacion + 1;

                for (int i = 0; i < cantidadDeTanques; i++)
                {
                    foreach (Tanque tanque in Estacion.InstanciaEstacion.Tanques)
                    {
                        if (tanque.NumeroDeTanque == (i + 1))
                        {
                            tanque.NumeroDeTanque = i + 1;
                            tanque.VolumenProductoT = ConvertDouble(LeerCampoVariable(respuesta, ref posicion));
                            tanque.VolumenAguaT = ConvertDouble(LeerCampoVariable(respuesta, ref posicion));
                            tanque.VolumenVacioT = ConvertDouble(LeerCampoVariable(respuesta, ref posicion));
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error al obtener informacion del tanque. \n\tExcepción: " + e.Message);
            }
            return Estacion.InstanciaEstacion.Tanques;
        }
        public Despacho InfoSurtidor(int numeroDeSurtidor)
        {
            byte[] mensaje = protocolo == 16 ? (new byte[] { 0x70 }) : (new byte[] { 0xC0 });
            int confirmacion = 0;
            int status = 1;
            int nro_venta = 2;
            int codigo_producto = 3;

            Despacho despachoTemp = new Despacho();

            if ((protocolo == 16 && numeroDeSurtidor == 16) || (protocolo == 32 && numeroDeSurtidor == 32))
            {
                numeroDeSurtidor = 0;
            }

            byte[] respuesta = Log.Instance.GetLogLevel().Equals(Log.LogType.t_debug) ? LeerArchivo("despacho-" + numeroDeSurtidor) : EnviarComando(new byte[] { (byte)(mensaje[0] + Convert.ToByte(numeroDeSurtidor)) });

            if (respuesta != null && respuesta[confirmacion] == 0x0)
            {
                try
                {
                    if (!File.Exists(Environment.CurrentDirectory + $"/despacho-{numeroDeSurtidor}.txt"))
                    {
                        GuardarRespuesta(respuesta, $"despacho-{numeroDeSurtidor}.txt");
                    }

                    // Proceso ultima venta
                    byte statusUltimaVenta = respuesta[status];

                    bool despachando = false;
                    bool detenido = false;

                    switch (statusUltimaVenta)
                    {
                        case 0x01:
                            despachoTemp.StatusUltimaVenta = Despacho.ESTADO_SURTIDOR.DISPONIBLE;
                            break;
                        case 0x02:
                            despachoTemp.StatusUltimaVenta = Despacho.ESTADO_SURTIDOR.EN_SOLICITUD;
                            break;
                        case 0x03:
                            despachoTemp.StatusUltimaVenta = Despacho.ESTADO_SURTIDOR.DESPACHANDO;
                            despachando = true;
                            break;
                        case 0x04:
                            despachoTemp.StatusUltimaVenta = Despacho.ESTADO_SURTIDOR.AUTORIZADO;
                            break;
                        case 0x05:
                            despachoTemp.StatusUltimaVenta = Despacho.ESTADO_SURTIDOR.VENTA_FINALIZADA_IMPAGA;
                            break;
                        case 0x08:
                            despachoTemp.StatusUltimaVenta = Despacho.ESTADO_SURTIDOR.DEFECTUOSO;
                            break;
                        case 0x09:
                            despachoTemp.StatusUltimaVenta = Despacho.ESTADO_SURTIDOR.ANULADO;
                            break;
                        case 0x0A:
                            despachoTemp.StatusUltimaVenta = Despacho.ESTADO_SURTIDOR.DETENIDO;
                            detenido = true;
                            break;
                        default:
                            break;
                    }

                    int posicion = codigo_producto + 1;

                    if (despachando || detenido)
                    {
                        despachoTemp.NroUltimaVenta = 0;
                        despachoTemp.ProductoUltimaVenta = 0;
                        despachoTemp.MontoUltimaVenta = 0;
                        despachoTemp.VolumenUltimaVenta = 0;
                        despachoTemp.PpuUltimaVenta = 0;
                        despachoTemp.UltimaVentaFacturada = false;
                        despachoTemp.IdUltimaVenta = 0;
                        posicion = status + 1;
                    }
                    else
                    {
                        despachoTemp.NroUltimaVenta = respuesta[nro_venta];
                        despachoTemp.ProductoUltimaVenta = respuesta[codigo_producto];
                        despachoTemp.MontoUltimaVenta = ConvertDouble(LeerCampoVariable(respuesta, ref posicion));
                        despachoTemp.VolumenUltimaVenta = ConvertDouble(LeerCampoVariable(respuesta, ref posicion));
                        despachoTemp.PpuUltimaVenta = ConvertDouble(LeerCampoVariable(respuesta, ref posicion));
                        despachoTemp.UltimaVentaFacturada = Convert.ToBoolean(respuesta[posicion]);
                        posicion++;
                        despachoTemp.IdUltimaVenta = Convert.ToInt32(LeerCampoVariable(respuesta, ref posicion));
                    }

                    //Proceso venta anterior
                    byte statusVentaAnterior = respuesta[posicion];
                    switch (statusVentaAnterior)
                    {
                        case 0x01:
                            despachoTemp.StatusVentaAnterior = Despacho.ESTADO_SURTIDOR.DISPONIBLE;
                            break;
                        case 0x05:
                            despachoTemp.StatusVentaAnterior = Despacho.ESTADO_SURTIDOR.VENTA_FINALIZADA_IMPAGA;
                            break;
                        default:
                            break;
                    }
                    posicion++;

                    despachoTemp.NroVentaAnterior = respuesta[posicion];
                    posicion++;

                    despachoTemp.ProductoVentaAnterior = respuesta[posicion];
                    posicion++;

                    despachoTemp.MontoVentaAnterior = ConvertDouble(LeerCampoVariable(respuesta, ref posicion));
                    despachoTemp.VolumenVentaAnterios = ConvertDouble(LeerCampoVariable(respuesta, ref posicion));
                    despachoTemp.PpuVentaAnterior = ConvertDouble(LeerCampoVariable(respuesta, ref posicion));
                    despachoTemp.VentaAnteriorFacturada = Convert.ToBoolean(respuesta[posicion]);
                    posicion++;

                    despachoTemp.IdVentaAnterior = despachando || detenido ? 0 : Convert.ToInt32(LeerCampoVariable(respuesta, ref posicion));
                }
                catch (Exception e)
                {
                    throw new Exception("\nError al obtener informacion del despacho.\n\tExcepcion: " + e.Message);
                }
            }
            else
            {
                _ = Log.Instance.WriteLog("No se recibieron datos al solicitar información del surtidor.", Log.LogType.t_warning);
                despachoTemp = null;
            }
            return despachoTemp;
        }

        /*
         * Metodo para realizar el corte del turno actual.
         */
        public CierreDeTurno InfoCierreDeTurnoActual()
        {
            byte[] mensaje = new byte[] { 0x07 };  //Comando para pedir la informacion del corte de turno actual
            byte[] respuesta = Log.Instance.GetLogLevel().Equals(Log.LogType.t_debug) ? LeerArchivo("Cierre") : EnviarComando(mensaje);

            if (!File.Exists(Environment.CurrentDirectory + "/Cierre.txt"))
            {
                GuardarRespuesta(respuesta, "Cierre.txt");
            }

            CierreDeTurno cierreDeTurno = InfoTurno(respuesta);

            return cierreDeTurno;
        }

        /*
         * Metodo para pedir información del turno actual (sin cortarlo).
         */
        public CierreDeTurno InfoTurnoEnCurso()
        {
            byte[] mensaje = new byte[] { 0x08 };  //Comando para pedir la informacion del turno en curso
            byte[] respuesta = Log.Instance.GetLogLevel().Equals(Log.LogType.t_debug) ? LeerArchivo("Cierre") : EnviarComando(mensaje);

            if (!File.Exists(Environment.CurrentDirectory + "/TurnoEnCurso.txt"))
            {
                GuardarRespuesta(respuesta, "TurnoEnCuerso.txt");
            }

            CierreDeTurno turnoEnCurso = InfoTurno(respuesta);

            return turnoEnCurso;
        }

        /*
         * Metodo para traer la informacion del cierre anterior, al ultimo cortado.
         */
        public CierreDeTurno InfoCierreDeTurnoAnterior()
        {
            byte[] mensaje = new byte[] { 0x0B };  //Comando para pedir la informacion del corte de turno anterior
            byte[] respuesta = Log.Instance.GetLogLevel().Equals(Log.LogType.t_debug) ? LeerArchivo("Cierre") : EnviarComando(mensaje);

            if (!File.Exists(Environment.CurrentDirectory + "/CierreAnterior.txt"))
            {
                GuardarRespuesta(respuesta, "CierreAnterior.txt");
            }

            CierreDeTurno cierreDeTurnoAnterior = InfoTurno(respuesta);

            return cierreDeTurnoAnterior;
        }
        private CierreDeTurno InfoTurno(byte[] respuesta)
        {
            CierreDeTurno turno = new CierreDeTurno();
            Estacion estacion = Estacion.InstanciaEstacion;
            try
            {
                if (respuesta[0] == 0xFF)
                {
                    turno.TurnoSinVentas = true;
                    return turno;
                }

                int posicion = 1;
                int cantidadMediosDePago = 8;
                for (int i = 0; i < cantidadMediosDePago; i++)
                {
                    TotalMedioDePago totalMedioDePagoTemp = new TotalMedioDePago
                    {
                        NumeroMedioPago = i + 1,
                        TotalMonto = ConvertDouble(LeerCampoVariable(respuesta, ref posicion)),
                        TotalVolumen = ConvertDouble(LeerCampoVariable(respuesta, ref posicion))
                    };
                    turno.TotalesMediosDePago.Add(totalMedioDePagoTemp);
                }
                turno.Impuesto1 = LeerCampoVariable(respuesta, ref posicion);
                turno.Impuesto2 = LeerCampoVariable(respuesta, ref posicion);

                turno.PeriodoDePrecios = respuesta[posicion];
                posicion++;
                for (int i = 0; i < turno.PeriodoDePrecios; i++)
                {
                    List<List<TotalPorProducto>> totalesPorProductoPorNivelesTemp = new List<List<TotalPorProducto>>();
                    turno.NivelesDePrecios = respuesta[posicion];
                    posicion++;
                    for (int j = 0; j < turno.NivelesDePrecios; j++)
                    {
                        List<TotalPorProducto> totalesPorProductoTemp = new List<TotalPorProducto>();
                        List<Producto> productosTemp = estacion.Productos;
                        foreach (Producto producto in productosTemp)
                        {
                            TotalPorProducto totalPorProductoTemp = new TotalPorProducto
                            {
                                Periodo = i + 1,
                                Nivel = j + 1,

                                NumeroDeProducto = producto.NumeroDeProducto,
                                TotalMonto = ConvertDouble(LeerCampoVariable(respuesta, ref posicion)),
                                TotalVolumen = ConvertDouble(LeerCampoVariable(respuesta, ref posicion)),
                                PrecioUnitario = ConvertDouble(LeerCampoVariable(respuesta, ref posicion))
                            };
                            /// Me informa el Monto, el total y el numero de producto
                            totalesPorProductoTemp.Add(totalPorProductoTemp);
                        }
                        totalesPorProductoPorNivelesTemp.Add(totalesPorProductoTemp);
                    }
                    turno.TotalesPorProductosPorNivelesPorPeriodo.Add(totalesPorProductoPorNivelesTemp);
                }
                int numeroDeMangueras = estacion.NumeroDeMangueras + 1;
                for (int i = 1; i < numeroDeMangueras; i++)
                {
                    TotalPorManguera totalPorMangueraTemp = new TotalPorManguera()
                    {
                        NumeroDeManguera = i,
                        TotalVntasMonto = ConvertDouble(LeerCampoVariable(respuesta, ref posicion)),
                        TotalVntasVolumen = ConvertDouble(LeerCampoVariable(respuesta, ref posicion)),
                        TotalVntasSinControlMonto = ConvertDouble(LeerCampoVariable(respuesta, ref posicion)),
                        TotalVntasSinControlVolumen = ConvertDouble(LeerCampoVariable(respuesta, ref posicion)),
                        TotalPruebasMonto = ConvertDouble(LeerCampoVariable(respuesta, ref posicion)),
                        TotalPruebasVolumen = ConvertDouble(LeerCampoVariable(respuesta, ref posicion))
                    };
                    turno.TotalPorMangueras.Add(totalPorMangueraTemp);
                }
                int numeroDeTanques = estacion.NumeroDeTanques + 1;
                for (int i = 1; i < numeroDeTanques; i++)
                {
                    TotalPorTanque totalPorTanqueTemp = new TotalPorTanque
                    {
                        NumeroDeTanque = i,
                        Producto = LeerCampoVariable(respuesta, ref posicion),
                        Agua = LeerCampoVariable(respuesta, ref posicion),
                        Vacio = LeerCampoVariable(respuesta, ref posicion),
                        Capacidad = LeerCampoVariable(respuesta, ref posicion)
                    };
                }
                int cantidadDeProductos = estacion.NumeroDeProductos + 1;
                for (int i = 1; i < cantidadDeProductos; i++)
                {
                    ProductoEnTanque productoEnTanqueTemp = new ProductoEnTanque
                    {
                        NumeroDeProducto = i,
                        VolumenEnTanques = LeerCampoVariable(respuesta, ref posicion),
                        AguaEnTanques = LeerCampoVariable(respuesta, ref posicion),
                        VacioEnTanques = LeerCampoVariable(respuesta, ref posicion),
                        CapacidadEnTanques = LeerCampoVariable(respuesta, ref posicion)
                    };
                    turno.ProductoEnTanques.Add(productoEnTanqueTemp);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error procesando el cierre de turno. \nExcepcion: {e.Message}");
            }
            return turno;
        }

        /*
         * Metodo de comunicacion con el cem-44. De este metodo salen todas las comunicaciones.
         * Se intetna enviar un comando y se trabaja con la respuesta.
         * En caso de desconexión, cuenta con un manejo de reconexión.
         */
        private byte[] EnviarComando(byte[] comando)
        {
            byte[] buffer = null;
            NamedPipeClientStream pipeClient = null;

            try
            {
                int retries = 1;

                // Política de reintentos
                PolicyResult policyResult = Policy.Handle<Exception>()
                    .WaitAndRetry(retryCount: 4,
                                  sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)),
                                  onRetry: (exception, TimeSpan, conttext) =>
                                  {
                                      // Cerrar el pipe en caso de fallo
                                      if (pipeClient != null)
                                      {
                                          pipeClient.Dispose();
                                          pipeClient = null; // Limpiar el pipe para la nueva conexión
                                      }
                                      _ = Log.Instance.WriteLog($"\n\t  Excepción: {exception.Message.Trim()} Intento: {retries}", Log.LogType.t_error);
                                      retries++;
                                  }).ExecuteAndCapture(() =>
                                  {
                                      // Crear el pipeClient si está cerrado
                                      if (pipeClient == null)
                                      {
                                          pipeClient = new NamedPipeClientStream(ipControlador, nombreDelPipe);
                                      }

                                      // Conectar con tiempo de espera
                                      pipeClient.Connect(5000);

                                      // Enviar el comando
                                      pipeClient.Write(comando, 0, comando.Length);

                                      // Leer respuesta
                                      buffer = new byte[pipeClient.OutBufferSize];
                                      _ = pipeClient.Read(buffer, 0, buffer.Length);
                                  });
                // Verificación de resultado de conexión
                Controlador.CheckConexion((int)policyResult.Outcome);
                if (policyResult.Outcome != 0)
                {
                    _ = Log.Instance.WriteLog($"  Fin de intentos...\n", Log.LogType.t_error);
                    ReloadData();
                }
            }
            catch (Exception e)
            {
                _ = Log.Instance.WriteLog($"Error al enviar comando. Excepcón: {e.Message}", Log.LogType.t_error);
            }
            finally
            {
                // Asegurarse de cerrar el pipe al final
                if (pipeClient != null)
                {
                    pipeClient.Dispose();
                }
            }

            return buffer;
        }

        /*
         * Metodo que se encarga de actualizar la configuracion, en caso de que haya cambiado.
         */
        private void ReloadData()
        {
            ipControlador = Configuracion.LeerConfiguracion().IpControlador;
            protocolo = Configuracion.LeerConfiguracion().Protocolo;
        }

        /*
         * Metodo para leer los campos variables, por ejemplo precios o cantidades.
         * El metodo para frenar la iteracion, es un valor conocido, proporcionado por el fabricante
         * denominado como "separador".
         */
        private string LeerCampoVariable(byte[] data, ref int pos)
        {
            string ret = "";
            ret += Encoding.ASCII.GetString(new byte[] { data[pos] });
            int i = pos + 1;
            while (data[i] != separador)
            {
                ret += Encoding.ASCII.GetString(new byte[] { data[i] });
                i++;
            }
            i++;
            pos = i;
            return ret;
        }

        /*
         * Metodo para saltearse los valores que no son utilizados en la respuesta del CEM.
         * Al finalizar el proceso del metodo, el valor de la posicion queda seteada para
         * el siguiente dato a procesar.
         */
        private void DescartarCampoVariable(byte[] data, ref int pos)
        {
            while (data[pos] != separador)
            {
                pos++;
            }
            pos++;
        }

        /*
         * Se utiliza para testear las respuestas reales del Cem-44
         * se lee un .txt que contiene las respuestas y las guarda en un byte,
         * para simular la respuesta.
         */
        private byte[] LeerArchivo(string nombreArchivo)
        {
            byte[] respuesta = null;
            // Obtener la ruta del directorio donde se ejecuta el programa
            string directorioEjecucion = AppDomain.CurrentDomain.BaseDirectory;

            // Combinar la ruta del directorio con el nombre del archivo
            string rutaArchivo = Path.Combine(directorioEjecucion, nombreArchivo + ".txt");

            // Verificar si el archivo existe
            if (File.Exists(rutaArchivo))
            {
                // Leer todas las líneas del archivo
                string[] lines = File.ReadAllLines(rutaArchivo);

                // Arreglo para almacenar todos los bytes
                byte[] byteArray = new byte[lines.Length * 4]; // Se asume que cada fila tiene 4 valores numéricos (4 bytes cada uno)

                // Índice para rastrear la posición en el arreglo de bytes
                int index = 0;

                // Procesar cada línea del archivo
                foreach (string line in lines)
                {
                    // Dividir la línea en valores numéricos individuales
                    string[] numericValues = line.Split(',');

                    // Convertir cada valor numérico en un byte y agregarlos al arreglo de bytes
                    foreach (string value in numericValues)
                    {
                        byteArray[index++] = Convert.ToByte(value.Trim());
                    }
                }
                respuesta = byteArray;
            }
            return respuesta;
        }
        public double ConvertDouble(string value)
        {
            return double.TryParse(value, NumberStyles.Any, culture, out double result) ? result : result;
        }
        public void GuardarRespuesta(byte[] respuesta, string nombreArchivo)
        {
            // Obtener la ruta del archivo en la misma carpeta donde se ejecuta el programa
            //string nombreArchivo = "respuesta.txt";
            string rutaCompleta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nombreArchivo);

            if (!File.Exists(nombreArchivo))
            {
                // Escribir en el archivo
                using (StreamWriter sw = File.AppendText(rutaCompleta))
                {
                    int cont = 0;
                    int iteraciones = 0;
                    int iterAnt = 0;
                    while (iteraciones < respuesta.Length)
                    {
                        sw.WriteLine(respuesta[iteraciones]);
                        if (iterAnt > 0)
                        {
                            if (respuesta[iteraciones] == 0 && respuesta[iterAnt] == 0 && cont < 6)
                            {
                                cont++;
                            }
                            else if (respuesta[iteraciones] == 0 && cont >= 6)
                            {
                                break;
                            }
                        }
                        iteraciones++;
                        iterAnt = iteraciones - 1;
                    }
                }
            }
        }
    }
}
