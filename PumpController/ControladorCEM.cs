using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace PumpController
{
    internal class ControladorCEM : Controlador
    {
        private readonly ConectorCEM conectorCEM;
        public ControladorCEM()
        {
            conectorCEM = new ConectorCEM();
        }
        public override void GrabarConfigEstacion()
        {
            Estacion estacion = conectorCEM.ConfiguracionDeLaEstacion();
            try
            {
                List<Surtidor> tempSurtidores = estacion.NivelesDePrecio[0];
                int numeroDeManguera = 1;
                foreach (Surtidor surtidor in tempSurtidores)
                {
                    string campos = "IdSurtidor,Manguera,Producto,Precio,DescProd";
                    List<Manguera> tempManguera = surtidor.Mangueras;
                    foreach (Manguera manguera in tempManguera)
                    {
                        string rows = string.Format("{0},{1},{2},{3},'{4}'",
                            surtidor.NumeroDeSurtidor,
                            manguera.NumeroDeManguera,
                            manguera.Producto.NumeroDeProducto,
                            manguera.Producto.PrecioUnitario.ToString(),
                            manguera.Producto.Descripcion);

                        DataTable tabla = ConectorSQLite.Dt_query("SELECT * FROM Surtidores WHERE IdSurtidor = " + surtidor.NumeroDeSurtidor + " AND Manguera = '" + manguera.NumeroDeManguera + "'");

                        _ = tabla.Rows.Count == 0
                            ? ConectorSQLite.Query(string.Format("INSERT INTO Surtidores ({0}) VALUES ({1})", campos, rows))
                            : ConectorSQLite.Query(string.Format("UPDATE Surtidores SET Producto = ('{0}'), Precio = ('{1}'), DescProd = ('{2}') WHERE IdSurtidor = ({3}) AND Manguera = ('{4}')",
                                manguera.Producto.NumeroDeProducto,
                                manguera.Producto.PrecioUnitario.ToString(),
                                manguera.Producto.Descripcion,
                                surtidor.NumeroDeSurtidor,
                                manguera.NumeroDeManguera));
                        _ = Log.Instance.WriteLog(string.Format("SURT: ({0}) Cem44: ({1}) Desc: ({2})", numeroDeManguera, surtidor.NumeroDeSurtidor + manguera.NumeroDeManguera, manguera.Producto.Descripcion), Log.LogType.t_info);
                        numeroDeManguera++;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error en el metodo GrabarConfigEstacion. Excepcion: {e.Message}");
            }
        }
        public override void GrabarTanques()
        {
            List<Tanque> tanques = conectorCEM.InfoTanques(Estacion.InstanciaEstacion.Tanques.Count);
            try
            {
                for (int i = 0; i < tanques.Count; i++)
                {
                    double total = tanques[i].VolumenProductoT + tanques[i].VolumenVacioT + tanques[i].VolumenAguaT;
                    int res = ConectorSQLite.Query("UPDATE Tanques " +
                                                   "SET volumen = " + tanques[i].VolumenProductoT.ToString(CultureInfo.InvariantCulture) + ", total = " + total.ToString(CultureInfo.InvariantCulture) + " " +
                                                   "WHERE id = " + tanques[i].NumeroDeTanque);

                    if (res == 0)
                    {
                        total = tanques[i].VolumenProductoT + tanques[i].VolumenVacioT + tanques[i].VolumenAguaT;
                        string campos = "id,volumen,total";
                        string rows = string.Format("{0},{1},{2}",
                            tanques[i].NumeroDeTanque,
                            tanques[i].VolumenProductoT.ToString(CultureInfo.InvariantCulture),
                            total.ToString(CultureInfo.InvariantCulture));
                        _ = ConectorSQLite.Query(string.Format("INSERT INTO Tanques ({0}) VALUES ({1})", campos, rows));
                    }
                    _ = Log.Instance.WriteLog(string.Format("Tanque ({0}) Capacidad Maxima ({1}) Cantidad Actual ({2})", i, total, tanques[i].VolumenProductoT), Log.LogType.t_info);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error en el método GrabarTanques.\nExcepción: " + e.Message);
            }
        }
        public override void GrabarTurnoEnCurso()
        {
            _ = Log.Instance.WriteLog($"Iniciando pedido de informacion del turno actual.\n", Log.LogType.t_info);
            CierreDeTurno turnoEnCurso = conectorCEM.InfoTurnoEnCurso();
            GrabarTurno(turnoEnCurso);
        }
        public override void GrabarCierreDeTurno()
        {
            _ = Log.Instance.WriteLog($"Iniciando pedido de informacion del cierre de turno actual.\n", Log.LogType.t_info);
            CierreDeTurno cierreDeTurno = conectorCEM.InfoCierreDeTurno();
            GrabarTurno(cierreDeTurno);
        }
        public override void GrabarCierreAnterior()
        {
            _ = Log.Instance.WriteLog($"Iniciando pedido de informacion del cierre de turno anterior.\n", Log.LogType.t_info);
            CierreDeTurno cierreAnterior = conectorCEM.InfoCierreDeTurnoAnterior();
            GrabarTurno(cierreAnterior);
        }
        public void GrabarTurno(CierreDeTurno turno)
        {
            _ = ConectorSQLite.Query("UPDATE cierreBandera SET hacerCierre = 0 WHERE hacerCierre = 1");
            if (!turno.TurnoSinVentas)
            {
                try
                {
                    string query = "INSERT INTO Cierres (monto_contado, volumen_contado, monto_YPFruta, volumen_YPFruta) VALUES ({0})";
                    string row = string.Format(
                        "'{0}','{1}','{2}','{3}'",
                        turno.TotalesMediosDePago[0].TotalMonto.ToString(CultureInfo.InvariantCulture),
                        turno.TotalesMediosDePago[0].TotalVolumen.ToString(CultureInfo.InvariantCulture),
                        turno.TotalesMediosDePago[3].TotalMonto.ToString(CultureInfo.InvariantCulture),
                        turno.TotalesMediosDePago[3].TotalVolumen.ToString(CultureInfo.InvariantCulture));

                    _ = ConectorSQLite.Query(string.Format(query, row));

                    // Traer ID del cierre para poder referenciar los detalles
                    DataTable table = ConectorSQLite.Dt_query("SELECT max(id) FROM Cierres");

                    int id = Convert.ToInt32(table.Rows[0][0]);

                    // Grabar CierresPorProducto
                    query = "INSERT INTO CierresPorProducto (id, producto, monto, volumen) VALUES (" + id + ", {0})";
                    for (int i = 0; i < turno.TotalesPorProductosPorNivelesPorPeriodo[0][0].Count; i++)
                    {
                        string aux =
                            (i + 1).ToString() + "," +
                            "'" + turno.TotalesPorProductosPorNivelesPorPeriodo[0][0][i].TotalMonto + "'," +
                            "'" + turno.TotalesPorProductosPorNivelesPorPeriodo[0][0][i].TotalVolumen + "'";

                        _ = ConectorSQLite.Query(string.Format(query, aux));
                    }

                    // Grabar CierresPorManguera
                    Estacion estacion = Estacion.InstanciaEstacion;
                    int contador = 0;

                    query = "INSERT INTO CierresPorManguera (id, surtidor, manguera, monto, volumen) VALUES (" + id + ", {0})";
                    for (int i = 0; i < estacion.NumeroDeSurtidores; i++)
                    {
                        List<Surtidor> surtidores = estacion.NivelesDePrecio[0];
                        for (int j = 0; j < surtidores[i].TipoDeSurtidor; j++)
                        {
                            string aux =
                                (i + 1).ToString() + "," +
                                (j + 1).ToString() + "," +
                                "'" + turno.TotalPorMangueras[contador].TotalVntasMonto + "'," +
                                "'" + turno.TotalPorMangueras[contador].TotalVntasVolumen + "'";

                            contador++;

                            _ = ConectorSQLite.Query(string.Format(query, aux));
                        }
                    }
                    _ = ConectorSQLite.Query("DELETE FROM despachos");
                }
                catch (Exception e)
                {
                    throw new Exception($"Error en el metodo GrabarTurno. Excepcion: {e.Message}");
                }
            }
        }
        /*
         * Metodo para procesar la informacion que traer el CEM y la guarda en la base
         */
        public override void GrabarDespachos()
        {
            for (int i = 1; i < Estacion.InstanciaEstacion.NumeroDeSurtidores + 1; i++)
            {
                Despacho despacho = conectorCEM.InfoSurtidor(i);
                try
                {
                    if (despacho.StatusUltimaVenta == Despacho.ESTADO_SURTIDOR.DESPACHANDO || despacho.StatusUltimaVenta == Despacho.ESTADO_SURTIDOR.DETENIDO
                                                           || despacho.NroUltimaVenta == 0 || despacho.IdUltimaVenta == 0)
                    {
                        continue;
                    }

                    DataTable tabla = ConectorSQLite.Dt_query("SELECT * FROM despachos WHERE id = '" + despacho.IdUltimaVenta + "' AND surtidor = " + i);

                    /// Procesamiento de la ultima venta
                    if (tabla.Rows.Count == 0)
                    {
                        InfoDespacho infoDespacho = new InfoDespacho
                        {
                            ID = despacho.IdUltimaVenta,
                            Surtidor = i,
                            Manguera = "0",
                            Producto = 0,
                            Monto = despacho.MontoUltimaVenta,
                            Volumen = despacho.VolumenUltimaVenta,
                            PPU = despacho.PpuUltimaVenta,
                            Facturado = despacho.UltimaVentaFacturada,
                            YPFRuta = false,
                            Desc = ""
                        };
                        // Verificamos YPF en Ruta
                        foreach (Producto producto in Estacion.InstanciaEstacion.Productos)
                        {
                            if (producto.PrecioUnitario == infoDespacho.PPU)
                            {
                                if (producto.NumeroPorDespacho == 0)
                                {
                                    producto.NumeroPorDespacho = despacho.ProductoUltimaVenta;
                                    _ = ConectorSQLite.Query("UPDATE Productos " +
                                                   "SET producto = '" + producto.Descripcion + "', precio = " + producto.PrecioUnitario.ToString(CultureInfo.InvariantCulture) +
                                                                                                ", numero_despacho =  " + producto.NumeroPorDespacho + " " +
                                                   "WHERE id_producto = " + producto.NumeroDeProducto);
                                }
                                infoDespacho.Producto = producto.NumeroDeProducto;
                                infoDespacho.Desc = producto.Descripcion;
                                if (despacho.UltimaVentaFacturada)
                                {
                                    infoDespacho.YPFRuta = true;
                                }
                                break;
                            }
                        }
                        if (infoDespacho.Producto == 0)
                        {
                            foreach (Producto producto in Estacion.InstanciaEstacion.Productos)
                            {
                                if (producto.NumeroPorDespacho != 0 && producto.NumeroPorDespacho == despacho.ProductoUltimaVenta)
                                {
                                    infoDespacho.Producto = producto.NumeroPorDespacho;
                                    infoDespacho.Desc = producto.Descripcion;
                                }
                                else
                                {
                                    infoDespacho.Producto = despacho.ProductoUltimaVenta;
                                }
                                infoDespacho.YPFRuta = true;
                            }
                        }
                        if (infoDespacho.Desc != "")
                        {
                            List<Manguera> tempManguera = Estacion.InstanciaEstacion.Surtidores[i - 1].Mangueras;
                            foreach (Manguera manguera in tempManguera)
                            {
                                if (infoDespacho.Desc.Equals(manguera.Producto.Descripcion))
                                {
                                    infoDespacho.Manguera = manguera.NumeroDeManguera.ToString();
                                    break;
                                }
                            }
                        }

                        /// Agregar a Base de Datos
                        bool despacho_pedido = false;
                        string campos = "id,surtidor,producto,monto,volumen,PPU,facturado,YPFruta,descripcion,manguera,despacho_pedido";
                        string row = string.Format("{0},{1},'{2}',{3},{4},{5},{6},{7},'{8}',{9},{10}",
                            infoDespacho.ID,
                            infoDespacho.Surtidor,
                            infoDespacho.Producto,
                            infoDespacho.Monto.ToString(CultureInfo.InvariantCulture),
                            infoDespacho.Volumen.ToString(CultureInfo.InvariantCulture),
                            infoDespacho.PPU.ToString(CultureInfo.InvariantCulture),
                            infoDespacho.Facturado,
                            infoDespacho.YPFRuta,
                            infoDespacho.Desc,
                            infoDespacho.Manguera,
                            despacho_pedido);
                        _ = ConectorSQLite.Query(string.Format("INSERT INTO Despachos ({0}) VALUES ({1})", campos, row));
                    }

                    tabla = ConectorSQLite.Dt_query("SELECT * FROM Despachos WHERE id = '" + despacho.IdVentaAnterior + "' AND surtidor = " + i);

                    if (despacho.IdVentaAnterior == 0)
                    {
                        continue;
                    }

                    /// Procesamiento de la venta anterior
                    if (tabla.Rows.Count == 0)
                    {
                        InfoDespacho infoDespacho = new InfoDespacho
                        {
                            ID = despacho.IdVentaAnterior,
                            Surtidor = i,
                            Manguera = "0",
                            Producto = 0,
                            Monto = despacho.MontoVentaAnterior,
                            Volumen = despacho.VolumenVentaAnterios,
                            PPU = despacho.PpuVentaAnterior,
                            YPFRuta = false,
                            Desc = ""
                        };
                        // Verificamos YPF en Ruta
                        foreach (Producto producto in Estacion.InstanciaEstacion.Productos)
                        {
                            if (producto.PrecioUnitario == infoDespacho.PPU)
                            {
                                if (producto.NumeroPorDespacho == 0)
                                {
                                    producto.NumeroPorDespacho = despacho.ProductoVentaAnterior;
                                    ConectorSQLite.Query("UPDATE Productos " +
                                                   "SET producto = '" + producto.Descripcion + "', precio = " + producto.PrecioUnitario.ToString(CultureInfo.InvariantCulture) +
                                                                                                ", numero_despacho =  " + producto.NumeroPorDespacho + " " +
                                                   "WHERE id_producto = " + producto.NumeroDeProducto);
                                }
                                infoDespacho.Producto = producto.NumeroDeProducto;
                                infoDespacho.Desc = producto.Descripcion;
                                if (despacho.VentaAnteriorFacturada)
                                {
                                    infoDespacho.YPFRuta = true;
                                }
                                break;
                            }
                        }
                        if (infoDespacho.Producto == 0)
                        {
                            foreach (Producto p in Estacion.InstanciaEstacion.Productos)
                            {
                                if (p.NumeroPorDespacho != 0 && p.NumeroPorDespacho == despacho.ProductoVentaAnterior)
                                {
                                    infoDespacho.Producto = p.NumeroPorDespacho;
                                    infoDespacho.Desc = p.Descripcion;
                                }
                                else
                                {
                                    infoDespacho.Producto = despacho.ProductoVentaAnterior;
                                }
                                infoDespacho.YPFRuta = true;
                            }
                        }
                        if (infoDespacho.Desc != "")
                        {
                            List<Manguera> tempManguera = Estacion.InstanciaEstacion.Surtidores[i - 1].Mangueras;
                            foreach (Manguera manguera in tempManguera)
                            {
                                if (infoDespacho.Desc.Equals(manguera.Producto.Descripcion))
                                {
                                    infoDespacho.Manguera = manguera.NumeroDeManguera.ToString();
                                }
                            }
                        }
                        /// Agregar a Base de Datos
                        bool despacho_pedido = false;
                        string campos = "id,surtidor,producto,monto,volumen,PPU,facturado,YPFruta,descripcion,manguera,despacho_pedido";
                        string row = string.Format("{0},{1},'{2}',{3},{4},{5},{6},{7},'{8}',{9},{10}",
                            infoDespacho.ID,
                            infoDespacho.Surtidor,
                            infoDespacho.Producto,
                            infoDespacho.Monto.ToString(CultureInfo.InvariantCulture),
                            infoDespacho.Volumen.ToString(CultureInfo.InvariantCulture),
                            infoDespacho.PPU.ToString(CultureInfo.InvariantCulture),
                            infoDespacho.Facturado,
                            infoDespacho.YPFRuta,
                            infoDespacho.Desc,
                            infoDespacho.Manguera,
                            despacho_pedido);
                        _ = ConectorSQLite.Query(string.Format("INSERT INTO Despachos ({0}) VALUES ({1})", campos, row));
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"Error en el metodo GrabarDespachos. Excepcion: {e.Message}");
                }
            }
        }

        public override void GrabarProductos()
        {
            List<Producto> tempProductos = Estacion.InstanciaEstacion.Productos;

            try
            {
                foreach (Producto producto in tempProductos)
                {
                    int res = ConectorSQLite.Query("UPDATE Productos " +
                                                   "SET producto = '" + producto.Descripcion + "', precio = " + producto.PrecioUnitario.ToString(CultureInfo.InvariantCulture) +
                                                                                                ", numero_despacho =  " + producto.NumeroPorDespacho + " " +
                                                   "WHERE id_producto = " + producto.NumeroDeProducto);

                    if (res == 0)
                    {
                        string campos = "numero_producto,numero_despacho,producto,precio";
                        string row = string.Format("{0},{1},'{2}',{3}",
                            producto.NumeroDeProducto,
                            producto.NumeroPorDespacho,
                            producto.Descripcion,
                            producto.PrecioUnitario.ToString(CultureInfo.InvariantCulture));
                        _ = ConectorSQLite.Query(string.Format("INSERT INTO Productos ({0}) VALUES ({1})", campos, row));
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error en el metodo GrabarProductos. Excepcion: {e}");
            }
        }
    }
}
