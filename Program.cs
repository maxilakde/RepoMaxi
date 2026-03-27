using System;
using System.Threading.Tasks;
using WitsmlODViewer.Services;

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

            // Modo conversión: WITSML 1.4.1.1 a 2.1
            if (args[0] == "--convert" || args[0] == "-c")
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Error: Especifica la ruta del archivo o directorio a convertir");
                    return;
                }
                await ConvertAsync(args[1]);
                return;
            }

            // Modo batch: convertir 1.4.1.1 → 2.1 y cargar en base de datos
            if (args[0] == "--load" || args[0] == "-l")
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Error: Especifica la ruta del directorio");
                    return;
                }
                var connectionString = Environment.GetEnvironmentVariable("WITSML_CONNECTION_STRING") ?? DefaultConnectionString;
                await ConvertAndLoadAsync(args[1], connectionString);
                return;
            }

            // Modo procesamiento: guardar en base de datos
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
            Console.WriteLine("WITSML - Conversor y Procesador");
            Console.WriteLine();
            Console.WriteLine("Uso:");
            Console.WriteLine("  1. Procesar archivo (guardar en base de datos SQL Server):");
            Console.WriteLine("     WitsmlODViewer <archivo.xml>");
            Console.WriteLine();
            Console.WriteLine("  2. Convertir WITSML 1.4.1.1 a 2.1:");
            Console.WriteLine("     WitsmlODViewer --convert <archivo-o-directorio>");
            Console.WriteLine("     WitsmlODViewer -c <archivo-o-directorio>");
            Console.WriteLine();
            Console.WriteLine("  3. Convertir directorio y cargar en base de datos:");
            Console.WriteLine("     WitsmlODViewer --load <directorio>");
            Console.WriteLine("     WitsmlODViewer -l <directorio>");
            Console.WriteLine();
            Console.WriteLine("Base de datos: .\\SQLExpress, WitsmlData");
            Console.WriteLine("Crear BD: sqlcmd -S .\\SQLExpress -i Database\\CreateWitsmlDatabase.sql");
        }

        static async Task ProcessAsync(string filePath, string connectionString)
        {
            var processor = new WitsmlProcessor();
            using (var repository = new WitsmlRepository(connectionString))
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

        static async Task ConvertAndLoadAsync(string inputDirectory, string connectionString)
        {
            if (!System.IO.Directory.Exists(inputDirectory))
            {
                Console.WriteLine($"Error: El directorio no existe: {inputDirectory}");
                return;
            }

            var converter = new WitsmlConverter();
            var outputDir = System.IO.Path.Combine(inputDirectory, "converted_v2.1");

            Console.WriteLine("=== Paso 1: Convertir WITSML 1.4.1.1 a 2.1 ===");
            var convertedCount = await converter.ConvertDirectoryAsync(inputDirectory);
            Console.WriteLine();

            Console.WriteLine("=== Paso 2: Cargar en base de datos ===");
            var xmlFiles = System.IO.Directory.GetFiles(outputDir, "*.xml", System.IO.SearchOption.AllDirectories);
            var processor = new WitsmlProcessor();
            int processedCount = 0;

            foreach (var file in xmlFiles)
            {
                try
                {
                    using (var repository = new WitsmlRepository(connectionString))
                    {
                        await processor.ProcessWitsmlFileAsync(file, repository);
                    }
                    processedCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Error: {System.IO.Path.GetFileName(file)}: {ex.Message}");
                }
            }

            Console.WriteLine();
            Console.WriteLine($"✓ Completado: {convertedCount} convertidos, {processedCount} archivos cargados en BD");
        }

        static async Task ConvertAsync(string inputPath)
        {
            Console.WriteLine("=== Conversor WITSML 1.4.1.1 a 2.1 ===");
            Console.WriteLine();

            var converter = new WitsmlConverter();

            try
            {
                if (System.IO.File.Exists(inputPath))
                {
                    Console.WriteLine($"Convirtiendo: {inputPath}");
                    var outputPath = await converter.ConvertFileAsync(inputPath);
                    Console.WriteLine($"✓ Guardado: {outputPath}");
                }
                else if (System.IO.Directory.Exists(inputPath))
                {
                    Console.WriteLine($"Convirtiendo directorio: {inputPath}");
                    var count = await converter.ConvertDirectoryAsync(inputPath);
                    Console.WriteLine($"✓ {count} archivo(s) convertido(s)");
                }
                else
                {
                    Console.WriteLine($"Error: La ruta no existe: {inputPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }
    }
}
