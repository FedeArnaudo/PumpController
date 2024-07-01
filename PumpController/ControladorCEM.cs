using System;
using System.Collections.Generic;
using System.Data;

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
                        string letra = null;
                        switch (manguera.NumeroDeManquera)
                        {
                            case 1:
                                letra = "A";
                                break;
                            case 2:
                                letra = "B";
                                break;
                            case 3:
                                letra = "C";
                                break;
                            case 4:
                                letra = "D";
                                break;
                            default:
                                break;
                        }
                        string rows = string.Format("{0},'{1}','{2}','{3}','{4}'",
                            surtidor.NumeroDeSurtidor,
                            letra,
                            manguera.Producto.NumeroDeProducto,
                            manguera.Producto.PrecioUnitario.ToString(),
                            manguera.Producto.Descripcion);

                        DataTable tabla = ConectorSQLite.Dt_query("SELECT * FROM Surtidores WHERE IdSurtidor = " + surtidor.NumeroDeSurtidor + " AND Manguera = '" + letra + "'");

                        _ = tabla.Rows.Count == 0
                            ? ConectorSQLite.Query(string.Format("INSERT INTO Surtidores ({0}) VALUES ({1})", campos, rows))
                            : ConectorSQLite.Query(string.Format("UPDATE Surtidores SET Producto = ('{0}'), Precio = ('{1}'), DescProd = ('{2}') WHERE IdSurtidor = ({3}) AND Manguera = ('{4}')",
                                manguera.Producto.NumeroDeProducto,
                                manguera.Producto.PrecioUnitario.ToString(),
                                manguera.Producto.Descripcion,
                                surtidor.NumeroDeSurtidor,
                                letra));
                        _ = Log.Instance.WriteLog(string.Format("SURT: ({0}) Cem44: ({1}) Desc: ({2})", numeroDeManguera, surtidor.NumeroDeSurtidor + letra, manguera.Producto.Descripcion), Log.LogType.t_info);
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
                    string total = (Convert.ToDouble(tanques[i].VolumenProductoT) + Convert.ToDouble(tanques[i].VolumenVacioT) + Convert.ToDouble(tanques[i].VolumenAguaT)).ToString();
                    int res = ConectorSQLite.Query("UPDATE Tanques SET volumen = '" + tanques[i].VolumenProductoT +
                        "" + "', total = '" + total +
                        "" + "' WHERE id = " + tanques[i].NumeroDeTanque);

                    if (res == 0)
                    {
                        total = (Convert.ToDouble(tanques[i].VolumenProductoT) + Convert.ToDouble(tanques[i].VolumenVacioT) + Convert.ToDouble(tanques[i].VolumenAguaT)).ToString();
                        string campos = "id,volumen,total";
                        string rows = string.Format("{0},'{1}','{2}'",
                            tanques[i].NumeroDeTanque,
                            tanques[i].VolumenProductoT,
                            total);
                        _ = ConectorSQLite.Query(string.Format("INSERT INTO Tanques ({0}) VALUES ({1})", campos, rows));
                    }
                    _ = Log.Instance.WriteLog(string.Format("Tanque ({0}) Capacidad Maxima ({1}) Cantidad Actual ({2})", i, total, tanques[i].VolumenProductoT), Log.LogType.t_info);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error en el método traer tanques.\nExcepción: " + e.Message);
            }
        }
        public override void GrabarCierre()
        {
            _ = ConectorSQLite.Query("DELETE FROM cierreBandera");
            CierreDeTurno cierreDeTurno = conectorCEM.InfoTurnoEnCurso();
            try
            {
                string query = "INSERT INTO cierres (monto_contado, volumen_contado, monto_YPFruta, volumen_YPFruta) VALUES ({0})";
                string valores = string.Format(
                    "'{0}','{1}','{2}','{3}'",
                    cierreDeTurno.TotalesMediosDePago[0].TotalMonto.Trim().Substring(1),
                    cierreDeTurno.TotalesMediosDePago[0].TotalVolumen.Trim(),
                    cierreDeTurno.TotalesMediosDePago[3].TotalMonto.Trim(),
                    cierreDeTurno.TotalesMediosDePago[3].TotalVolumen.Trim());

                query = string.Format(query, valores);

                _ = ConectorSQLite.Query(query);

                // Traer ID del cierre para poder referenciar los detalles
                query = "SELECT max(id) FROM cierres";

                DataTable table = ConectorSQLite.Dt_query(query);

                int id = Convert.ToInt32(table.Rows[0][0]);

                // Grabar cierresxproducto
                query = "INSERT INTO cierresxProducto (id, producto, monto, volumen) VALUES (" + id + ", {0})";
                for (int i = 0; i < cierreDeTurno.TotalesPorProductosPorNivelesPorPeriodo[0][0].Count; i++)
                {
                    string aux =
                        (i + 1).ToString() + "," +
                        "'" + cierreDeTurno.TotalesPorProductosPorNivelesPorPeriodo[0][0][i].TotalMonto.Trim() + "'," +
                        "'" + cierreDeTurno.TotalesPorProductosPorNivelesPorPeriodo[0][0][i].TotalVolumen.Trim() + "'";

                    aux = string.Format(query, aux);

                    _ = ConectorSQLite.Query(aux);
                }

                Estacion estacion = Estacion.InstanciaEstacion;
                // Grabar cierresxmanguera
                query = "INSERT INTO cierresxManguera (id, surtidor, manguera, monto, volumen) VALUES (" + id + ", {0})";
                int contador = 0;
                for (int i = 0; i < estacion.NumeroDeSurtidores; i++)
                {
                    List<Surtidor> surtidores = estacion.NivelesDePrecio[0];
                    for (int j = 0; j < surtidores[i].TipoDeSurtidor; j++)
                    {
                        string aux =
                            (i + 1).ToString() + "," +
                            (j + 1).ToString() + "," +
                            "'" + cierreDeTurno.TotalPorMangueras[contador].TotalVntasMonto.Trim() + "'," +
                            "'" + cierreDeTurno.TotalPorMangueras[contador].TotalVntasVolumen.Trim() + "'";

                        contador++;

                        aux = string.Format(query, aux);

                        _ = ConectorSQLite.Query(aux);
                    }
                }
                _ = ConectorSQLite.Query("DELETE FROM despachos");
            }
            catch (Exception e)
            {
                throw new Exception($"Error en el metodo GrabarCierre. Excepcion: {e.Message}");
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
                    //List<InfoDespacho> infoDespachos = TablaDespachos.InstanciaDespachos.InfoDespachos;

                    if (despacho.NroUltimaVenta == 0 || despacho.IdUltimaVenta == null || despacho.IdUltimaVenta == "" || despacho.IdUltimaVenta.EndsWith("0000"))
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
                            Manguera = "",
                            Producto = "",
                            Monto = despacho.MontoUltimaVenta,
                            Volumen = despacho.VolumenUltimaVenta,
                            PPU = despacho.PpuUltimaVenta,
                            Facturado = Convert.ToInt32(despacho.UltimaVentaFacturada),
                            YPFRuta = 0,
                            Desc = ""
                        };
                        // Verificamos YPF en Ruta
                        foreach (Producto p in Estacion.InstanciaEstacion.Productos)
                        {
                            if (p.PrecioUnitario == infoDespacho.PPU)
                            {
                                if (p.NumeroPorDespacho == null || p.NumeroPorDespacho == "")
                                {
                                    p.NumeroPorDespacho = despacho.ProductoUltimaVenta.ToString();
                                }
                                infoDespacho.Producto = p.NumeroDeProducto;
                                infoDespacho.Desc = p.Descripcion;
                                if (despacho.UltimaVentaFacturada)
                                {
                                    infoDespacho.YPFRuta = 1;
                                }
                                break;
                            }
                        }
                        if (infoDespacho.Producto == "")
                        {
                            foreach (Producto p in Estacion.InstanciaEstacion.Productos)
                            {
                                if (p.NumeroPorDespacho != null && p.NumeroPorDespacho != "" && p.NumeroPorDespacho == despacho.ProductoUltimaVenta.ToString())
                                {
                                    infoDespacho.Producto = p.NumeroPorDespacho;
                                    infoDespacho.Desc = p.Descripcion;
                                }
                                else
                                {
                                    infoDespacho.Producto = despacho.ProductoUltimaVenta.ToString();
                                }
                                infoDespacho.YPFRuta = 1;
                            }
                        }
                        if (infoDespacho.Desc != "")
                        {
                            List<Manguera> tempManguera = Estacion.InstanciaEstacion.Surtidores[i - 1].Mangueras;
                            foreach (Manguera manguera in tempManguera)
                            {
                                if (infoDespacho.Desc.Equals(manguera.Producto.Descripcion))
                                {
                                    string letra = null;
                                    switch (manguera.NumeroDeManquera)
                                    {
                                        case 1:
                                            letra = "A";
                                            break;
                                        case 2:
                                            letra = "B";
                                            break;
                                        case 3:
                                            letra = "C";
                                            break;
                                        case 4:
                                            letra = "D";
                                            break;
                                        default:
                                            break;
                                    }
                                    infoDespacho.Manguera = letra;
                                }
                            }
                        }

                        /// Agregar a Base de Datos
                        bool despacho_pedido = false;
                        string campos = "id,surtidor,producto,monto,volumen,PPU,facturado,YPFruta,descripcion,manguera,despacho_pedido";
                        string rows = string.Format("'{0}',{1},'{2}','{3}','{4}','{5}',{6},{7},'{8}','{9}',{10}",
                            infoDespacho.ID,
                            infoDespacho.Surtidor,
                            infoDespacho.Producto,
                            infoDespacho.Monto,
                            infoDespacho.Volumen,
                            infoDespacho.PPU,
                            infoDespacho.Facturado,
                            infoDespacho.YPFRuta,
                            infoDespacho.Desc,
                            infoDespacho.Manguera,
                            despacho_pedido);
                        _ = ConectorSQLite.Query(string.Format("INSERT INTO Despachos ({0}) VALUES ({1})", campos, rows));
                    }

                    tabla = ConectorSQLite.Dt_query("SELECT * FROM Despachos WHERE id = '" + despacho.IdVentaAnterior + "' AND surtidor = " + i);

                    if (despacho.IdVentaAnterior == null || despacho.IdVentaAnterior == "")
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
                            Producto = "",
                            Monto = despacho.MontoVentaAnterior,
                            Volumen = despacho.VolumenVentaAnterios,
                            PPU = despacho.PpuVentaAnterior,
                            YPFRuta = 0,
                            Desc = ""
                        };
                        // Verificamos YPF en Ruta
                        foreach (Producto p in Estacion.InstanciaEstacion.Productos)
                        {
                            if (p.PrecioUnitario == infoDespacho.PPU)
                            {
                                if (p.NumeroPorDespacho == null || p.NumeroPorDespacho == "")
                                {
                                    p.NumeroPorDespacho = despacho.ProductoVentaAnterior.ToString();
                                }
                                infoDespacho.Producto = p.NumeroDeProducto;
                                infoDespacho.Desc = p.Descripcion;
                                if (despacho.VentaAnteriorFacturada)
                                {
                                    infoDespacho.YPFRuta = 1;
                                }
                                break;
                            }
                        }
                        if (infoDespacho.Producto == "")
                        {
                            foreach (Producto p in Estacion.InstanciaEstacion.Productos)
                            {
                                if (p.NumeroPorDespacho != null && p.NumeroPorDespacho != "" && p.NumeroPorDespacho == despacho.ProductoVentaAnterior.ToString())
                                {
                                    infoDespacho.Producto = p.NumeroPorDespacho;
                                    infoDespacho.Desc = p.Descripcion;
                                }
                                else
                                {
                                    infoDespacho.Producto = despacho.ProductoVentaAnterior.ToString();
                                }
                                infoDespacho.YPFRuta = 1;
                            }
                        }
                        if (infoDespacho.Desc != "")
                        {
                            List<Manguera> tempManguera = Estacion.InstanciaEstacion.Surtidores[i - 1].Mangueras;
                            foreach (Manguera manguera in tempManguera)
                            {
                                if (infoDespacho.Desc.Equals(manguera.Producto.Descripcion))
                                {
                                    string letra = null;
                                    switch (manguera.NumeroDeManquera)
                                    {
                                        case 1:
                                            letra = "A";
                                            break;
                                        case 2:
                                            letra = "B";
                                            break;
                                        case 3:
                                            letra = "C";
                                            break;
                                        case 4:
                                            letra = "D";
                                            break;
                                        default:
                                            break;
                                    }
                                    infoDespacho.Manguera = letra;
                                }
                            }
                        }
                        /// Agregar a Base de Datos
                        bool despacho_pedido = false;
                        string campos = "id,surtidor,producto,monto,volumen,PPU,facturado,YPFruta,descripcion,manguera,despacho_pedido";
                        string rows = string.Format("'{0}',{1},'{2}','{3}','{4}','{5}',{6},{7},'{8}','{9}',{10}",
                            infoDespacho.ID,
                            infoDespacho.Surtidor,
                            infoDespacho.Producto,
                            infoDespacho.Monto,
                            infoDespacho.Volumen,
                            infoDespacho.PPU,
                            infoDespacho.Facturado,
                            infoDespacho.YPFRuta,
                            infoDespacho.Desc,
                            infoDespacho.Manguera,
                            despacho_pedido);
                        _ = ConectorSQLite.Query(string.Format("INSERT INTO Despachos ({0}) VALUES ({1})", campos, rows));
                    }
                    // Cuando se hagan los cierres esto se debe eliminar
                    DataTable cantidadDeFilas = ConectorSQLite.Dt_query("SELECT * FROM Despachos");
                    if (cantidadDeFilas.Rows.Count >= 50)
                    {
                        _ = ConectorSQLite.Query(@"DELETE FROM Despachos WHERE id IN(SELECT id FROM Despachos ORDER BY fecha LIMIT 40)");
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"Error en el metodo GrabarDespachos. Excepcion: {e.Message}");
                }
            }
        }
    }
}
