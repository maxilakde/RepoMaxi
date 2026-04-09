using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Witsml.Common;

namespace WitsmlODViewer.Services
{
    /// <summary>
    /// Cuenta archivos WITSML por tipo de objeto en un directorio y sus subcarpetas.
    /// </summary>
    public class WitsmlFileCounter
    {
        /// <summary>
        /// Carpeta legada (conversiones antiguas a 2.1). Se excluye del listado si existe.
        /// </summary>
        public const string ConvertedFolderName = "converted_v2.1";

        /// <summary>
        /// Tipos de objeto WITSML soportados con sus nombres de carpeta asociados.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> ObjectTypeToFolder = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "wells", "_wellInfo" },
            { "wellbores", "_wellboreInfo" },
            { "trajectories", "trajectory" },
            { "mudLogs", "mudLog" },
            { "logs", "log" },
            { "rigs", "rig" },
            { "tubulars", "tubular" },
            { "wbGeometrys", "wbGeometry" },
            { "bhaRuns", "bhaRun" },
            { "messages", "message" },
            { "attachments", "attachment" },
            { "formationMarkers", "formationMarker" },
            { "trajectoryStations", "trajectoryStation" },
            { "tubularComponents", "tubularComponent" },
            { "wbGeometrySections", "wbGeometrySection" },
            { "geologyIntervals", "geologyInterval" },
            { "lithologies", "lithology" }
        };

        /// <summary>
        /// Nombres amigables para mostrar en la UI.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> ObjectTypeDisplayNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "wells", "Wells" },
            { "wellbores", "Wellbores" },
            { "trajectories", "Trajectories" },
            { "mudLogs", "Mud Logs" },
            { "logs", "Logs" },
            { "rigs", "Rigs" },
            { "tubulars", "Tubulars" },
            { "wbGeometrys", "WB Geometrys" },
            { "bhaRuns", "BHA Runs" },
            { "messages", "Messages" },
            { "attachments", "Adjuntos" },
            { "formationMarkers", "Formation Markers" },
            { "trajectoryStations", "Estaciones de trayectoria" },
            { "tubularComponents", "Componentes de tubular" },
            { "wbGeometrySections", "Secciones WB Geometry" },
            { "geologyIntervals", "Intervalos geológicos" },
            { "lithologies", "Litologías" }
        };

        private readonly string _basePath;

        public WitsmlFileCounter(string basePath)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        }

        /// <summary>
        /// Cuenta archivos XML por nombre de carpeta (más rápido).
        /// Una ruta cuenta si contiene la carpeta como segmento (ej: ...\log\1\2.xml para logs).
        /// Para "log" se evita "mudLog" comprobando que el segmento sea exactamente "log".
        /// </summary>
        public int CountByFolder(string objectType, CancellationToken cancellationToken = default)
        {
            if (!ObjectTypeToFolder.TryGetValue(objectType, out var folderName))
                return 0;

            if (!Directory.Exists(_basePath))
                return 0;

            var count = 0;
            var sep = Path.DirectorySeparatorChar;

            foreach (var file in Directory.EnumerateFiles(_basePath, "*.xml", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var path = file;
                if (IsUnderConvertedFolder(path))
                    continue;

                if (folderName == "log")
                {
                    // Evitar mudLog: buscar "\log\" como segmento completo
                    var parts = path.Split(sep);
                    if (Array.Exists(parts, p => string.Equals(p, "log", StringComparison.OrdinalIgnoreCase)))
                        count++;
                }
                else
                {
                    if (path.Contains(sep + folderName + sep, StringComparison.OrdinalIgnoreCase) ||
                        path.EndsWith(sep + folderName, StringComparison.OrdinalIgnoreCase))
                        count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Cuenta archivos XML verificando el elemento raíz (más preciso, más lento).
        /// </summary>
        public async Task<int> CountByContentAsync(string objectType, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
        {
            var expectedRoot = objectType.ToLowerInvariant();
            if (!ObjectTypeToFolder.ContainsKey(objectType))
                return 0;

            if (!Directory.Exists(_basePath))
                return 0;

            var files = Directory.EnumerateFiles(_basePath, "*.xml", SearchOption.AllDirectories).ToList();
            var count = 0;
            var total = files.Count;

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (IsUnderConvertedFolder(file))
                    continue;

                try
                {
                    using var reader = File.OpenText(file);
                    var firstChunk = new char[2048];
                    var n = await reader.ReadBlockAsync(firstChunk, 0, firstChunk.Length);
                    var content = new string(firstChunk, 0, n);

                    // Buscar elemento raíz sin cargar el XML completo
                    var rootMatch = System.Text.RegularExpressions.Regex.Match(
                        content,
                        @"<(\w+)(?:\s|>)",
                        System.Text.RegularExpressions.RegexOptions.Singleline);

                    if (rootMatch.Success)
                    {
                        var rootLocal = rootMatch.Groups[1].Value.ToLowerInvariant();
                        if (rootLocal == expectedRoot || (expectedRoot == "trajectories" && rootLocal == "trajectorys"))
                            count++;
                    }
                }
                catch { /* ignorar archivos corruptos */ }

                progress?.Report(count);
            }

            return count;
        }

        /// <summary>
        /// Obtiene todos los archivos XML del tipo indicado (por carpeta).
        /// </summary>
        public IEnumerable<string> GetFilesForType(string objectType, CancellationToken cancellationToken = default)
        {
            if (!ObjectTypeToFolder.TryGetValue(objectType, out var folderName))
                yield break;

            if (!Directory.Exists(_basePath))
                yield break;

            var sep = Path.DirectorySeparatorChar;

            foreach (var file in Directory.EnumerateFiles(_basePath, "*.xml", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var path = file;
                if (IsUnderConvertedFolder(path))
                    continue;

                var match = false;

                if (folderName == "log")
                {
                    var parts = path.Split(sep);
                    match = Array.Exists(parts, p => string.Equals(p, "log", StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    match = path.Contains(sep + folderName + sep, StringComparison.OrdinalIgnoreCase) ||
                            path.EndsWith(sep + folderName, StringComparison.OrdinalIgnoreCase);
                }

                if (match)
                    yield return file;
            }
        }

        private static bool IsUnderConvertedFolder(string filePath)
        {
            var parts = filePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return Array.Exists(parts, p => string.Equals(p, ConvertedFolderName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Obtiene archivos que realmente contienen el tipo de objeto (verifica elemento raíz).
        /// Evita procesar archivos en la carpeta "rig" que tienen root &lt;wells&gt; u otro tipo.
        /// </summary>
        public IEnumerable<string> GetFilesForTypeByContent(string objectType, CancellationToken cancellationToken = default)
        {
            var expectedRoot = objectType.ToLowerInvariant();
            foreach (var file in GetFilesForType(objectType, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!HasMatchingRoot(file, expectedRoot))
                    continue;
                yield return file;
            }
        }

        /// <summary>
        /// Cuenta archivos que realmente contienen el tipo de objeto (por carpeta + contenido).
        /// </summary>
        public int CountByContentInFolder(string objectType, CancellationToken cancellationToken = default)
        {
            var count = 0;
            foreach (var _ in GetFilesForTypeByContent(objectType, cancellationToken))
                count++;
            return count;
        }

        private const int VersionCheckChunkSize = 8192;

        /// <summary>
        /// Cuenta archivos WITSML 1.4.1 (1series) del tipo indicado; excluye 2.x.
        /// </summary>
        public int CountWitsml141ByContent(string objectType, CancellationToken cancellationToken = default)
        {
            var expectedRoot = objectType.ToLowerInvariant();
            if (!ObjectTypeToFolder.ContainsKey(objectType))
                return 0;

            var count = 0;
            foreach (var file in GetFilesForType(objectType, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var chunk = ReadFirstChunk(file, VersionCheckChunkSize);
                    if (!ContentMatchesRoot(chunk, expectedRoot))
                        continue;
                    if (WitsmlXmlVersionDetector.IsWitsml21FromChunk(chunk))
                        continue;
                    count++;
                }
                catch { /* ignorar archivos corruptos */ }
            }

            return count;
        }

        private static string ReadFirstChunk(string filePath, int size)
        {
            using var reader = File.OpenText(filePath);
            var buffer = new char[size];
            var n = reader.Read(buffer, 0, size);
            return new string(buffer, 0, n);
        }

        private static bool ContentMatchesRoot(string content, string expectedRoot)
        {
            var rootMatch = System.Text.RegularExpressions.Regex.Match(
                content, @"<(\w+)(?:\s|>)", System.Text.RegularExpressions.RegexOptions.Singleline);
            if (!rootMatch.Success) return false;
            var rootLocal = rootMatch.Groups[1].Value.ToLowerInvariant();
            return rootLocal == expectedRoot || (expectedRoot == "trajectories" && rootLocal == "trajectorys");
        }

        private static bool HasMatchingRoot(string filePath, string expectedRoot)
        {
            try
            {
                using var reader = File.OpenText(filePath);
                var firstChunk = new char[2048];
                var n = reader.Read(firstChunk, 0, firstChunk.Length);
                var content = new string(firstChunk, 0, n);
                var rootMatch = System.Text.RegularExpressions.Regex.Match(
                    content, @"<(\w+)(?:\s|>)", System.Text.RegularExpressions.RegexOptions.Singleline);
                if (!rootMatch.Success) return false;
                var rootLocal = rootMatch.Groups[1].Value.ToLowerInvariant();
                return rootLocal == expectedRoot || (expectedRoot == "trajectories" && rootLocal == "trajectorys");
            }
            catch { return false; }
        }
    }
}
