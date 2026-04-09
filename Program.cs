using System;
using System.Threading.Tasks;
using WitsmlODViewer.Witsml141;

namespace WitsmlODViewer
{
    class Program
    {
        const string DefaultConnectionString = "Server=.\\SQLExpress;Database=WitsmlData;Trusted_Connection=True;TrustServerCertificate=True;";

        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }

            var inputPath = args[0];
            if (!System.IO.File.Exists(inputPath))
            {
                Console.WriteLine($"Error: El archivo no existe: {inputPath}");
                return;
            }

            var connStr = Environment.GetEnvironmentVariable("WITSML_CONNECTION_STRING") ?? DefaultConnectionString;
            await ProcessAsync(inputPath, connStr);
        }

        static void ShowUsage()
        {
            Console.WriteLine("WITSML 1.4.1 — Procesador (importación a SQL Server)");
            Console.WriteLine();
            Console.WriteLine("Uso:");
            Console.WriteLine("  WitsmlODViewer <archivo.xml>");
            Console.WriteLine();
            Console.WriteLine("Variable de entorno opcional: WITSML_CONNECTION_STRING");
            Console.WriteLine("Base de datos por defecto: .\\SQLExpress, WitsmlData");
            Console.WriteLine("Crear BD: sqlcmd -S .\\SQLExpress -i Database\\CreateWitsmlDatabase.sql");
            Console.WriteLine();
            Console.WriteLine("WITSML 2.1 y ETP: repositorio / producto separado (ver Documentación).");
        }

        static async Task ProcessAsync(string filePath, string connectionString)
        {
            var processor = new Witsml141Processor();
            using (var repository = new Witsml141Repository(connectionString))
            {
                try
                {
                    Console.WriteLine($"Procesando: {filePath}");
                    await processor.ProcessWitsmlFileAsync(filePath, repository);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Error: {ex.Message}");
                }
            }
        }
    }
}
