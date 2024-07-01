using System.Collections.Generic;

namespace PumpController
{
    #region ESTACION
    public class Estacion
    {
        public readonly int NUM_MAX_NIVELES = 5;
        public Estacion()
        {
            NivelesDePrecio = new List<List<Surtidor>>();
            Surtidores = new List<Surtidor>();
            Tanques = new List<Tanque>();
            Productos = new List<Producto>();
        }
        // Instancia de Singleton
        public static Estacion InstanciaEstacion { get; } = new Estacion();
        public int NumeroDeSurtidores { get; set; }
        public int NumeroDeTanques { get; set; }
        public int NumeroDeProductos { get; set; }
        public int NumeroDeMangueras { get; set; }
        public List<List<Surtidor>> NivelesDePrecio { get; set; }
        public List<Surtidor> Surtidores { get; set; }
        public List<Tanque> Tanques { get; set; }
        public List<Producto> Productos { get; set; }
    }
    public class ConfigEstacion
    {
        public ConfigEstacion(string surt, string mang, string prod, string precio, string desc)
        {
            Surt = surt;
            Mang = mang;
            Prod = prod;
            Prec = precio;
            Desc = desc;
        }
        public string Surt { get; set; }
        public string Mang { get; set; }
        public string Prod { get; set; }
        public string Prec { get; set; }
        public string Desc { get; set; }
    }
    public class Surtidor
    {
        public Surtidor()
        {
            Mangueras = new List<Manguera>();
        }
        public int NivelDeSurtidor { get; set; }  // indica al nivel de precio al que trabaja
        public int TipoDeSurtidor { get; set; }   // indica la cantidad de mangueras que tiene ese surtidor
        public List<Manguera> Mangueras { get; set; }
        public int NumeroDeSurtidor { get; set; }

        class SurtidorPorNivel
        {
            public string NumeroDeNivel { get; set; }
            public string NumeroDeSurtidor { get; set; }
            public string TipoDeSurtidor { get; set; } // indica la cantidad de mangueras que tiene ese surtidor
        }
    }
    public class Tanque
    {
        public Tanque()
        {

        }
        public int NumeroDeTanque { get; set; }
        public int ProductoTanque { get; set; }
        public string VolumenProductoT { get; set; }
        public string VolumenAguaT { get; set; }
        public string VolumenVacioT { get; set; }
        public Producto Producto { get; set; }
    }
    /**
     * Es el "Tipo Surtidor X" y me dice la cantidad de mangueras
     */
    public class Manguera
    {
        public Manguera()
        { }
        public int NumeroDeManquera { get; set; }
        public Producto Producto { get; set; }
        public Tanque Tanque { get; set; }
    }
    public class Producto
    {
        public Producto()
        {
            Descripcion = "";
        }

        public string NumeroDeProducto { get; set; }
        public string NumeroPorDespacho { get; set; }
        public string PrecioUnitario { get; set; }
        public string Descripcion { get; set; }
    }
    #endregion
    #region DESPACHO
    public class Despacho
    {
        public Despacho()
        { }

        public enum ESTADO_SURTIDOR
        {
            DISPONIBLE,
            EN_SOLICITUD,
            DESPACHANDO,
            AUTORIZADO,
            VENTA_FINALIZADA_IMPAGA,
            DEFECTUOSO,
            ANULADO,
            DETENIDO
        }
        public ESTADO_SURTIDOR StatusUltimaVenta { get; set; }
        public int NroUltimaVenta { get; set; }
        public int ProductoUltimaVenta { get; set; }
        public string MontoUltimaVenta { get; set; }
        public string VolumenUltimaVenta { get; set; }
        public string PpuUltimaVenta { get; set; }
        public bool UltimaVentaFacturada { get; set; }
        public string IdUltimaVenta { get; set; }

        public ESTADO_SURTIDOR StatusVentaAnterior { get; set; }
        public int NroVentaAnterior { get; set; }
        public int ProductoVentaAnterior { get; set; }
        public string MontoVentaAnterior { get; set; }
        public string VolumenVentaAnterios { get; set; }
        public string PpuVentaAnterior { get; set; }
        public bool VentaAnteriorFacturada { get; set; }
        public string IdVentaAnterior { get; set; }
    }
    public class InfoDespacho
    {
        public InfoDespacho() { }
        public string ID { get; set; }
        public int Surtidor { get; set; }
        public string Manguera { get; set; }
        public string Producto { get; set; }
        public string Monto { get; set; }
        public string Volumen { get; set; }
        public string PPU { get; set; }
        public int Facturado { get; set; }
        public int YPFRuta { get; set; }
        public string Desc { get; set; }
    }
    #endregion
    #region CierreDeTurno
    public class CierreDeTurno
    {
        public CierreDeTurno()
        {
            TotalesMediosDePago = new List<TotalMedioDePago>();
            TotalesPorProductosPorNivelesPorPeriodo = new List<List<List<TotalPorProducto>>>();
            TotalPorMangueras = new List<TotalPorManguera>();
            Tanques = new List<TotalPorTanque>();
            ProductoEnTanques = new List<ProductoEnTanque>();
        }
        public List<TotalMedioDePago> TotalesMediosDePago { get; set; }
        public string Impuesto1 { get; set; }
        public string Impuesto2 { get; set; }
        public int PeriodoDePrecios { get; set; }
        public int NivelesDePrecios { get; set; }
        public List<List<List<TotalPorProducto>>> TotalesPorProductosPorNivelesPorPeriodo { get; set; }
        public List<TotalPorManguera> TotalPorMangueras { get; set; }
        public List<TotalPorTanque> Tanques { get; set; }
        public List<ProductoEnTanque> ProductoEnTanques { get; set; }
    }
    public class TotalMedioDePago
    {
        public TotalMedioDePago() { }
        public int NumeroMedioPago { get; set; }
        public string TotalMonto { get; set; }
        public string TotalVolumen { get; set; }
    }
    public class TotalPorProducto
    {
        public TotalPorProducto() { }
        public int Periodo { get; set; }
        public int Nivel { get; set; }
        public string NumeroDeProducto { get; set; }
        public string PrecioUnitario { get; set; }
        public string TotalMonto { get; set; }
        public string TotalVolumen { get; set; }
    }
    public class TotalPorManguera
    {
        public TotalPorManguera() { }
        public int NumeroDeManguera { get; set; }
        public string TotalVntasMonto { get; set; }
        public string TotalVntasVolumen { get; set; }
        public string TotalVntasSinControlMonto { get; set; }
        public string TotalVntasSinControlVolumen { get; set; }
        public string TotalPruebasMonto { get; set; }
        public string TotalPruebasVolumen { get; set; }
    }
    public class TotalPorTanque
    {
        public TotalPorTanque() { }
        public int NumeroDeTanque { get; set; }
        public string Producto { get; set; }
        public string Agua { get; set; }
        public string Vacio { get; set; }
        public string Capacidad { get; set; }
    }
    public class ProductoEnTanque
    {
        public ProductoEnTanque() { }
        public int NumeroDeProducto { get; set; }
        public string VolumenEnTanques { get; set; }
        public string AguaEnTanques { get; set; }
        public string VacioEnTanques { get; set; }
        public string CapacidadEnTanques { get; set; }
    }
    #endregion
}
