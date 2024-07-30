using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace PumpController
{
    internal class ConectorSQLite
    {
        private static readonly string databaseName = "cds.db";
        private static readonly string connectionString = "Data Source='{0}';Version=3;";

        /// <summary>
        /// Método para ejecutar una query que devuelva una dataTable (como un select)
        /// </summary>
        /// <param name="query"> Comando a ejecutar </param>
        /// <returns> Una tabla con la respuesta al comando </returns>
        public static DataTable Dt_query(string query)
        {
            DataTable ret = new DataTable();
            using (SQLiteConnection db = new SQLiteConnection(
                string.Format(connectionString, Configuracion.LeerConfiguracion().ProyNuevoRuta + "\\CDS\\" + databaseName)))
            {
                db.Open();
                using (SQLiteCommand command = new SQLiteCommand(query, db))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        ret.Load(reader);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Método para ejecutar una query que no devuelve una tabla (como un update)
        /// </summary>
        /// <param name="query"> Comando a ejecutar </param>
        /// <returns> Cantidad de registros alterados </returns>
        public static int Query(string query)
        {
            int ret = 0;
            using (SQLiteConnection db = new SQLiteConnection(
                string.Format(connectionString, Configuracion.LeerConfiguracion().ProyNuevoRuta + "\\CDS\\" + databaseName)))
            {
                db.Open();
                using (SQLiteCommand command = new SQLiteCommand(query, db))
                {
                    ret = command.ExecuteNonQuery();
                }
            }
            return ret;
        }
        /// <summary>
        /// Método para obtener el precio de un producto según su ID
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>Precio del producto</returns>
        public static bool ComprobarCierre()
        {
            bool cierreFlag = false;

            string query = $"SELECT hacerCierre FROM cierreBandera LIMIT 1";

            using (SQLiteConnection db = new SQLiteConnection(string.Format(connectionString, Configuracion.LeerConfiguracion().ProyNuevoRuta + "\\CDS\\" + databaseName)))
            {
                db.Open();
                using (SQLiteCommand command = new SQLiteCommand(query, db))
                {
                    cierreFlag = Convert.ToBoolean(command.ExecuteScalar());
                }
            }
            return cierreFlag;
        }
        public static void CrearBBDD()
        {
            string folderPath = Configuracion.LeerConfiguracion().ProyNuevoRuta + "\\CDS";
            string databasePath = Path.Combine(folderPath, databaseName);

            // Crear la carpeta si no existe
            if (!Directory.Exists(folderPath))
            {
                _ = Directory.CreateDirectory(folderPath);
            }

            // Crear la base de datos si no existe
            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);
            }

            // Conectar a la base de datos
            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
            {
                connection.Open();

                // Crear las tablas si no existen
                string createTableQuery = "CREATE TABLE IF NOT EXISTS Surtidores " +
                                          "(IdSurtidor INTEGER, Manguera  INTEGER, Producto  INTEGER, " +
                                          "Precio REAL, DescProd TEXT)";
                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = command.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS Tanques (id INTEGER PRIMARY KEY, " +
                                   "volumen REAL NOT NULL, total REAL NOT NULL)";
                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = command.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS Despachos " +
                                   "(id INTEGER NOT NULL, surtidor INTEGER NOT NULL, " +
                                   "producto TEXT NOT NULL, monto REAL NOT NULL, " +
                                   "volumen REAL NOT NULL, PPU REAL NOT NULL, " +
                                   "facturado BLOB, fecha date DEFAULT(datetime('now', 'localtime')), " +
                                   "YPFruta BLOB, descripcion TEXT, manguera INTEGER, despacho_pedido BLOB, PRIMARY KEY(id,surtidor))";
                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = command.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS cierreBandera " +
                                   "(hacerCierre INTEGER NOT NULL);" +
                                   "\nINSERT INTO cierreBandera (hacerCierre) " +
                                   "SELECT 0 WHERE NOT EXISTS (SELECT 1 FROM cierreBandera)";
                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = command.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS Cierres " +
                                   "(id INTEGER, fecha date DEFAULT(datetime('now', 'localtime')), " +
                                   "monto_contado TEXT NOT NULL, volumen_contado TEXT NOT NULL, " +
                                   "monto_YPFruta TEXT NOT NULL, volumen_YPFruta TEXT NOT NULL, PRIMARY KEY(id AUTOINCREMENT))";
                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = command.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS CierresPorManguera " +
                                   "(id INTEGER NOT NULL, surtidor INTEGER NOT NULL, " +
                                   "manguera INTEGER NOT NULL, monto REAL NOT NULL, " +
                                   "volumen REAL NOT NULL)";
                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = command.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS CierresPorProducto " +
                                   "(id INTEGER NOT NULL, producto INTEGER NOT NULL, " +
                                   "monto REAL NOT NULL, volumen REAL NOT NULL)";
                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = command.ExecuteNonQuery();
                }

                createTableQuery = "CREATE TABLE IF NOT EXISTS Productos " +
                                   "(id_producto INTEGER PRIMARY KEY, numero_producto INTEGER NOT NULL, numero_despacho INTEGER NOT NULL," +
                                   "producto TEXT NOT NULL, precio REAL NOT NULL)";
                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    _ = command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
    }
}
