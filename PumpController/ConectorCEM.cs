using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace PumpController
{
    public class ConectorCEM
    {
        private readonly byte separador = 0x7E;
        private readonly string nombreDelPipe = "CEM44POSPIPE";
        private readonly string ipControlador;
        private readonly int protocolo;

        public ConectorCEM()
        {
            ipControlador = Configuracion.LeerConfiguracion().IpControlador;
            protocolo = Configuracion.LeerConfiguracion().Protocolo;
        }
        public Estacion ConfiguracionDeLaEstacion()
        {
            Estacion estacionTemp = Estacion.InstanciaEstacion;
            try
            {
                byte[] mensaje = protocolo == 16 ? (new byte[] { 0x65 }) : (new byte[] { 0xB5 });
                int confirmacion = 0;
                int surtidores = 1;
                int islas = 2;          // Esto no se usa, guarda el valor 0
                int tanques = 3;
                int productos = 4;

                //Traigo las descripciones de los productos de la tabla combus
                List<string[]> combus = TraerDescripciones();

                ///byte[] respuesta = EnviarComando(mensaje);

                ///Uso este comando para leer respuestas guardadas
                byte[] respuesta = LeerArchivo("ConfigEstacion");

                if (respuesta[confirmacion] != 0x0)
                {
                    throw new Exception("No se recibió mensaje de confirmación al solicitar info de la estación");
                }

                estacionTemp.NumeroDeSurtidores = respuesta[surtidores];
                _ = respuesta[islas];

                estacionTemp.NumeroDeTanques = respuesta[tanques];

                estacionTemp.NumeroDeProductos = respuesta[productos];

                int posicion = productos + 1;
                List<Producto> tempProductos = new List<Producto>();
                for (int i = 0; i < estacionTemp.NumeroDeProductos; i++)
                {
                    Producto producto = new Producto
                    {
                        NumeroDeProducto = LeerCampoVariable(respuesta, ref posicion),
                        PrecioUnitario = LeerCampoVariable(respuesta, ref posicion)
                    };
                    DescartarCampoVariable(respuesta, ref posicion);

                    foreach (string[] s in combus)
                    {
                        if (s[1].Equals(producto.PrecioUnitario))
                        {
                            producto.Descripcion = s[0];
                            break;
                        }
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
                            NumeroDeManquera = j + 1
                        };
                        string productoH = "0" + respuesta[posicion];
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
                //estacionTemp.nivelesDePrecio.Add(tempSurtidores);

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
                throw new Exception($"Error al obtener informacion de la estacion. Excepcion: {e.Message}");
            }
            return estacionTemp;
        }
        public List<Tanque> InfoTanques(int cantidadDeTanques)
        {
            byte[] mensaje = protocolo == 16 ? (new byte[] { 0x68 }) : (new byte[] { 0xB8 });
            int confirmacion = 0;

            ///Uso este comando para leer respuestas guardadas
            byte[] respuesta = LeerArchivo("infoTanques");
            
            ///byte[] respuesta = EnviarComando(new byte[] { mensaje[0] });
            try
            {
                if (respuesta[confirmacion] != 0x0)
                {
                    throw new Exception("No se recibió mensaje de confirmación al solicitar info del surtidor");
                }

                int posicion = confirmacion + 1;

                for (int i = 0; i < cantidadDeTanques; i++)
                {
                    foreach (Tanque tanque in Estacion.InstanciaEstacion.Tanques)
                    {
                        if (tanque.NumeroDeTanque == (i + 1))
                        {
                            tanque.NumeroDeTanque = i + 1;
                            tanque.VolumenProductoT = LeerCampoVariable(respuesta, ref posicion);
                            tanque.VolumenAguaT = LeerCampoVariable(respuesta, ref posicion);
                            tanque.VolumenVacioT = LeerCampoVariable(respuesta, ref posicion);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error al obtener informacion del tanque" + e.Message);
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

            ///Uso este comando para leer respuestas guardadas
            byte[] respuesta = LeerArchivo("despacho-" + numeroDeSurtidor);

            ///byte[] respuesta = EnviarComando(new byte[] { (byte)(mensaje[0] + Convert.ToByte(numeroDeSurtidor)) });
            try
            {
                if (respuesta[confirmacion] != 0x0)
                {
                    throw new Exception("No se recibió mensaje de confirmación al solicitar info del surtidor");
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
                    despachoTemp.MontoUltimaVenta = "";
                    despachoTemp.VolumenUltimaVenta = "";
                    despachoTemp.PpuUltimaVenta = "";
                    despachoTemp.UltimaVentaFacturada = false;
                    despachoTemp.IdUltimaVenta = null;
                    posicion = status + 1;
                }
                else
                {
                    despachoTemp.NroUltimaVenta = respuesta[nro_venta];
                    despachoTemp.ProductoUltimaVenta = respuesta[codigo_producto];
                    despachoTemp.MontoUltimaVenta = LeerCampoVariable(respuesta, ref posicion);
                    despachoTemp.VolumenUltimaVenta = LeerCampoVariable(respuesta, ref posicion);
                    despachoTemp.PpuUltimaVenta = LeerCampoVariable(respuesta, ref posicion);
                    despachoTemp.UltimaVentaFacturada = Convert.ToBoolean(respuesta[posicion]);
                    posicion++;
                    despachoTemp.IdUltimaVenta = LeerCampoVariable(respuesta, ref posicion);
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

                despachoTemp.MontoVentaAnterior = LeerCampoVariable(respuesta, ref posicion);
                despachoTemp.VolumenVentaAnterios = LeerCampoVariable(respuesta, ref posicion);
                despachoTemp.PpuVentaAnterior = LeerCampoVariable(respuesta, ref posicion);
                despachoTemp.VentaAnteriorFacturada = Convert.ToBoolean(respuesta[posicion]);
                posicion++;

                despachoTemp.IdVentaAnterior = despachando || detenido ? "" : LeerCampoVariable(respuesta, ref posicion);
                //tablaDespachos.despachos.Add(tempDespacho);
            }
            catch (Exception e)
            {
                throw new Exception("Error al obtener informacion del despacho" + e.Message);
            }
            return despachoTemp;
        }
        public CierreDeTurno InfoTurnoEnCurso()
        {
            //byte[] mensaje = new byte[] { 0x07 };  //Comando para cortar el turno
            byte[] mensaje = new byte[] { 0x08 };  //Comando para pedir la informacion del turno en curso
            int estadoTurno = 0;
            CierreDeTurno cierreDeTurno = new CierreDeTurno();
            Estacion infoConfigEstacion = Estacion.InstanciaEstacion;

            ///Uso este comando para leer respuestas guardadas
            ///byte[] respuesta = LeerArchivo("turnoEnCurso1");

            byte[] respuesta = EnviarComando(new byte[] { mensaje[0] });
            try
            {
                if (respuesta[estadoTurno] == 0xFF)
                {
                    return cierreDeTurno;
                }

                int posicion = estadoTurno + 1;

                int cantidadMediosDePago = 8;
                for (int i = 0; i < cantidadMediosDePago; i++)
                {
                    TotalMedioDePago totalMedioDePagoTemp = new TotalMedioDePago
                    {
                        NumeroMedioPago = i + 1,
                        TotalMonto = LeerCampoVariable(respuesta, ref posicion),
                        TotalVolumen = LeerCampoVariable(respuesta, ref posicion)
                    };

                    cierreDeTurno.TotalesMediosDePago.Add(totalMedioDePagoTemp);
                }

                cierreDeTurno.Impuesto1 = LeerCampoVariable(respuesta, ref posicion);
                cierreDeTurno.Impuesto2 = LeerCampoVariable(respuesta, ref posicion);

                cierreDeTurno.PeriodoDePrecios = respuesta[posicion];
                posicion++;
                for (int i = 0; i < cierreDeTurno.PeriodoDePrecios; i++)
                {
                    List<List<TotalPorProducto>> totalesPorProductoPorNivelesTemp = new List<List<TotalPorProducto>>();
                    cierreDeTurno.NivelesDePrecios = respuesta[posicion];
                    posicion++;
                    for (int j = 0; j < cierreDeTurno.NivelesDePrecios; j++)
                    {
                        List<TotalPorProducto> totalesPorProductoTemp = new List<TotalPorProducto>();
                        List<Producto> productosTemp = infoConfigEstacion.Productos;
                        foreach (Producto producto in productosTemp)
                        {
                            TotalPorProducto totalPorProductoTemp = new TotalPorProducto
                            {
                                Periodo = i + 1,
                                Nivel = j + 1,

                                NumeroDeProducto = producto.NumeroDeProducto,
                                TotalMonto = LeerCampoVariable(respuesta, ref posicion),
                                TotalVolumen = LeerCampoVariable(respuesta, ref posicion),
                                PrecioUnitario = LeerCampoVariable(respuesta, ref posicion)
                            };
                            /// Me tira el Monto, el total y el numero de producto
                            totalesPorProductoTemp.Add(totalPorProductoTemp);
                        }
                        totalesPorProductoPorNivelesTemp.Add(totalesPorProductoTemp);
                    }
                    cierreDeTurno.TotalesPorProductosPorNivelesPorPeriodo.Add(totalesPorProductoPorNivelesTemp);
                }

                int numeroDeMangueras = infoConfigEstacion.NumeroDeMangueras + 1;
                for (int i = 1; i < numeroDeMangueras; i++)
                {
                    TotalPorManguera totalPorMangueraTemp = new TotalPorManguera()
                    {
                        NumeroDeManguera = i,
                        TotalVntasMonto = LeerCampoVariable(respuesta, ref posicion),
                        TotalVntasVolumen = LeerCampoVariable(respuesta, ref posicion),
                        TotalVntasSinControlMonto = LeerCampoVariable(respuesta, ref posicion),
                        TotalVntasSinControlVolumen = LeerCampoVariable(respuesta, ref posicion),
                        TotalPruebasMonto = LeerCampoVariable(respuesta, ref posicion),
                        TotalPruebasVolumen = LeerCampoVariable(respuesta, ref posicion)
                    };
                    cierreDeTurno.TotalPorMangueras.Add(totalPorMangueraTemp);
                }

                int numeroDeTanques = infoConfigEstacion.NumeroDeTanques + 1;
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

                int cantidadDeProductos = infoConfigEstacion.NumeroDeProductos + 1;
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
                    cierreDeTurno.ProductoEnTanques.Add(productoEnTanqueTemp);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error procesando el cierre de turno. Excepcion: {e.Message}");
            }
            return cierreDeTurno;
        }
        private byte[] EnviarComando(byte[] comando)
        {
            byte[] buffer = null;
            string ip = ipControlador;
            int maxRetries = 5;
            int delayBetweenRetries = 2000; // Milisegundos
            while (true)
            {
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(ip, nombreDelPipe))
                {
                    bool connected = false;
                    for (int i = 0; i < maxRetries; i++)
                    {
                        try
                        {
                            pipeClient.Connect(1000); // Intentar conectar con timeout de 1 segundo
                            connected = true;
                            break;
                        }
                        catch (TimeoutException)
                        {
                            _ = Log.Instance.WriteLog("Timeout al intentar conectar. Intentando de nuevo...", Log.LogType.t_warning);
                            Thread.Sleep(delayBetweenRetries);
                        }
                        catch (Exception e)
                        {
                            pipeClient.Close();
                            pipeClient.Dispose();
                            _ = Log.Instance.WriteLog("Error al intentar conectar: " + e.Message, Log.LogType.t_error);
                            Thread.Sleep(delayBetweenRetries);
                        }
                    }
                    if (!connected)
                    {
                        throw new Exception("No se pudo establecer la conexión después de varios intentos. Saliendo...");
                    }

                    try
                    {
                        pipeClient.Write(comando, 0, comando.Length);

                        buffer = new byte[pipeClient.OutBufferSize];

                        _ = pipeClient.Read(buffer, 0, buffer.Length);

                        break;
                    }
                    catch (IOException e)
                    {
                        throw new IOException("Error de comunicación: " + e.Message);
                        // Reconectar en el siguiente ciclo
                    }
                }
            }
            return buffer;
            /*using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(ip, nombreDelPipe))
            {
                try
                {
                    pipeClient.Connect(500);
                }
                catch (TimeoutException e)
                {
                    throw new IOException("Error tiempo de espera al enviar el comando. Excepción: " + e.Message);
                }
                catch (IOException e)
                {
                    pipeClient.Close();
                    pipeClient.Dispose();
                    throw new IOException("Error input/output al enviar el comando.\nExcepción: " + e.Message);
                }
                catch (Exception e)
                {
                    throw new Exception("Error al enviar el comando. Excepción: " + e.Message);
                }

                pipeClient.Write(comando, 0, comando.Length);

                buffer = new byte[pipeClient.OutBufferSize];

                _ = pipeClient.Read(buffer, 0, buffer.Length);
            }*/
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
        /*
         *  Metodo para obtener las descripciones de los combustibles, de la tabla combus
         */
        private List<string[]> TraerDescripciones()
        {
            List<string[]> datos = new List<string[]>();
            string rutaDatos = Configuracion.LeerConfiguracion().ProyNuevoRuta + @"\tablas";
            string connectionString = @"Driver={Driver para o Microsoft Visual FoxPro};SourceType=DBF;" + $@"Dbq={rutaDatos}\";

            // Definir la consulta SQL
            string query = "SELECT Desc, Importe FROM Combus";

            try
            {
                // Crear una conexión a la base de datos
                using (OdbcConnection connection = new OdbcConnection(connectionString))
                {
                    // Abrir la conexión
                    connection.Open();

                    // Crear un comando SQL con la consulta y la conexión
                    using (OdbcCommand command = new OdbcCommand(query, connection))
                    {
                        // Crear un lector de datos
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            Console.WriteLine();
                            // Leer y mostrar los datos
                            while (reader.Read())
                            {
                                // Acceder a las columnas por índice o nombre
                                string columna1 = reader.GetString(0);// Suponiendo que el segundo campo es un entero
                                float columna2 = reader.GetFloat(1);

                                datos.Add(item: new string[] { columna1.Trim(), columna2.ToString("0.000").Replace(",", ".") });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejar la excepción
                Console.WriteLine($"Error al acceder a la tabla Combus. Excepcion: {ex.Message}");
            }
            return datos;
        }
    }
}
