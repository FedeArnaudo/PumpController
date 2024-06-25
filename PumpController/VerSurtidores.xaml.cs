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
            Icon = new BitmapImage(new Uri("pack://application:,,,/PumpController;component/LogoSiges24x24.ico"));
            List<ConfigEstacion> infoSurtidors = MostrarConfiguracion();
            DataGridDatos.ItemsSource = infoSurtidors;
        }
        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private List<ConfigEstacion> MostrarConfiguracion()
        {
            Estacion estacion = Estacion.InstanciaEstacion;
            List<ConfigEstacion> infoSurtidores = new List<ConfigEstacion>();
            List<Surtidor> tempSurtidores = estacion.nivelesDePrecio[0];
            foreach (Surtidor surtidor in tempSurtidores)
            {
                List<Manguera> tempManguera = surtidor.mangueras;
                foreach (Manguera manguera in tempManguera)
                {
                    string letra = "A";
                    if (manguera.numeroDeManquera == 2)
                    {
                        letra = "B";
                    }
                    else if (manguera.numeroDeManquera == 3)
                    {
                        letra = "C";
                    }
                    else if (manguera.numeroDeManquera == 4)
                    {
                        letra = "D";
                    }
                    infoSurtidores.Add(new ConfigEstacion(surtidor.numeroDeSurtidor.ToString(), letra, manguera.producto.numeroDeProducto, manguera.producto.precioUnitario, manguera.producto.descripcion));
                }
            }
            return infoSurtidores;
        }
    }
}
