using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BochazoEtpWitsml.Services
{
    public class WitsmlProcessor
    {
        public async Task ProcessWitsmlFileAsync(string filePath, WitsmlRepository? repository = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"El archivo no existe: {filePath}");

            var xmlContent = await File.ReadAllTextAsync(filePath);
            var doc = XDocument.Parse(xmlContent);

            if (repository != null)
            {
                ProcessWitsmlToDatabase(doc, repository, filePath);
            }
            else
            {
                ProcessWitsmlToConsole(doc);
            }
        }

        private void ProcessWitsmlToDatabase(XDocument doc, WitsmlRepository repo, string sourceFile)
        {
            var root = doc.Root;
            if (root == null) return;

            var ns = root.Name.Namespace;
            var rootName = root.Name.LocalName.ToLower();

            switch (rootName)
            {
                    case "wells":
                        SaveWells(root, ns, repo, sourceFile);
                        break;
                    case "wellbores":
                        SaveWellbores(root, ns, repo, sourceFile);
                        break;
                    case "trajectories":
                    case "trajectorys":
                        SaveTrajectories(root, ns, repo, sourceFile);
                        break;
                    case "mudlogs":
                        SaveMudLogs(root, ns, repo, sourceFile);
                        break;
                    case "logs":
                        SaveLogs(root, ns, repo, sourceFile);
                        break;
                    case "rigs":
                        SaveRigs(root, ns, repo, sourceFile);
                        break;
                    case "tubulars":
                        SaveTubulars(root, ns, repo, sourceFile);
                        break;
                    case "wbgeometrys":
                        SaveWbGeometrys(root, ns, repo, sourceFile);
                        break;
                    case "bharuns":
                        SaveBhaRuns(root, ns, repo, sourceFile);
                        break;
                    case "messages":
                        SaveMessages(root, ns, repo, sourceFile);
                        break;
                    default:
                        Console.WriteLine($"Tipo no soportado para guardar en BD: {rootName}");
                        break;
            }
        }

        private void SaveWells(XElement root, XNamespace ns, WitsmlRepository repo, string sourceFile)
        {
            var wells = root.Elements(ns + "well").ToList();
            foreach (var well in wells)
            {
                var uid = well.Attribute("uid")?.Value ?? GetVal(well, ns, "uid");
                if (string.IsNullOrEmpty(uid)) continue;

                var commonData = well.Element(ns + "commonData");
                var created = commonData != null ? GetVal(commonData, ns, "dTimCreation") : null;
                var updated = commonData != null ? GetVal(commonData, ns, "dTimLastChange") : null;

                repo.SaveWell(
                    uid,
                    GetVal(well, ns, "name"),
                    GetVal(well, ns, "timeZone"),
                    GetVal(well, ns, "statusWell"),
                    created, updated, sourceFile);
            }
            Console.WriteLine($"✓ {wells.Count} well(s) guardado(s) en la base de datos");
        }

        private void SaveWellbores(XElement root, XNamespace ns, WitsmlRepository repo, string sourceFile)
        {
            var wellbores = root.Elements(ns + "wellbore").ToList();
            foreach (var wb in wellbores)
            {
                var uid = wb.Attribute("uid")?.Value ?? "";
                var wellUid = wb.Element(ns + "well")?.Attribute("uid")?.Value ?? wb.Attribute("uidWell")?.Value ?? "";

                var commonData = wb.Element(ns + "commonData");
                var created = commonData != null ? GetVal(commonData, ns, "dTimCreation") : null;
                var updated = commonData != null ? GetVal(commonData, ns, "dTimLastChange") : null;

                repo.SaveWellbore(uid, wellUid, GetVal(wb, ns, "name"), GetVal(wb, ns, "isActive"),
                    created, updated, sourceFile);
            }
            Console.WriteLine($"✓ {wellbores.Count} wellbore(s) guardado(s) en la base de datos");
        }

        private void SaveTrajectories(XElement root, XNamespace ns, WitsmlRepository repo, string sourceFile)
        {
            var trajectories = root.Elements(ns + "trajectory").ToList();
            foreach (var traj in trajectories)
            {
                var uid = traj.Attribute("uid")?.Value ?? "";
                var wellRef = traj.Element(ns + "well");
                var wellboreRef = traj.Element(ns + "wellbore");
                var wellUid = wellRef?.Attribute("uid")?.Value ?? "";
                var wellboreUid = wellboreRef?.Attribute("uid")?.Value ?? "";

                var commonData = traj.Element(ns + "commonData");
                var created = commonData != null ? GetVal(commonData, ns, "dTimCreation") : null;
                var updated = commonData != null ? GetVal(commonData, ns, "dTimLastChange") : null;

                repo.SaveTrajectory(
                    uid, wellUid, wellboreUid,
                    GetVal(traj, ns, "name"),
                    GetVal(traj, ns, "serviceCompany"),
                    GetVal(traj, ns, "objectGrowing"),
                    GetVal(traj, ns, "dTimTrajStart"),
                    GetVal(traj, ns, "dTimTrajEnd"),
                    GetVal(traj, ns, "mdMn"),
                    GetVal(traj, ns, "mdMx"),
                    GetVal(traj, ns, "gridCorUsed"),
                    GetVal(traj, ns, "aziVertSect"),
                    GetVal(traj, ns, "aziRef"),
                    created, updated, sourceFile);

                var stations = traj.Elements(ns + "trajectoryStation").ToList();
                foreach (var stn in stations)
                {
                    var commonDataStn = stn.Element(ns + "commonData");
                    repo.SaveTrajectoryStation(
                        uid,
                        stn.Attribute("uid")?.Value ?? "",
                        GetVal(stn, ns, "dTimStn"),
                        GetVal(stn, ns, "typeTrajStation"),
                        GetVal(stn, ns, "md"),
                        GetVal(stn, ns, "tvd"),
                        GetVal(stn, ns, "incl"),
                        GetVal(stn, ns, "azi"),
                        GetVal(stn, ns, "dispNs"),
                        GetVal(stn, ns, "dispEw"),
                        GetVal(stn, ns, "vertSect"),
                        GetVal(stn, ns, "dls"),
                        GetVal(stn, ns, "rateTurn"),
                        GetVal(stn, ns, "rateBuild"),
                        GetVal(stn, ns, "mdDelta"),
                        GetVal(stn, ns, "tvdDelta"),
                        GetVal(stn, ns, "statusTrajStation"),
                        sourceFile);
                }
            }
            var totalStations = trajectories.Sum(t => t.Elements(ns + "trajectoryStation").Count());
            Console.WriteLine($"✓ {trajectories.Count} trajectory(s) y {totalStations} estación(es) guardado(s) en la base de datos");
        }

        private void SaveMudLogs(XElement root, XNamespace ns, WitsmlRepository repo, string sourceFile)
        {
            var mudLogs = root.Elements(ns + "mudLog").ToList();
            foreach (var ml in mudLogs)
            {
                var uid = ml.Attribute("uid")?.Value ?? "";
                var wellUid = ml.Element(ns + "well")?.Attribute("uid")?.Value ?? ml.Attribute("uidWell")?.Value ?? "";
                var wellboreUid = ml.Element(ns + "wellbore")?.Attribute("uid")?.Value ?? ml.Attribute("uidWellbore")?.Value ?? "";

                var commonData = ml.Element(ns + "commonData");
                var created = commonData != null ? GetVal(commonData, ns, "dTimCreation") : null;
                var updated = commonData != null ? GetVal(commonData, ns, "dTimLastChange") : null;

                repo.SaveMudLog(
                    uid, wellUid, wellboreUid,
                    GetVal(ml, ns, "name"),
                    GetVal(ml, ns, "mudLogCompany"),
                    GetVal(ml, ns, "mudLogEngineers"),
                    GetVal(ml, ns, "objectGrowing"),
                    GetVal(ml, ns, "dTim"),
                    GetVal(ml, ns, "startMd"),
                    GetVal(ml, ns, "endMd"),
                    created, updated, sourceFile);

                foreach (var interval in ml.Elements(ns + "geologyInterval"))
                {
                    var intervalUid = interval.Attribute("uid")?.Value ?? "";
                    var commonTime = interval.Element(ns + "commonTime");
                    var icreated = commonTime != null ? GetVal(commonTime, ns, "dTimCreation") : null;
                    var iupdated = commonTime != null ? GetVal(commonTime, ns, "dTimLastChange") : null;

                    repo.SaveGeologyInterval(
                        uid, intervalUid, GetVal(interval, ns, "typeLithology"),
                        GetVal(interval, ns, "mdTop"),
                        GetVal(interval, ns, "mdBottom"),
                        icreated, iupdated, sourceFile);

                    foreach (var lith in interval.Elements(ns + "lithology"))
                    {
                        repo.SaveLithology(
                            intervalUid,
                            lith.Attribute("uid")?.Value ?? "",
                            GetVal(lith, ns, "type"),
                            GetVal(lith, ns, "codeLith"),
                            GetVal(lith, ns, "lithPc"),
                            GetVal(lith, ns, "description"),
                            sourceFile);
                    }
                }
            }
            Console.WriteLine($"✓ {mudLogs.Count} mudLog(s) guardado(s) en la base de datos");
        }

        private void SaveLogs(XElement root, XNamespace ns, WitsmlRepository repo, string sourceFile)
        {
            var logs = root.Elements(ns + "log").ToList();
            foreach (var log in logs)
            {
                var uid = log.Attribute("uid")?.Value ?? "";
                var wellUid = log.Element(ns + "well")?.Attribute("uid")?.Value ?? log.Attribute("uidWell")?.Value ?? "";
                var wellboreUid = log.Element(ns + "wellbore")?.Attribute("uid")?.Value ?? log.Attribute("uidWellbore")?.Value ?? "";

                var commonData = log.Element(ns + "commonData");
                var created = commonData != null ? GetVal(commonData, ns, "dTimCreation") : null;
                var updated = commonData != null ? GetVal(commonData, ns, "dTimLastChange") : null;

                repo.SaveLog(uid, wellUid, wellboreUid,
                    GetVal(log, ns, "name"),
                    GetVal(log, ns, "indexType"),
                    GetVal(log, ns, "objectGrowing"),
                    created, updated, sourceFile);
            }
            Console.WriteLine($"✓ {logs.Count} log(s) guardado(s) en la base de datos");
        }

        private void SaveRigs(XElement root, XNamespace ns, WitsmlRepository repo, string sourceFile)
        {
            var rigs = root.Elements(ns + "rig").ToList();
            foreach (var rig in rigs)
            {
                var uid = rig.Attribute("uid")?.Value ?? "";
                var wellUid = rig.Element(ns + "well")?.Attribute("uid")?.Value ?? rig.Attribute("uidWell")?.Value ?? "";
                var wellboreUid = rig.Element(ns + "wellbore")?.Attribute("uid")?.Value ?? rig.Attribute("uidWellbore")?.Value ?? "";

                var commonData = rig.Element(ns + "commonData");
                var created = commonData != null ? GetVal(commonData, ns, "dTimCreation") : null;
                var updated = commonData != null ? GetVal(commonData, ns, "dTimLastChange") : null;

                repo.SaveRig(uid, wellUid, wellboreUid,
                    GetVal(rig, ns, "name"),
                    GetVal(rig, ns, "owner"),
                    GetVal(rig, ns, "typeRig"),
                    created, updated, sourceFile);
            }
            Console.WriteLine($"✓ {rigs.Count} rig(s) guardado(s) en la base de datos");
        }

        private void SaveTubulars(XElement root, XNamespace ns, WitsmlRepository repo, string sourceFile)
        {
            var tubulars = root.Elements(ns + "tubular").ToList();
            foreach (var tub in tubulars)
            {
                var uid = tub.Attribute("uid")?.Value ?? "";
                var wellUid = tub.Element(ns + "well")?.Attribute("uid")?.Value ?? tub.Attribute("uidWell")?.Value ?? "";
                var wellboreUid = tub.Element(ns + "wellbore")?.Attribute("uid")?.Value ?? tub.Attribute("uidWellbore")?.Value ?? "";

                var commonData = tub.Element(ns + "commonData");
                var created = commonData != null ? GetVal(commonData, ns, "dTimCreation") : null;
                var updated = commonData != null ? GetVal(commonData, ns, "dTimLastChange") : null;

                repo.SaveTubular(uid, wellUid, wellboreUid,
                    GetVal(tub, ns, "name"),
                    GetVal(tub, ns, "typeTubularAssy"),
                    created, updated, sourceFile);

                foreach (var comp in tub.Elements(ns + "tubularComponent"))
                {
                    repo.SaveTubularComponent(
                        uid,
                        comp.Attribute("uid")?.Value ?? "",
                        GetVal(comp, ns, "typeTubularComp"),
                        GetVal(comp, ns, "sequence"),
                        GetVal(comp, ns, "id"),
                        GetVal(comp, ns, "od"),
                        GetVal(comp, ns, "len"),
                        GetVal(comp, ns, "lenJointAv"),
                        GetVal(comp, ns, "numJointStand"),
                        GetVal(comp, ns, "wtPerLen"),
                        GetVal(comp, ns, "vendor"),
                        sourceFile);
                }
            }
            var totalComps = tubulars.Sum(t => t.Elements(ns + "tubularComponent").Count());
            Console.WriteLine($"✓ {tubulars.Count} tubular(s) y {totalComps} componente(s) guardado(s) en la base de datos");
        }

        private void SaveWbGeometrys(XElement root, XNamespace ns, WitsmlRepository repo, string sourceFile)
        {
            var wbGeometrys = root.Elements(ns + "wbGeometry").ToList();
            foreach (var wb in wbGeometrys)
            {
                var uid = wb.Attribute("uid")?.Value ?? "";
                var wellUid = wb.Element(ns + "well")?.Attribute("uid")?.Value ?? wb.Attribute("uidWell")?.Value ?? "";
                var wellboreUid = wb.Element(ns + "wellbore")?.Attribute("uid")?.Value ?? wb.Attribute("uidWellbore")?.Value ?? "";

                var commonData = wb.Element(ns + "commonData");
                var created = commonData != null ? GetVal(commonData, ns, "dTimCreation") : null;
                var updated = commonData != null ? GetVal(commonData, ns, "dTimLastChange") : null;

                repo.SaveWbGeometry(uid, wellUid, wellboreUid,
                    GetVal(wb, ns, "name"),
                    GetVal(wb, ns, "dTimReport"),
                    GetVal(wb, ns, "mdBottom"),
                    created, updated, sourceFile);

                foreach (var section in wb.Elements(ns + "wbGeometrySection"))
                {
                    repo.SaveWbGeometrySection(
                        uid,
                        section.Attribute("uid")?.Value ?? "",
                        GetVal(section, ns, "typeHoleCasing"),
                        GetVal(section, ns, "mdTop"),
                        GetVal(section, ns, "mdBottom"),
                        GetVal(section, ns, "idSection"),
                        GetVal(section, ns, "odSection"),
                        sourceFile);
                }
            }
            var totalSections = wbGeometrys.Sum(w => w.Elements(ns + "wbGeometrySection").Count());
            Console.WriteLine($"✓ {wbGeometrys.Count} wbGeometry(s) y {totalSections} sección(es) guardado(s) en la base de datos");
        }

        private void SaveBhaRuns(XElement root, XNamespace ns, WitsmlRepository repo, string sourceFile)
        {
            var bhaRuns = root.Elements(ns + "bhaRun").ToList();
            foreach (var bha in bhaRuns)
            {
                var uid = bha.Attribute("uid")?.Value ?? "";
                var wellUid = bha.Element(ns + "well")?.Attribute("uid")?.Value ?? bha.Attribute("uidWell")?.Value ?? "";
                var wellboreUid = bha.Element(ns + "wellbore")?.Attribute("uid")?.Value ?? bha.Attribute("uidWellbore")?.Value ?? "";

                var tubularEl = bha.Element(ns + "tubular");
                var tubularRef = tubularEl?.Attribute("uid")?.Value ?? tubularEl?.Value ?? "";

                var commonData = bha.Element(ns + "commonData");
                var created = commonData != null ? GetVal(commonData, ns, "dTimCreation") : null;
                var updated = commonData != null ? GetVal(commonData, ns, "dTimLastChange") : null;

                repo.SaveBhaRun(uid, wellUid, wellboreUid,
                    GetVal(bha, ns, "name"),
                    tubularRef,
                    GetVal(bha, ns, "dTimStart"),
                    GetVal(bha, ns, "dTimStop"),
                    GetVal(bha, ns, "numStringRun"),
                    created, updated, sourceFile);
            }
            Console.WriteLine($"✓ {bhaRuns.Count} bhaRun(s) guardado(s) en la base de datos");
        }

        private void SaveMessages(XElement root, XNamespace ns, WitsmlRepository repo, string sourceFile)
        {
            var messages = root.Elements(ns + "message").ToList();
            foreach (var msg in messages)
            {
                var uid = msg.Attribute("uid")?.Value ?? "";
                var wellUid = msg.Element(ns + "well")?.Attribute("uid")?.Value ?? msg.Attribute("uidWell")?.Value ?? "";
                var wellboreUid = msg.Element(ns + "wellbore")?.Attribute("uid")?.Value ?? msg.Attribute("uidWellbore")?.Value ?? "";

                var commonData = msg.Element(ns + "commonData");
                var created = commonData != null ? GetVal(commonData, ns, "dTimCreation") : null;
                var updated = commonData != null ? GetVal(commonData, ns, "dTimLastChange") : null;

                repo.SaveMessage(uid, wellUid, wellboreUid,
                    GetVal(msg, ns, "name"),
                    GetVal(msg, ns, "dTim"),
                    GetVal(msg, ns, "md"),
                    GetVal(msg, ns, "typeMessage"),
                    GetVal(msg, ns, "messageText"),
                    created, updated, sourceFile);
            }
            Console.WriteLine($"✓ {messages.Count} message(s) guardado(s) en la base de datos");
        }

        private void ProcessWitsmlToConsole(XDocument doc)
        {
            var root = doc.Root;
            if (root == null) return;

            var ns = root.Name.Namespace;
            var version = root.Attribute("version")?.Value ?? "unknown";

            Console.WriteLine();
            Console.WriteLine($"╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║  WITSML Database View - Version: {version,-44}║");
            Console.WriteLine($"╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            var rootName = root.Name.LocalName.ToLower();
            switch (rootName)
            {
                case "wells": ProcessWellsTable(root, ns); break;
                case "wellbores": ProcessWellboresTable(root, ns); break;
                case "trajectories":
                case "trajectorys": ProcessTrajectoriesTable(root, ns); break;
                case "mudlogs": ProcessMudLogsTable(root, ns); break;
                case "logs": ProcessLogsTable(root, ns); break;
                case "rigs": ProcessRigsTable(root, ns); break;
                case "tubulars": ProcessTubularsTable(root, ns); break;
                case "wbgeometrys": ProcessWbGeometrysTable(root, ns); break;
                case "bharuns": ProcessBhaRunsTable(root, ns); break;
                case "messages": ProcessMessagesTable(root, ns); break;
                default: ProcessGenericTable(root, ns); break;
            }
        }

        private string? GetVal(XElement parent, XNamespace ns, string name)
        {
            var el = parent.Element(ns + name);
            var v = el?.Value;
            return string.IsNullOrWhiteSpace(v) || v == "NULL" ? null : v;
        }

        #region Console output (legacy)

        private void ProcessWellsTable(XElement root, XNamespace ns)
        {
            var wells = root.Elements(ns + "well").ToList();
            Console.WriteLine("┌── TABLE: wells ──┐");
            foreach (var w in wells)
            {
                var uid = w.Attribute("uid")?.Value ?? GetVal(w, ns, "uid") ?? "";
                var name = GetVal(w, ns, "name") ?? "NULL";
                Console.WriteLine($"│ {uid} | {name}");
            }
            Console.WriteLine($"({wells.Count} rows)");
        }

        private void ProcessWellboresTable(XElement root, XNamespace ns)
        {
            var wbs = root.Elements(ns + "wellbore").ToList();
            Console.WriteLine("┌── TABLE: wellbores ──┐");
            foreach (var w in wbs)
            {
                var uid = w.Attribute("uid")?.Value ?? "";
                var name = GetVal(w, ns, "name") ?? "NULL";
                Console.WriteLine($"│ {uid} | {name}");
            }
            Console.WriteLine($"({wbs.Count} rows)");
        }

        private void ProcessTrajectoriesTable(XElement root, XNamespace ns)
        {
            var trajs = root.Elements(ns + "trajectory").ToList();
            Console.WriteLine("┌── TABLE: trajectories ──┐");
            foreach (var t in trajs)
            {
                var uid = t.Attribute("uid")?.Value ?? "";
                var name = GetVal(t, ns, "name") ?? "NULL";
                var stations = t.Elements(ns + "trajectoryStation").Count();
                Console.WriteLine($"│ {uid} | {name} | {stations} stations");
            }
            Console.WriteLine($"({trajs.Count} rows)");
        }

        private void ProcessMudLogsTable(XElement root, XNamespace ns)
        {
            var mls = root.Elements(ns + "mudLog").ToList();
            Console.WriteLine("┌── TABLE: mud_logs ──┐");
            foreach (var m in mls)
            {
                var uid = m.Attribute("uid")?.Value ?? "";
                var name = GetVal(m, ns, "name") ?? "NULL";
                Console.WriteLine($"│ {uid} | {name}");
            }
            Console.WriteLine($"({mls.Count} rows)");
        }

        private void ProcessLogsTable(XElement root, XNamespace ns)
        {
            var logs = root.Elements(ns + "log").ToList();
            Console.WriteLine("┌── TABLE: logs ──┐");
            foreach (var l in logs)
            {
                var uid = l.Attribute("uid")?.Value ?? "";
                var name = GetVal(l, ns, "name") ?? "NULL";
                Console.WriteLine($"│ {uid} | {name}");
            }
            Console.WriteLine($"({logs.Count} rows)");
        }

        private void ProcessRigsTable(XElement root, XNamespace ns)
        {
            var rigs = root.Elements(ns + "rig").ToList();
            Console.WriteLine("┌── TABLE: rigs ──┐");
            foreach (var r in rigs)
            {
                var uid = r.Attribute("uid")?.Value ?? "";
                var name = GetVal(r, ns, "name") ?? "NULL";
                var typeRig = GetVal(r, ns, "typeRig") ?? "NULL";
                Console.WriteLine($"│ {uid} | {name} | {typeRig}");
            }
            Console.WriteLine($"({rigs.Count} rows)");
        }

        private void ProcessTubularsTable(XElement root, XNamespace ns)
        {
            var tubulars = root.Elements(ns + "tubular").ToList();
            Console.WriteLine("┌── TABLE: tubulars ──┐");
            foreach (var t in tubulars)
            {
                var uid = t.Attribute("uid")?.Value ?? "";
                var name = GetVal(t, ns, "name") ?? "NULL";
                var comps = t.Elements(ns + "tubularComponent").Count();
                Console.WriteLine($"│ {uid} | {name} | {comps} components");
            }
            Console.WriteLine($"({tubulars.Count} rows)");
        }

        private void ProcessWbGeometrysTable(XElement root, XNamespace ns)
        {
            var wbGeoms = root.Elements(ns + "wbGeometry").ToList();
            Console.WriteLine("┌── TABLE: wb_geometrys ──┐");
            foreach (var w in wbGeoms)
            {
                var uid = w.Attribute("uid")?.Value ?? "";
                var name = GetVal(w, ns, "name") ?? "NULL";
                var sections = w.Elements(ns + "wbGeometrySection").Count();
                Console.WriteLine($"│ {uid} | {name} | {sections} sections");
            }
            Console.WriteLine($"({wbGeoms.Count} rows)");
        }

        private void ProcessBhaRunsTable(XElement root, XNamespace ns)
        {
            var bhaRuns = root.Elements(ns + "bhaRun").ToList();
            Console.WriteLine("┌── TABLE: bha_runs ──┐");
            foreach (var b in bhaRuns)
            {
                var uid = b.Attribute("uid")?.Value ?? "";
                var name = GetVal(b, ns, "name") ?? "NULL";
                Console.WriteLine($"│ {uid} | {name}");
            }
            Console.WriteLine($"({bhaRuns.Count} rows)");
        }

        private void ProcessMessagesTable(XElement root, XNamespace ns)
        {
            var messages = root.Elements(ns + "message").ToList();
            Console.WriteLine("┌── TABLE: messages ──┐");
            foreach (var m in messages)
            {
                var uid = m.Attribute("uid")?.Value ?? "";
                var name = GetVal(m, ns, "name") ?? "NULL";
                var dTim = GetVal(m, ns, "dTim") ?? "NULL";
                Console.WriteLine($"│ {uid} | {name} | {dTim}");
            }
            Console.WriteLine($"({messages.Count} rows)");
        }

        private void ProcessGenericTable(XElement root, XNamespace ns)
        {
            Console.WriteLine($"Tipo genérico: {root.Name.LocalName}");
        }

        #endregion
    }
}
