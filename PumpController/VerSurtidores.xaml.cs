using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PumpController
{
    /// <summary>
    /// Lógica de interacción para VerSurtidores.xaml
    /// </summary>
    public partial class VerSurtidores : Window
    {
        public VerSurtidores()
        {
            InitializeComponent();
            Icon = new BitmapImage(new Uri("pack://application:,,,/PumpController;component/LogoSiges.ico"));
            List<ConfigEstacion> infoSurtidors = MostrarConfiguracion();
            DataGridDatos.ItemsSource = infoSurtidors;
        }
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private List<ConfigEstacion> MostrarConfiguracion()
        {
            Estacion estacion = Estacion.InstanciaEstacion;
            List<ConfigEstacion> infoSurtidores = new List<ConfigEstacion>();
            try
            {
                List<Surtidor> tempSurtidores = estacion.NivelesDePrecio[0];
                foreach (Surtidor surtidor in tempSurtidores)
                {
                    List<Manguera> tempManguera = surtidor.Mangueras;
                    foreach (Manguera manguera in tempManguera)
                    {
                        string letra = "A";
                        if (manguera.NumeroDeManguera == 2)
                        {
                            letra = "B";
                        }
                        else if (manguera.NumeroDeManguera == 3)
                        {
                            letra = "C";
                        }
                        else if (manguera.NumeroDeManguera == 4)
                        {
                            letra = "D";
                        }
                        infoSurtidores.Add(new ConfigEstacion(surtidor.NumeroDeSurtidor.ToString(), letra, manguera.Producto.NumeroDeProducto, manguera.Producto.PrecioUnitario, manguera.Producto.Descripcion));
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {

            }
            return infoSurtidores;
        }
    }
}
