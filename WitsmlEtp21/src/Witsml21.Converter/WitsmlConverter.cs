using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cabl.Witsml.Common;

namespace Witsml21.Converter
{
    /// <summary>
    /// Convierte archivos WITSML 1.4.1.1 a 2.1 (producto separado del visor 1.4.1).
    /// </summary>
    public class WitsmlConverter
    {
        /// <summary>Indica si el contenido XML es WITSML 2.x y no requiere conversión.</summary>
        public static bool IsWitsml21(string xmlContent) => WitsmlXmlVersionDetector.IsWitsml21(xmlContent);

        /// <summary>Versión rápida: primeros bytes del archivo.</summary>
        public static bool IsWitsml21FromChunk(string xmlChunk) => WitsmlXmlVersionDetector.IsWitsml21FromChunk(xmlChunk);

        private static readonly XNamespace Witsml1411 = XNamespace.Get(WitsmlNamespaces.Witsml141Series);
        private static readonly XNamespace Witsml21Ns = XNamespace.Get(WitsmlNamespaces.EnergyMlWitsmlV2);

        /// <summary>
        /// Convierte un archivo XML de Witsml 1.4.1.1 a 2.1
        /// </summary>
        public async Task<string> ConvertFileAsync(string inputPath, string? outputPath = null)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException($"El archivo no existe: {inputPath}");

            Console.WriteLine($"Convirtiendo archivo: {Path.GetFileName(inputPath)}");

            var xmlContent = await File.ReadAllTextAsync(inputPath);
            var convertedXml = ConvertXml(xmlContent);

            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = Path.Combine(
                    Path.GetDirectoryName(inputPath) ?? "",
                    Path.GetFileNameWithoutExtension(inputPath) + "_v2.1.xml"
                );
            }

            await File.WriteAllTextAsync(outputPath, convertedXml);
            Console.WriteLine($"✓ Archivo convertido guardado en: {outputPath}");

            return outputPath;
        }

        /// <summary>
        /// Convierte el contenido XML de Witsml 1.4.1.1 a 2.1
        /// </summary>
        public string ConvertXml(string xmlContent)
        {
            try
            {
                var doc = XDocument.Parse(xmlContent);
                var root = doc.Root;

                if (root == null)
                    throw new InvalidOperationException("El documento XML no tiene elemento raíz");

                // Determinar el tipo de objeto Witsml
                var localName = root.Name.LocalName.ToLower();
                XElement? convertedRoot = null;

                switch (localName)
                {
                    case "wells":
                        convertedRoot = ConvertWells(root);
                        break;
                    case "wellbores":
                        convertedRoot = ConvertWellbores(root);
                        break;
                    case "trajectorys":
                    case "trajectories":
                        convertedRoot = ConvertTrajectories(root);
                        break;
                    case "mudlogs":
                        convertedRoot = ConvertMudLogs(root);
                        break;
                    case "logs":
                        convertedRoot = ConvertLogs(root);
                        break;
                    case "rigs":
                        convertedRoot = ConvertRigs(root);
                        break;
                    case "tubulars":
                        convertedRoot = ConvertTubulars(root);
                        break;
                    case "wbgeometrys":
                        convertedRoot = ConvertWbGeometrys(root);
                        break;
                    case "bharuns":
                        convertedRoot = ConvertBhaRuns(root);
                        break;
                    case "messages":
                        convertedRoot = ConvertMessages(root);
                        break;
                    case "attachments":
                        convertedRoot = ConvertAttachments(root);
                        break;
                    case "formationmarkers":
                        convertedRoot = ConvertFormationMarkers(root);
                        break;
                    case "trajectorystations":
                        convertedRoot = ConvertGeneric(root);
                        break;
                    case "geologyintervals":
                        convertedRoot = ConvertGeneric(root);
                        break;
                    case "lithologies":
                        convertedRoot = ConvertGeneric(root);
                        break;
                    case "tubularcomponents":
                        convertedRoot = ConvertGeneric(root);
                        break;
                    case "wbgeometrysections":
                        convertedRoot = ConvertGeneric(root);
                        break;
                    default:
                        Console.WriteLine($"⚠ Tipo de objeto no reconocido: {localName}. Intentando conversión genérica...");
                        convertedRoot = ConvertGeneric(root);
                        break;
                }

                if (convertedRoot == null)
                    throw new InvalidOperationException($"No se pudo convertir el elemento: {localName}");

                var result = new XDocument(new XDeclaration("1.0", "UTF-8", null), convertedRoot);
                return result.ToString(SaveOptions.None);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al convertir XML: {ex.Message}", ex);
            }
        }

        private XElement ConvertWells(XElement root)
        {
            var wells = new XElement(Witsml21Ns + "wells",
                new XAttribute("version", "2.1")
            );

            foreach (var well in root.Elements(Witsml1411 + "well"))
            {
                var convertedWell = new XElement(Witsml21Ns + "well");

                // Copiar atributos
                foreach (var attr in well.Attributes())
                {
                    if (attr.Name.LocalName == "uid")
                        convertedWell.Add(new XAttribute("uid", attr.Value));
                }

                // Convertir elementos
                ConvertElement(well, convertedWell, "uid", "uid");
                ConvertElement(well, convertedWell, "name", "name");
                ConvertElement(well, convertedWell, "timeZone", "timeZone");
                ConvertElement(well, convertedWell, "statusWell", "statusWell");
                ConvertElement(well, convertedWell, "country", "country");
                ConvertElement(well, convertedWell, "state", "state");
                ConvertElement(well, convertedWell, "county", "county");
                ConvertElement(well, convertedWell, "field", "field");

                // Convertir commonData
                var commonData = well.Element(Witsml1411 + "commonData");
                if (commonData != null)
                {
                    var convertedCommonData = new XElement(Witsml21Ns + "commonData");
                    ConvertElement(commonData, convertedCommonData, "dTimCreation", "dTimCreation");
                    ConvertElement(commonData, convertedCommonData, "dTimLastChange", "dTimLastChange");
                    if (convertedCommonData.HasElements || convertedCommonData.HasAttributes)
                        convertedWell.Add(convertedCommonData);
                }

                wells.Add(convertedWell);
            }

            return wells;
        }

        private XElement ConvertWellbores(XElement root)
        {
            var wellbores = new XElement(Witsml21Ns + "wellbores",
                new XAttribute("version", "2.1")
            );

            foreach (var wellbore in root.Elements(Witsml1411 + "wellbore"))
            {
                var convertedWellbore = new XElement(Witsml21Ns + "wellbore");

                // Copiar atributos
                foreach (var attr in wellbore.Attributes())
                {
                    if (attr.Name.LocalName == "uid")
                        convertedWellbore.Add(new XAttribute("uid", attr.Value));
                    else if (attr.Name.LocalName == "uidWell")
                    {
                        // En Witsml 2.1, uidWell se convierte en un elemento well dentro de wellbore
                        var wellRef = new XElement(Witsml21Ns + "well");
                        wellRef.Add(new XAttribute("uid", attr.Value));
                        convertedWellbore.Add(wellRef);
                    }
                }

                // Convertir elementos
                ConvertElement(wellbore, convertedWellbore, "name", "name");
                ConvertElement(wellbore, convertedWellbore, "isActive", "isActive");

                // nameWell ya está manejado por el uidWell en el atributo

                // Convertir commonData
                var commonData = wellbore.Element(Witsml1411 + "commonData");
                if (commonData != null)
                {
                    var convertedCommonData = new XElement(Witsml21Ns + "commonData");
                    ConvertElement(commonData, convertedCommonData, "dTimCreation", "dTimCreation");
                    ConvertElement(commonData, convertedCommonData, "dTimLastChange", "dTimLastChange");
                    if (convertedCommonData.HasElements || convertedCommonData.HasAttributes)
                        convertedWellbore.Add(convertedCommonData);
                }

                wellbores.Add(convertedWellbore);
            }

            return wellbores;
        }

        private XElement ConvertTrajectories(XElement root)
        {
            var trajectories = new XElement(Witsml21Ns + "trajectories",
                new XAttribute("version", "2.1")
            );

            foreach (var trajectory in root.Elements(Witsml1411 + "trajectory"))
            {
                var convertedTrajectory = new XElement(Witsml21Ns + "trajectory");

                // Copiar atributos
                foreach (var attr in trajectory.Attributes())
                {
                    if (attr.Name.LocalName == "uid")
                        convertedTrajectory.Add(new XAttribute("uid", attr.Value));
                    else if (attr.Name.LocalName == "uidWell")
                    {
                        var wellRef = new XElement(Witsml21Ns + "well");
                        wellRef.Add(new XAttribute("uid", attr.Value));
                        convertedTrajectory.Add(wellRef);
                    }
                    else if (attr.Name.LocalName == "uidWellbore")
                    {
                        var wellboreRef = new XElement(Witsml21Ns + "wellbore");
                        wellboreRef.Add(new XAttribute("uid", attr.Value));
                        convertedTrajectory.Add(wellboreRef);
                    }
                }

                // Convertir elementos básicos
                ConvertElement(trajectory, convertedTrajectory, "name", "name");
                ConvertElement(trajectory, convertedTrajectory, "objectGrowing", "objectGrowing");
                ConvertElement(trajectory, convertedTrajectory, "dTimTrajStart", "dTimTrajStart");
                ConvertElement(trajectory, convertedTrajectory, "dTimTrajEnd", "dTimTrajEnd");
                ConvertElement(trajectory, convertedTrajectory, "mdMn", "mdMn");
                ConvertElement(trajectory, convertedTrajectory, "mdMx", "mdMx");
                ConvertElement(trajectory, convertedTrajectory, "serviceCompany", "serviceCompany");
                ConvertElement(trajectory, convertedTrajectory, "gridCorUsed", "gridCorUsed");
                ConvertElement(trajectory, convertedTrajectory, "aziVertSect", "aziVertSect");
                ConvertElement(trajectory, convertedTrajectory, "memory", "memory");
                ConvertElement(trajectory, convertedTrajectory, "aziRef", "aziRef");

                // Convertir trajectoryStations
                foreach (var station in trajectory.Elements(Witsml1411 + "trajectoryStation"))
                {
                    var convertedStation = new XElement(Witsml21Ns + "trajectoryStation");
                    
                    foreach (var attr in station.Attributes())
                    {
                        if (attr.Name.LocalName == "uid")
                            convertedStation.Add(new XAttribute("uid", attr.Value));
                    }

                    // Convertir elementos de la estación
                    ConvertElement(station, convertedStation, "dTimStn", "dTimStn");
                    ConvertElement(station, convertedStation, "typeTrajStation", "typeTrajStation");
                    ConvertElement(station, convertedStation, "md", "md");
                    ConvertElement(station, convertedStation, "tvd", "tvd");
                    ConvertElement(station, convertedStation, "incl", "incl");
                    ConvertElement(station, convertedStation, "azi", "azi");
                    ConvertElement(station, convertedStation, "dispNs", "dispNs");
                    ConvertElement(station, convertedStation, "dispEw", "dispEw");
                    ConvertElement(station, convertedStation, "vertSect", "vertSect");
                    ConvertElement(station, convertedStation, "dls", "dls");
                    ConvertElement(station, convertedStation, "rateTurn", "rateTurn");
                    ConvertElement(station, convertedStation, "rateBuild", "rateBuild");
                    ConvertElement(station, convertedStation, "mdDelta", "mdDelta");
                    ConvertElement(station, convertedStation, "tvdDelta", "tvdDelta");
                    ConvertElement(station, convertedStation, "statusTrajStation", "statusTrajStation");

                    // Convertir corUsed si existe
                    var corUsed = station.Element(Witsml1411 + "corUsed");
                    if (corUsed != null)
                    {
                        var convertedCorUsed = new XElement(Witsml21Ns + "corUsed");
                        foreach (var child in corUsed.Elements())
                        {
                            var convertedChild = new XElement(Witsml21Ns + child.Name.LocalName);
                            CopyAttributesAndValue(child, convertedChild);
                            convertedCorUsed.Add(convertedChild);
                        }
                        convertedStation.Add(convertedCorUsed);
                    }

                    // Convertir commonData
                    var commonData = station.Element(Witsml1411 + "commonData");
                    if (commonData != null)
                    {
                        var convertedCommonData = new XElement(Witsml21Ns + "commonData");
                        ConvertElement(commonData, convertedCommonData, "dTimCreation", "dTimCreation");
                        ConvertElement(commonData, convertedCommonData, "dTimLastChange", "dTimLastChange");
                        if (convertedCommonData.HasElements || convertedCommonData.HasAttributes)
                            convertedStation.Add(convertedCommonData);
                    }

                    convertedTrajectory.Add(convertedStation);
                }

                // Convertir commonData del trajectory
                var trajCommonData = trajectory.Element(Witsml1411 + "commonData");
                if (trajCommonData != null)
                {
                    var convertedCommonData = new XElement(Witsml21Ns + "commonData");
                    ConvertElement(trajCommonData, convertedCommonData, "dTimCreation", "dTimCreation");
                    ConvertElement(trajCommonData, convertedCommonData, "dTimLastChange", "dTimLastChange");
                    if (convertedCommonData.HasElements || convertedCommonData.HasAttributes)
                        convertedTrajectory.Add(convertedCommonData);
                }

                trajectories.Add(convertedTrajectory);
            }

            return trajectories;
        }

        private XElement ConvertMudLogs(XElement root)
        {
            var mudLogs = new XElement(Witsml21Ns + "mudLogs",
                new XAttribute("version", "2.1")
            );

            foreach (var mudLog in root.Elements(Witsml1411 + "mudLog"))
            {
                var convertedMudLog = new XElement(Witsml21Ns + "mudLog");

                // Copiar atributos
                foreach (var attr in mudLog.Attributes())
                {
                    if (attr.Name.LocalName == "uid")
                        convertedMudLog.Add(new XAttribute("uid", attr.Value));
                    else if (attr.Name.LocalName == "uidWell")
                    {
                        var wellRef = new XElement(Witsml21Ns + "well");
                        wellRef.Add(new XAttribute("uid", attr.Value));
                        convertedMudLog.Add(wellRef);
                    }
                    else if (attr.Name.LocalName == "uidWellbore")
                    {
                        var wellboreRef = new XElement(Witsml21Ns + "wellbore");
                        wellboreRef.Add(new XAttribute("uid", attr.Value));
                        convertedMudLog.Add(wellboreRef);
                    }
                }

                // Convertir elementos básicos
                ConvertElement(mudLog, convertedMudLog, "name", "name");
                ConvertElement(mudLog, convertedMudLog, "objectGrowing", "objectGrowing");
                ConvertElement(mudLog, convertedMudLog, "dTim", "dTim");
                ConvertElement(mudLog, convertedMudLog, "mudLogCompany", "mudLogCompany");
                ConvertElement(mudLog, convertedMudLog, "mudLogEngineers", "mudLogEngineers");
                ConvertElement(mudLog, convertedMudLog, "startMd", "startMd");
                ConvertElement(mudLog, convertedMudLog, "endMd", "endMd");

                // Convertir geologyIntervals
                foreach (var interval in mudLog.Elements(Witsml1411 + "geologyInterval"))
                {
                    var convertedInterval = new XElement(Witsml21Ns + "geologyInterval");
                    
                    foreach (var attr in interval.Attributes())
                    {
                        if (attr.Name.LocalName == "uid")
                            convertedInterval.Add(new XAttribute("uid", attr.Value));
                    }

                    ConvertElement(interval, convertedInterval, "typeLithology", "typeLithology");
                    ConvertElement(interval, convertedInterval, "mdTop", "mdTop");
                    ConvertElement(interval, convertedInterval, "mdBottom", "mdBottom");

                    // Convertir lithologies
                    foreach (var lithology in interval.Elements(Witsml1411 + "lithology"))
                    {
                        var convertedLithology = new XElement(Witsml21Ns + "lithology");
                        foreach (var attr in lithology.Attributes())
                        {
                            if (attr.Name.LocalName == "uid")
                                convertedLithology.Add(new XAttribute("uid", attr.Value));
                        }
                        ConvertElement(lithology, convertedLithology, "type", "type");
                        ConvertElement(lithology, convertedLithology, "codeLith", "codeLith");
                        ConvertElement(lithology, convertedLithology, "lithPc", "lithPc");
                        ConvertElement(lithology, convertedLithology, "description", "description");
                        convertedInterval.Add(convertedLithology);
                    }

                    // Convertir commonTime a commonData
                    var commonTime = interval.Element(Witsml1411 + "commonTime");
                    if (commonTime != null)
                    {
                        var convertedCommonData = new XElement(Witsml21Ns + "commonData");
                        ConvertElement(commonTime, convertedCommonData, "dTimCreation", "dTimCreation");
                        ConvertElement(commonTime, convertedCommonData, "dTimLastChange", "dTimLastChange");
                        if (convertedCommonData.HasElements || convertedCommonData.HasAttributes)
                            convertedInterval.Add(convertedCommonData);
                    }

                    convertedMudLog.Add(convertedInterval);
                }

                // Convertir commonData
                var commonData = mudLog.Element(Witsml1411 + "commonData");
                if (commonData != null)
                {
                    var convertedCommonData = new XElement(Witsml21Ns + "commonData");
                    ConvertElement(commonData, convertedCommonData, "dTimCreation", "dTimCreation");
                    ConvertElement(commonData, convertedCommonData, "dTimLastChange", "dTimLastChange");
                    if (convertedCommonData.HasElements || convertedCommonData.HasAttributes)
                        convertedMudLog.Add(convertedCommonData);
                }

                mudLogs.Add(convertedMudLog);
            }

            return mudLogs;
        }

        private XElement ConvertLogs(XElement root)
        {
            // Similar a mudLogs pero para logs
            var logs = new XElement(Witsml21Ns + "logs",
                new XAttribute("version", "2.1")
            );

            foreach (var log in root.Elements(Witsml1411 + "log"))
            {
                var convertedLog = new XElement(Witsml21Ns + "log");

                // Copiar atributos
                foreach (var attr in log.Attributes())
                {
                    if (attr.Name.LocalName == "uid")
                        convertedLog.Add(new XAttribute("uid", attr.Value));
                    else if (attr.Name.LocalName == "uidWell")
                    {
                        var wellRef = new XElement(Witsml21Ns + "well");
                        wellRef.Add(new XAttribute("uid", attr.Value));
                        convertedLog.Add(wellRef);
                    }
                    else if (attr.Name.LocalName == "uidWellbore")
                    {
                        var wellboreRef = new XElement(Witsml21Ns + "wellbore");
                        wellboreRef.Add(new XAttribute("uid", attr.Value));
                        convertedLog.Add(wellboreRef);
                    }
                }

                // Convertir elementos básicos (simplificado - puede necesitar más campos)
                ConvertElement(log, convertedLog, "name", "name");
                ConvertElement(log, convertedLog, "indexType", "indexType");
                ConvertElement(log, convertedLog, "direction", "direction");
                ConvertElement(log, convertedLog, "objectGrowing", "objectGrowing");

                // Convertir commonData
                var commonData = log.Element(Witsml1411 + "commonData");
                if (commonData != null)
                {
                    var convertedCommonData = new XElement(Witsml21Ns + "commonData");
                    ConvertElement(commonData, convertedCommonData, "dTimCreation", "dTimCreation");
                    ConvertElement(commonData, convertedCommonData, "dTimLastChange", "dTimLastChange");
                    if (convertedCommonData.HasElements || convertedCommonData.HasAttributes)
                        convertedLog.Add(convertedCommonData);
                }

                logs.Add(convertedLog);
            }

            return logs;
        }

        private XElement ConvertRigs(XElement root)
        {
            var rigs = new XElement(Witsml21Ns + "rigs", new XAttribute("version", "2.1"));

            foreach (var rig in root.Elements(Witsml1411 + "rig"))
            {
                var convertedRig = new XElement(Witsml21Ns + "rig");

                foreach (var attr in rig.Attributes())
                {
                    if (attr.Name.LocalName == "uid")
                        convertedRig.Add(new XAttribute("uid", attr.Value));
                    else if (attr.Name.LocalName == "uidWell")
                    {
                        var wellRef = new XElement(Witsml21Ns + "well");
                        wellRef.Add(new XAttribute("uid", attr.Value));
                        convertedRig.Add(wellRef);
                    }
                    else if (attr.Name.LocalName == "uidWellbore")
                    {
                        var wellboreRef = new XElement(Witsml21Ns + "wellbore");
                        wellboreRef.Add(new XAttribute("uid", attr.Value));
                        convertedRig.Add(wellboreRef);
                    }
                }

                ConvertElement(rig, convertedRig, "name", "name");
                ConvertElement(rig, convertedRig, "owner", "owner");
                ConvertElement(rig, convertedRig, "typeRig", "typeRig");

                // Convertir bop (estructura anidada simplificada)
                var bop = rig.Element(Witsml1411 + "bop");
                if (bop != null)
                    convertedRig.Add(ConvertElementGeneric(bop));

                // Convertir pits y pumps
                foreach (var pit in rig.Elements(Witsml1411 + "pit"))
                    convertedRig.Add(ConvertElementGeneric(pit));
                foreach (var pump in rig.Elements(Witsml1411 + "pump"))
                    convertedRig.Add(ConvertElementGeneric(pump));

                var surfaceEquipment = rig.Element(Witsml1411 + "surfaceEquipment");
                if (surfaceEquipment != null)
                    convertedRig.Add(ConvertElementGeneric(surfaceEquipment));

                ConvertElement(rig, convertedRig, "ratingDerrick", "ratingDerrick");
                ConvertElement(rig, convertedRig, "htDerrick", "htDerrick");
                ConvertElement(rig, convertedRig, "wtBlock", "wtBlock");
                ConvertElement(rig, convertedRig, "typeDrawWorks", "typeDrawWorks");
                ConvertElement(rig, convertedRig, "powerDrawWorks", "powerDrawWorks");
                ConvertElement(rig, convertedRig, "ratingTqRotSys", "ratingTqRotSys");
                ConvertElement(rig, convertedRig, "ratingRotSystem", "ratingRotSystem");

                var commonData = rig.Element(Witsml1411 + "commonData");
                if (commonData != null)
                {
                    var convertedCommonData = new XElement(Witsml21Ns + "commonData");
                    ConvertElement(commonData, convertedCommonData, "dTimCreation", "dTimCreation");
                    ConvertElement(commonData, convertedCommonData, "dTimLastChange", "dTimLastChange");
                    if (convertedCommonData.HasElements || convertedCommonData.HasAttributes)
                        convertedRig.Add(convertedCommonData);
                }

                rigs.Add(convertedRig);
            }

            return rigs;
        }

        private XElement ConvertTubulars(XElement root)
        {
            var tubulars = new XElement(Witsml21Ns + "tubulars", new XAttribute("version", "2.1"));

            foreach (var tubular in root.Elements(Witsml1411 + "tubular"))
            {
                var convertedTubular = new XElement(Witsml21Ns + "tubular");

                foreach (var attr in tubular.Attributes())
                {
                    if (attr.Name.LocalName == "uid")
                        convertedTubular.Add(new XAttribute("uid", attr.Value));
                    else if (attr.Name.LocalName == "uidWell")
                    {
                        var wellRef = new XElement(Witsml21Ns + "well");
                        wellRef.Add(new XAttribute("uid", attr.Value));
                        convertedTubular.Add(wellRef);
                    }
                    else if (attr.Name.LocalName == "uidWellbore")
                    {
                        var wellboreRef = new XElement(Witsml21Ns + "wellbore");
                        wellboreRef.Add(new XAttribute("uid", attr.Value));
                        convertedTubular.Add(wellboreRef);
                    }
                }

                ConvertElement(tubular, convertedTubular, "name", "name");
                ConvertElement(tubular, convertedTubular, "typeTubularAssy", "typeTubularAssy");

                foreach (var comp in tubular.Elements(Witsml1411 + "tubularComponent"))
                    convertedTubular.Add(ConvertElementGeneric(comp));

                var commonData = tubular.Element(Witsml1411 + "commonData");
                if (commonData != null)
                {
                    var convertedCommonData = new XElement(Witsml21Ns + "commonData");
                    ConvertElement(commonData, convertedCommonData, "dTimCreation", "dTimCreation");
                    ConvertElement(commonData, convertedCommonData, "dTimLastChange", "dTimLastChange");
                    if (convertedCommonData.HasElements || convertedCommonData.HasAttributes)
                        convertedTubular.Add(convertedCommonData);
                }

                tubulars.Add(convertedTubular);
            }

            return tubulars;
        }

        private XElement ConvertWbGeometrys(XElement root)
        {
            var wbGeometrys = new XElement(Witsml21Ns + "wbGeometrys", new XAttribute("version", "2.1"));

            foreach (var wbGeometry in root.Elements(Witsml1411 + "wbGeometry"))
            {
                var converted = new XElement(Witsml21Ns + "wbGeometry");

                foreach (var attr in wbGeometry.Attributes())
                {
                    if (attr.Name.LocalName == "uid")
                        converted.Add(new XAttribute("uid", attr.Value));
                    else if (attr.Name.LocalName == "uidWell")
                    {
                        var wellRef = new XElement(Witsml21Ns + "well");
                        wellRef.Add(new XAttribute("uid", attr.Value));
                        converted.Add(wellRef);
                    }
                    else if (attr.Name.LocalName == "uidWellbore")
                    {
                        var wellboreRef = new XElement(Witsml21Ns + "wellbore");
                        wellboreRef.Add(new XAttribute("uid", attr.Value));
                        converted.Add(wellboreRef);
                    }
                }

                ConvertElement(wbGeometry, converted, "name", "name");
                ConvertElement(wbGeometry, converted, "dTimReport", "dTimReport");
                ConvertElement(wbGeometry, converted, "mdBottom", "mdBottom");

                foreach (var section in wbGeometry.Elements(Witsml1411 + "wbGeometrySection"))
                    converted.Add(ConvertElementGeneric(section));

                var commonData = wbGeometry.Element(Witsml1411 + "commonData");
                if (commonData != null)
                {
                    var convertedCommonData = new XElement(Witsml21Ns + "commonData");
                    ConvertElement(commonData, convertedCommonData, "dTimCreation", "dTimCreation");
                    ConvertElement(commonData, convertedCommonData, "dTimLastChange", "dTimLastChange");
                    if (convertedCommonData.HasElements || convertedCommonData.HasAttributes)
                        converted.Add(convertedCommonData);
                }

                wbGeometrys.Add(converted);
            }

            return wbGeometrys;
        }

        private XElement ConvertBhaRuns(XElement root)
        {
            var bhaRuns = new XElement(Witsml21Ns + "bhaRuns", new XAttribute("version", "2.1"));

            foreach (var bhaRun in root.Elements(Witsml1411 + "bhaRun"))
            {
                var converted = new XElement(Witsml21Ns + "bhaRun");

                foreach (var attr in bhaRun.Attributes())
                {
                    if (attr.Name.LocalName == "uid")
                        converted.Add(new XAttribute("uid", attr.Value));
                    else if (attr.Name.LocalName == "uidWell")
                    {
                        var wellRef = new XElement(Witsml21Ns + "well");
                        wellRef.Add(new XAttribute("uid", attr.Value));
                        converted.Add(wellRef);
                    }
                    else if (attr.Name.LocalName == "uidWellbore")
                    {
                        var wellboreRef = new XElement(Witsml21Ns + "wellbore");
                        wellboreRef.Add(new XAttribute("uid", attr.Value));
                        converted.Add(wellboreRef);
                    }
                }

                ConvertElement(bhaRun, converted, "name", "name");
                // tubular en v1.4 puede ser <tubular uidRef="x"/> o elemento tubularRef
                var tubularEl = bhaRun.Element(Witsml1411 + "tubular");
                if (tubularEl != null)
                {
                    var tubularRef = tubularEl.Attribute("uidRef")?.Value ?? tubularEl.Attribute("uid")?.Value ?? tubularEl.Value;
                    if (!string.IsNullOrEmpty(tubularRef))
                    {
                        var tubularConverted = new XElement(Witsml21Ns + "tubular");
                        tubularConverted.Add(new XAttribute("uid", tubularRef));
                        converted.Add(tubularConverted);
                    }
                }
                ConvertElement(bhaRun, converted, "dTimStart", "dTimStart");
                ConvertElement(bhaRun, converted, "dTimStop", "dTimStop");
                ConvertElement(bhaRun, converted, "numStringRun", "numStringRun");

                var commonData = bhaRun.Element(Witsml1411 + "commonData");
                if (commonData != null)
                {
                    var convertedCommonData = new XElement(Witsml21Ns + "commonData");
                    ConvertElement(commonData, convertedCommonData, "dTimCreation", "dTimCreation");
                    ConvertElement(commonData, convertedCommonData, "dTimLastChange", "dTimLastChange");
                    if (convertedCommonData.HasElements || convertedCommonData.HasAttributes)
                        converted.Add(convertedCommonData);
                }

                bhaRuns.Add(converted);
            }

            return bhaRuns;
        }

        private XElement ConvertAttachments(XElement root)
        {
            var attachments = new XElement(Witsml21Ns + "attachments", new XAttribute("version", "2.1"));

            foreach (var att in root.Elements(Witsml1411 + "attachment"))
            {
                var converted = new XElement(Witsml21Ns + "attachment");

                foreach (var attr in att.Attributes())
                {
                    if (attr.Name.LocalName == "uid")
                        converted.Add(new XAttribute("uid", attr.Value));
                    else if (attr.Name.LocalName == "uidWell")
                    {
                        var wellRef = new XElement(Witsml21Ns + "well");
                        wellRef.Add(new XAttribute("uid", attr.Value));
                        converted.Add(wellRef);
                    }
                    else if (attr.Name.LocalName == "uidWellbore")
                    {
                        var wellboreRef = new XElement(Witsml21Ns + "wellbore");
                        wellboreRef.Add(new XAttribute("uid", attr.Value));
                        converted.Add(wellboreRef);
                    }
                }

                ConvertElement(att, converted, "name", "name");
                ConvertElement(att, converted, "fileName", "fileName");
                ConvertElement(att, converted, "description", "description");
                ConvertElement(att, converted, "object", "object");

                var commonData = att.Element(Witsml1411 + "commonData");
                if (commonData != null)
                {
                    var convertedCommonData = new XElement(Witsml21Ns + "commonData");
                    ConvertElement(commonData, convertedCommonData, "dTimCreation", "dTimCreation");
                    ConvertElement(commonData, convertedCommonData, "dTimLastChange", "dTimLastChange");
                    if (convertedCommonData.HasElements || convertedCommonData.HasAttributes)
                        converted.Add(convertedCommonData);
                }

                attachments.Add(converted);
            }

            return attachments;
        }

        private XElement ConvertFormationMarkers(XElement root)
        {
            var formationMarkers = new XElement(Witsml21Ns + "formationMarkers", new XAttribute("version", "2.1"));

            foreach (var fm in root.Elements(Witsml1411 + "formationMarker"))
            {
                var converted = new XElement(Witsml21Ns + "formationMarker");

                foreach (var attr in fm.Attributes())
                {
                    if (attr.Name.LocalName == "uid")
                        converted.Add(new XAttribute("uid", attr.Value));
                    else if (attr.Name.LocalName == "uidWell")
                    {
                        var wellRef = new XElement(Witsml21Ns + "well");
                        wellRef.Add(new XAttribute("uid", attr.Value));
                        converted.Add(wellRef);
                    }
                    else if (attr.Name.LocalName == "uidWellbore")
                    {
                        var wellboreRef = new XElement(Witsml21Ns + "wellbore");
                        wellboreRef.Add(new XAttribute("uid", attr.Value));
                        converted.Add(wellboreRef);
                    }
                }

                ConvertElement(fm, converted, "name", "name");
                ConvertElement(fm, converted, "md", "md");
                ConvertElement(fm, converted, "tvd", "tvd");
                ConvertElement(fm, converted, "formationLithology", "formationLithology");

                var commonData = fm.Element(Witsml1411 + "commonData");
                if (commonData != null)
                {
                    var convertedCommonData = new XElement(Witsml21Ns + "commonData");
                    ConvertElement(commonData, convertedCommonData, "dTimCreation", "dTimCreation");
                    ConvertElement(commonData, convertedCommonData, "dTimLastChange", "dTimLastChange");
                    if (convertedCommonData.HasElements || convertedCommonData.HasAttributes)
                        converted.Add(convertedCommonData);
                }

                formationMarkers.Add(converted);
            }

            return formationMarkers;
        }

        private XElement ConvertMessages(XElement root)
        {
            var messages = new XElement(Witsml21Ns + "messages", new XAttribute("version", "2.1"));

            foreach (var message in root.Elements(Witsml1411 + "message"))
            {
                var converted = new XElement(Witsml21Ns + "message");

                foreach (var attr in message.Attributes())
                {
                    if (attr.Name.LocalName == "uid")
                        converted.Add(new XAttribute("uid", attr.Value));
                    else if (attr.Name.LocalName == "uidWell")
                    {
                        var wellRef = new XElement(Witsml21Ns + "well");
                        wellRef.Add(new XAttribute("uid", attr.Value));
                        converted.Add(wellRef);
                    }
                    else if (attr.Name.LocalName == "uidWellbore")
                    {
                        var wellboreRef = new XElement(Witsml21Ns + "wellbore");
                        wellboreRef.Add(new XAttribute("uid", attr.Value));
                        converted.Add(wellboreRef);
                    }
                }

                ConvertElement(message, converted, "name", "name");
                ConvertElement(message, converted, "dTim", "dTim");
                ConvertElement(message, converted, "md", "md");
                ConvertElement(message, converted, "typeMessage", "typeMessage");
                ConvertElement(message, converted, "messageText", "messageText");

                var commonData = message.Element(Witsml1411 + "commonData");
                if (commonData != null)
                {
                    var convertedCommonData = new XElement(Witsml21Ns + "commonData");
                    ConvertElement(commonData, convertedCommonData, "dTimCreation", "dTimCreation");
                    ConvertElement(commonData, convertedCommonData, "dTimLastChange", "dTimLastChange");
                    if (convertedCommonData.HasElements || convertedCommonData.HasAttributes)
                        converted.Add(convertedCommonData);
                }

                messages.Add(converted);
            }

            return messages;
        }

        private XElement ConvertGeneric(XElement root)
        {
            // Conversión genérica para tipos no específicos
            var localName = root.Name.LocalName;
            var convertedRoot = new XElement(Witsml21Ns + localName,
                new XAttribute("version", "2.1")
            );

            // Copiar todos los elementos recursivamente
            foreach (var element in root.Elements())
            {
                var convertedElement = ConvertElementGeneric(element);
                convertedRoot.Add(convertedElement);
            }

            return convertedRoot;
        }

        private XElement ConvertElementGeneric(XElement source)
        {
            var converted = new XElement(Witsml21Ns + source.Name.LocalName);
            
            // Copiar atributos
            foreach (var attr in source.Attributes())
            {
                converted.Add(new XAttribute(attr.Name, attr.Value));
            }

            // Copiar valor si es un elemento simple
            if (!source.HasElements && !string.IsNullOrWhiteSpace(source.Value))
            {
                converted.Value = source.Value;
            }

            // Convertir elementos hijos
            foreach (var child in source.Elements())
            {
                var convertedChild = ConvertElementGeneric(child);
                converted.Add(convertedChild);
            }

            return converted;
        }

        private void ConvertElement(XElement source, XElement target, string sourceName, string targetName)
        {
            var element = source.Element(Witsml1411 + sourceName);
            if (element != null)
            {
                var converted = new XElement(Witsml21Ns + targetName);
                CopyAttributesAndValue(element, converted);
                target.Add(converted);
            }
        }

        private void CopyAttributesAndValue(XElement source, XElement target)
        {
            // Copiar atributos (como uom)
            foreach (var attr in source.Attributes())
            {
                target.Add(new XAttribute(attr.Name, attr.Value));
            }

            // Copiar valor
            if (!source.HasElements)
            {
                target.Value = source.Value;
            }
            else
            {
                // Si tiene elementos hijos, copiarlos también
                foreach (var child in source.Elements())
                {
                    var convertedChild = new XElement(Witsml21Ns + child.Name.LocalName);
                    CopyAttributesAndValue(child, convertedChild);
                    target.Add(convertedChild);
                }
            }
        }

        /// <summary>
        /// Convierte todos los archivos XML en un directorio
        /// </summary>
        public async Task<int> ConvertDirectoryAsync(string inputDirectory, string? outputDirectory = null)
        {
            if (!Directory.Exists(inputDirectory))
                throw new DirectoryNotFoundException($"El directorio no existe: {inputDirectory}");

            if (string.IsNullOrEmpty(outputDirectory))
            {
                outputDirectory = Path.Combine(inputDirectory, "converted_v2.1");
            }

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var xmlFiles = Directory.GetFiles(inputDirectory, "*.xml", SearchOption.AllDirectories);
            int convertedCount = 0;

            Console.WriteLine($"Encontrados {xmlFiles.Length} archivos XML para convertir...");
            Console.WriteLine($"Directorio de salida: {outputDirectory}");
            Console.WriteLine();

            foreach (var file in xmlFiles)
            {
                try
                {
                    var relativePath = Path.GetRelativePath(inputDirectory, file);
                    var outputPath = Path.Combine(outputDirectory, relativePath);
                    var outputDir = Path.GetDirectoryName(outputPath);
                    
                    if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }

                    await ConvertFileAsync(file, outputPath);
                    convertedCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Error al convertir {Path.GetFileName(file)}: {ex.Message}");
                }
            }

            Console.WriteLine();
            Console.WriteLine($"✓ Conversión completada: {convertedCount}/{xmlFiles.Length} archivos convertidos");
            return convertedCount;
        }
    }
}
