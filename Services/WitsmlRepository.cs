using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace BochazoEtpWitsml.Services
{
    public class WellInfo
    {
        public string Uid { get; set; } = "";
        public string? Name { get; set; }
        public string? TimeZone { get; set; }
        public string? StatusWell { get; set; }
        public DateTime? DTimCreation { get; set; }
        public DateTime? DTimLastChange { get; set; }
        public string? SourceFile { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }

    public class WitsmlRepository : IDisposable
    {
        private readonly string _connectionString;
        private SqlConnection? _connection;

        public WitsmlRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqlConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new SqlConnection(_connectionString);
                _connection.Open();
            }
            return _connection;
        }

        public void SaveWell(string uid, string? name, string? timeZone, string? statusWell,
            string? dTimCreation, string? dTimLastChange, string sourceFile)
        {
            using var cmd = new SqlCommand(
                @"MERGE wells AS target
                  USING (SELECT @uid AS uid) AS source
                  ON target.uid = source.uid
                  WHEN MATCHED THEN
                    UPDATE SET name = @name, time_zone = @timeZone, status_well = @statusWell,
                      d_tim_creation = @dTimCreation, d_tim_last_change = @dTimLastChange,
                      source_file = @sourceFile, processed_at = GETDATE()
                  WHEN NOT MATCHED THEN
                    INSERT (uid, name, time_zone, status_well, d_tim_creation, d_tim_last_change, source_file)
                    VALUES (@uid, @name, @timeZone, @statusWell, @dTimCreation, @dTimLastChange, @sourceFile);",
                GetConnection());

            cmd.Parameters.AddWithValue("@uid", uid);
            cmd.Parameters.AddWithValue("@name", (object?)name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@timeZone", (object?)timeZone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@statusWell", (object?)statusWell ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dTimCreation", ParseDateTime(dTimCreation));
            cmd.Parameters.AddWithValue("@dTimLastChange", ParseDateTime(dTimLastChange));
            cmd.Parameters.AddWithValue("@sourceFile", (object?)sourceFile ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void SaveWellbore(string uid, string? wellUid, string? name, string? isActive,
            string? dTimCreation, string? dTimLastChange, string sourceFile)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO wellbores (uid, well_uid, name, is_active, d_tim_creation, d_tim_last_change, source_file)
                  VALUES (@uid, @wellUid, @name, @isActive, @dTimCreation, @dTimLastChange, @sourceFile);",
                GetConnection());

            cmd.Parameters.AddWithValue("@uid", uid);
            cmd.Parameters.AddWithValue("@wellUid", EmptyToNull(wellUid));
            cmd.Parameters.AddWithValue("@name", (object?)name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@isActive", (object?)isActive ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dTimCreation", ParseDateTime(dTimCreation));
            cmd.Parameters.AddWithValue("@dTimLastChange", ParseDateTime(dTimLastChange));
            cmd.Parameters.AddWithValue("@sourceFile", (object?)sourceFile ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void SaveTrajectory(string uid, string? wellUid, string? wellboreUid, string? name,
            string? serviceCompany, string? objectGrowing, string? dTimTrajStart, string? dTimTrajEnd,
            string? mdMin, string? mdMax, string? gridCorUsed, string? aziVertSect, string? aziRef,
            string? dTimCreation, string? dTimLastChange, string sourceFile)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO trajectories (uid, well_uid, wellbore_uid, name, service_company, object_growing,
                  d_tim_traj_start, d_tim_traj_end, md_min, md_max, grid_cor_used, azi_vert_sect, azi_ref,
                  d_tim_creation, d_tim_last_change, source_file)
                  VALUES (@uid, @wellUid, @wellboreUid, @name, @serviceCompany, @objectGrowing,
                  @dTimTrajStart, @dTimTrajEnd, @mdMin, @mdMax, @gridCorUsed, @aziVertSect, @aziRef,
                  @dTimCreation, @dTimLastChange, @sourceFile);",
                GetConnection());

            cmd.Parameters.AddWithValue("@uid", uid);
            cmd.Parameters.AddWithValue("@wellUid", EmptyToNull(wellUid));
            cmd.Parameters.AddWithValue("@wellboreUid", EmptyToNull(wellboreUid));
            cmd.Parameters.AddWithValue("@name", (object?)name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@serviceCompany", (object?)serviceCompany ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@objectGrowing", (object?)objectGrowing ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dTimTrajStart", ParseDateTime(dTimTrajStart));
            cmd.Parameters.AddWithValue("@dTimTrajEnd", ParseDateTime(dTimTrajEnd));
            cmd.Parameters.AddWithValue("@mdMin", ParseDecimal(mdMin));
            cmd.Parameters.AddWithValue("@mdMax", ParseDecimal(mdMax));
            cmd.Parameters.AddWithValue("@gridCorUsed", ParseDecimal(gridCorUsed));
            cmd.Parameters.AddWithValue("@aziVertSect", ParseDecimal(aziVertSect));
            cmd.Parameters.AddWithValue("@aziRef", (object?)aziRef ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dTimCreation", ParseDateTime(dTimCreation));
            cmd.Parameters.AddWithValue("@dTimLastChange", ParseDateTime(dTimLastChange));
            cmd.Parameters.AddWithValue("@sourceFile", (object?)sourceFile ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void SaveTrajectoryStation(string trajectoryUid, string? uid, string? dTimStn, string? typeTrajStation,
            string? md, string? tvd, string? incl, string? azi, string? dispNs, string? dispEw, string? vertSect,
            string? dls, string? rateTurn, string? rateBuild, string? mdDelta, string? tvdDelta,
            string? statusTrajStation, string sourceFile)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO trajectory_stations (trajectory_uid, uid, d_tim_stn, type_traj_station, md, tvd, incl, azi,
                  disp_ns, disp_ew, vert_sect, dls, rate_turn, rate_build, md_delta, tvd_delta, status_traj_station, source_file)
                  VALUES (@trajectoryUid, @uid, @dTimStn, @typeTrajStation, @md, @tvd, @incl, @azi,
                  @dispNs, @dispEw, @vertSect, @dls, @rateTurn, @rateBuild, @mdDelta, @tvdDelta, @statusTrajStation, @sourceFile);",
                GetConnection());

            cmd.Parameters.AddWithValue("@trajectoryUid", trajectoryUid);
            cmd.Parameters.AddWithValue("@uid", (object?)uid ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dTimStn", ParseDateTime(dTimStn));
            cmd.Parameters.AddWithValue("@typeTrajStation", (object?)typeTrajStation ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@md", ParseDecimal(md));
            cmd.Parameters.AddWithValue("@tvd", ParseDecimal(tvd));
            cmd.Parameters.AddWithValue("@incl", ParseDecimal(incl));
            cmd.Parameters.AddWithValue("@azi", ParseDecimal(azi));
            cmd.Parameters.AddWithValue("@dispNs", ParseDecimal(dispNs));
            cmd.Parameters.AddWithValue("@dispEw", ParseDecimal(dispEw));
            cmd.Parameters.AddWithValue("@vertSect", ParseDecimal(vertSect));
            cmd.Parameters.AddWithValue("@dls", ParseDecimal(dls));
            cmd.Parameters.AddWithValue("@rateTurn", ParseDecimal(rateTurn));
            cmd.Parameters.AddWithValue("@rateBuild", ParseDecimal(rateBuild));
            cmd.Parameters.AddWithValue("@mdDelta", ParseDecimal(mdDelta));
            cmd.Parameters.AddWithValue("@tvdDelta", ParseDecimal(tvdDelta));
            cmd.Parameters.AddWithValue("@statusTrajStation", (object?)statusTrajStation ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sourceFile", (object?)sourceFile ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void SaveMudLog(string uid, string? wellUid, string? wellboreUid, string? name,
            string? mudLogCompany, string? mudLogEngineers, string? objectGrowing, string? dTim,
            string? startMd, string? endMd, string? dTimCreation, string? dTimLastChange, string sourceFile)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO mud_logs (uid, well_uid, wellbore_uid, name, mud_log_company, mud_log_engineers,
                  object_growing, d_tim, start_md, end_md, d_tim_creation, d_tim_last_change, source_file)
                  VALUES (@uid, @wellUid, @wellboreUid, @name, @mudLogCompany, @mudLogEngineers,
                  @objectGrowing, @dTim, @startMd, @endMd, @dTimCreation, @dTimLastChange, @sourceFile);",
                GetConnection());

            cmd.Parameters.AddWithValue("@uid", uid);
            cmd.Parameters.AddWithValue("@wellUid", EmptyToNull(wellUid));
            cmd.Parameters.AddWithValue("@wellboreUid", EmptyToNull(wellboreUid));
            cmd.Parameters.AddWithValue("@name", (object?)name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@mudLogCompany", (object?)mudLogCompany ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@mudLogEngineers", (object?)mudLogEngineers ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@objectGrowing", (object?)objectGrowing ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dTim", ParseDateTime(dTim));
            cmd.Parameters.AddWithValue("@startMd", ParseDecimal(startMd));
            cmd.Parameters.AddWithValue("@endMd", ParseDecimal(endMd));
            cmd.Parameters.AddWithValue("@dTimCreation", ParseDateTime(dTimCreation));
            cmd.Parameters.AddWithValue("@dTimLastChange", ParseDateTime(dTimLastChange));
            cmd.Parameters.AddWithValue("@sourceFile", (object?)sourceFile ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void SaveGeologyInterval(string mudLogUid, string? uid, string? typeLithology,
            string? mdTop, string? mdBottom, string? dTimCreation, string? dTimLastChange, string sourceFile)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO geology_intervals (mud_log_uid, uid, type_lithology, md_top, md_bottom, d_tim_creation, d_tim_last_change, source_file)
                  VALUES (@mudLogUid, @uid, @typeLithology, @mdTop, @mdBottom, @dTimCreation, @dTimLastChange, @sourceFile);",
                GetConnection());

            cmd.Parameters.AddWithValue("@mudLogUid", mudLogUid);
            cmd.Parameters.AddWithValue("@uid", (object?)uid ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@typeLithology", (object?)typeLithology ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@mdTop", ParseDecimal(mdTop));
            cmd.Parameters.AddWithValue("@mdBottom", ParseDecimal(mdBottom));
            cmd.Parameters.AddWithValue("@dTimCreation", ParseDateTime(dTimCreation));
            cmd.Parameters.AddWithValue("@dTimLastChange", ParseDateTime(dTimLastChange));
            cmd.Parameters.AddWithValue("@sourceFile", (object?)sourceFile ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void SaveLithology(string geologyIntervalUid, string? uid, string? type, string? codeLith,
            string? lithPc, string? description, string sourceFile)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO lithologies (geology_interval_uid, uid, type, code_lith, lith_pc, description, source_file)
                  VALUES (@geologyIntervalUid, @uid, @type, @codeLith, @lithPc, @description, @sourceFile);",
                GetConnection());

            cmd.Parameters.AddWithValue("@geologyIntervalUid", geologyIntervalUid);
            cmd.Parameters.AddWithValue("@uid", (object?)uid ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@type", (object?)type ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@codeLith", (object?)codeLith ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@lithPc", ParseDecimal(lithPc));
            cmd.Parameters.AddWithValue("@description", (object?)description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sourceFile", (object?)sourceFile ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void SaveLog(string uid, string? wellUid, string? wellboreUid, string? name,
            string? indexType, string? direction, string? objectGrowing, string? dTimCreation, string? dTimLastChange, string sourceFile)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO logs (uid, well_uid, wellbore_uid, name, index_type, direction, object_growing, d_tim_creation, d_tim_last_change, source_file)
                  VALUES (@uid, @wellUid, @wellboreUid, @name, @indexType, @direction, @objectGrowing, @dTimCreation, @dTimLastChange, @sourceFile);",
                GetConnection());

            cmd.Parameters.AddWithValue("@uid", uid);
            cmd.Parameters.AddWithValue("@wellUid", EmptyToNull(wellUid));
            cmd.Parameters.AddWithValue("@wellboreUid", EmptyToNull(wellboreUid));
            cmd.Parameters.AddWithValue("@name", (object?)name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@indexType", (object?)indexType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@direction", (object?)direction ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@objectGrowing", (object?)objectGrowing ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dTimCreation", ParseDateTime(dTimCreation));
            cmd.Parameters.AddWithValue("@dTimLastChange", ParseDateTime(dTimLastChange));
            cmd.Parameters.AddWithValue("@sourceFile", (object?)sourceFile ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void SaveRig(string uid, string? wellUid, string? wellboreUid, string? name,
            string? owner, string? typeRig, string? dTimCreation, string? dTimLastChange, string sourceFile)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO rigs (uid, well_uid, wellbore_uid, name, owner, type_rig, d_tim_creation, d_tim_last_change, source_file)
                  VALUES (@uid, @wellUid, @wellboreUid, @name, @owner, @typeRig, @dTimCreation, @dTimLastChange, @sourceFile);",
                GetConnection());

            cmd.Parameters.AddWithValue("@uid", uid);
            cmd.Parameters.AddWithValue("@wellUid", EmptyToNull(wellUid));
            cmd.Parameters.AddWithValue("@wellboreUid", EmptyToNull(wellboreUid));
            cmd.Parameters.AddWithValue("@name", (object?)name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@owner", (object?)owner ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@typeRig", (object?)typeRig ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dTimCreation", ParseDateTime(dTimCreation));
            cmd.Parameters.AddWithValue("@dTimLastChange", ParseDateTime(dTimLastChange));
            cmd.Parameters.AddWithValue("@sourceFile", (object?)sourceFile ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void SaveTubular(string uid, string? wellUid, string? wellboreUid, string? name,
            string? typeTubularAssy, string? dTimCreation, string? dTimLastChange, string sourceFile)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO tubulars (uid, well_uid, wellbore_uid, name, type_tubular_assy, d_tim_creation, d_tim_last_change, source_file)
                  VALUES (@uid, @wellUid, @wellboreUid, @name, @typeTubularAssy, @dTimCreation, @dTimLastChange, @sourceFile);",
                GetConnection());

            cmd.Parameters.AddWithValue("@uid", uid);
            cmd.Parameters.AddWithValue("@wellUid", EmptyToNull(wellUid));
            cmd.Parameters.AddWithValue("@wellboreUid", EmptyToNull(wellboreUid));
            cmd.Parameters.AddWithValue("@name", (object?)name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@typeTubularAssy", (object?)typeTubularAssy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dTimCreation", ParseDateTime(dTimCreation));
            cmd.Parameters.AddWithValue("@dTimLastChange", ParseDateTime(dTimLastChange));
            cmd.Parameters.AddWithValue("@sourceFile", (object?)sourceFile ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void SaveTubularComponent(string tubularUid, string? uid, string? typeTubularComp, string? sequence,
            string? idMeasure, string? odMeasure, string? len, string? lenJointAv, string? numJointStand,
            string? wtPerLen, string? vendor, string sourceFile)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO tubular_components (tubular_uid, uid, type_tubular_comp, sequence, id_measure, od_measure, len,
                  len_joint_av, num_joint_stand, wt_per_len, vendor, source_file)
                  VALUES (@tubularUid, @uid, @typeTubularComp, @sequence, @idMeasure, @odMeasure, @len,
                  @lenJointAv, @numJointStand, @wtPerLen, @vendor, @sourceFile);",
                GetConnection());

            cmd.Parameters.AddWithValue("@tubularUid", tubularUid);
            cmd.Parameters.AddWithValue("@uid", (object?)uid ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@typeTubularComp", (object?)typeTubularComp ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sequence", ParseInt(sequence));
            cmd.Parameters.AddWithValue("@idMeasure", ParseDecimal(idMeasure));
            cmd.Parameters.AddWithValue("@odMeasure", ParseDecimal(odMeasure));
            cmd.Parameters.AddWithValue("@len", ParseDecimal(len));
            cmd.Parameters.AddWithValue("@lenJointAv", ParseDecimal(lenJointAv));
            cmd.Parameters.AddWithValue("@numJointStand", ParseInt(numJointStand));
            cmd.Parameters.AddWithValue("@wtPerLen", ParseDecimal(wtPerLen));
            cmd.Parameters.AddWithValue("@vendor", (object?)vendor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sourceFile", (object?)sourceFile ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void SaveWbGeometry(string uid, string? wellUid, string? wellboreUid, string? name,
            string? dTimReport, string? mdBottom, string? dTimCreation, string? dTimLastChange, string sourceFile)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO wb_geometrys (uid, well_uid, wellbore_uid, name, d_tim_report, md_bottom, d_tim_creation, d_tim_last_change, source_file)
                  VALUES (@uid, @wellUid, @wellboreUid, @name, @dTimReport, @mdBottom, @dTimCreation, @dTimLastChange, @sourceFile);",
                GetConnection());

            cmd.Parameters.AddWithValue("@uid", uid);
            cmd.Parameters.AddWithValue("@wellUid", EmptyToNull(wellUid));
            cmd.Parameters.AddWithValue("@wellboreUid", EmptyToNull(wellboreUid));
            cmd.Parameters.AddWithValue("@name", (object?)name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dTimReport", ParseDateTime(dTimReport));
            cmd.Parameters.AddWithValue("@mdBottom", ParseDecimal(mdBottom));
            cmd.Parameters.AddWithValue("@dTimCreation", ParseDateTime(dTimCreation));
            cmd.Parameters.AddWithValue("@dTimLastChange", ParseDateTime(dTimLastChange));
            cmd.Parameters.AddWithValue("@sourceFile", (object?)sourceFile ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void SaveWbGeometrySection(string wbGeometryUid, string? uid, string? typeHoleCasing,
            string? mdTop, string? mdBottom, string? idSection, string? odSection, string sourceFile)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO wb_geometry_sections (wb_geometry_uid, uid, type_hole_casing, md_top, md_bottom, id_section, od_section, source_file)
                  VALUES (@wbGeometryUid, @uid, @typeHoleCasing, @mdTop, @mdBottom, @idSection, @odSection, @sourceFile);",
                GetConnection());

            cmd.Parameters.AddWithValue("@wbGeometryUid", wbGeometryUid);
            cmd.Parameters.AddWithValue("@uid", (object?)uid ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@typeHoleCasing", (object?)typeHoleCasing ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@mdTop", ParseDecimal(mdTop));
            cmd.Parameters.AddWithValue("@mdBottom", ParseDecimal(mdBottom));
            cmd.Parameters.AddWithValue("@idSection", ParseDecimal(idSection));
            cmd.Parameters.AddWithValue("@odSection", ParseDecimal(odSection));
            cmd.Parameters.AddWithValue("@sourceFile", (object?)sourceFile ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void SaveBhaRun(string uid, string? wellUid, string? wellboreUid, string? name,
            string? tubularRef, string? dTimStart, string? dTimStop, string? numStringRun,
            string? dTimCreation, string? dTimLastChange, string sourceFile)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO bha_runs (uid, well_uid, wellbore_uid, name, tubular_ref, d_tim_start, d_tim_stop,
                  num_string_run, d_tim_creation, d_tim_last_change, source_file)
                  VALUES (@uid, @wellUid, @wellboreUid, @name, @tubularRef, @dTimStart, @dTimStop,
                  @numStringRun, @dTimCreation, @dTimLastChange, @sourceFile);",
                GetConnection());

            cmd.Parameters.AddWithValue("@uid", uid);
            cmd.Parameters.AddWithValue("@wellUid", EmptyToNull(wellUid));
            cmd.Parameters.AddWithValue("@wellboreUid", EmptyToNull(wellboreUid));
            cmd.Parameters.AddWithValue("@name", (object?)name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tubularRef", (object?)tubularRef ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dTimStart", ParseDateTime(dTimStart));
            cmd.Parameters.AddWithValue("@dTimStop", ParseDateTime(dTimStop));
            cmd.Parameters.AddWithValue("@numStringRun", ParseInt(numStringRun));
            cmd.Parameters.AddWithValue("@dTimCreation", ParseDateTime(dTimCreation));
            cmd.Parameters.AddWithValue("@dTimLastChange", ParseDateTime(dTimLastChange));
            cmd.Parameters.AddWithValue("@sourceFile", (object?)sourceFile ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void SaveAttachment(string uid, string? wellUid, string? wellboreUid, string? name,
            string? fileName, string? description, string? dTimCreation, string? dTimLastChange, string sourceFile)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO attachments (uid, well_uid, wellbore_uid, name, file_name, description, d_tim_creation, d_tim_last_change, source_file)
                  VALUES (@uid, @wellUid, @wellboreUid, @name, @fileName, @description, @dTimCreation, @dTimLastChange, @sourceFile);",
                GetConnection());

            cmd.Parameters.AddWithValue("@uid", uid);
            cmd.Parameters.AddWithValue("@wellUid", EmptyToNull(wellUid));
            cmd.Parameters.AddWithValue("@wellboreUid", EmptyToNull(wellboreUid));
            cmd.Parameters.AddWithValue("@name", (object?)name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@fileName", (object?)fileName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@description", (object?)description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dTimCreation", ParseDateTime(dTimCreation));
            cmd.Parameters.AddWithValue("@dTimLastChange", ParseDateTime(dTimLastChange));
            cmd.Parameters.AddWithValue("@sourceFile", (object?)sourceFile ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void SaveFormationMarker(string uid, string? wellUid, string? wellboreUid, string? name,
            string? md, string? tvd, string? formationLithology, string? dTimCreation, string? dTimLastChange, string sourceFile)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO formation_markers (uid, well_uid, wellbore_uid, name, md, tvd, formation_lithology, d_tim_creation, d_tim_last_change, source_file)
                  VALUES (@uid, @wellUid, @wellboreUid, @name, @md, @tvd, @formationLithology, @dTimCreation, @dTimLastChange, @sourceFile);",
                GetConnection());

            cmd.Parameters.AddWithValue("@uid", uid);
            cmd.Parameters.AddWithValue("@wellUid", EmptyToNull(wellUid));
            cmd.Parameters.AddWithValue("@wellboreUid", EmptyToNull(wellboreUid));
            cmd.Parameters.AddWithValue("@name", (object?)name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@md", ParseDecimal(md));
            cmd.Parameters.AddWithValue("@tvd", ParseDecimal(tvd));
            cmd.Parameters.AddWithValue("@formationLithology", (object?)formationLithology ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dTimCreation", ParseDateTime(dTimCreation));
            cmd.Parameters.AddWithValue("@dTimLastChange", ParseDateTime(dTimLastChange));
            cmd.Parameters.AddWithValue("@sourceFile", (object?)sourceFile ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void SaveMessage(string uid, string? wellUid, string? wellboreUid, string? name,
            string? dTim, string? md, string? typeMessage, string? messageText,
            string? dTimCreation, string? dTimLastChange, string sourceFile)
        {
            using var cmd = new SqlCommand(
                @"INSERT INTO messages (uid, well_uid, wellbore_uid, name, d_tim, md, type_message, message_text,
                  d_tim_creation, d_tim_last_change, source_file)
                  VALUES (@uid, @wellUid, @wellboreUid, @name, @dTim, @md, @typeMessage, @messageText,
                  @dTimCreation, @dTimLastChange, @sourceFile);",
                GetConnection());

            cmd.Parameters.AddWithValue("@uid", uid);
            cmd.Parameters.AddWithValue("@wellUid", EmptyToNull(wellUid));
            cmd.Parameters.AddWithValue("@wellboreUid", EmptyToNull(wellboreUid));
            cmd.Parameters.AddWithValue("@name", (object?)name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dTim", ParseDateTime(dTim));
            cmd.Parameters.AddWithValue("@md", ParseDecimal(md));
            cmd.Parameters.AddWithValue("@typeMessage", (object?)typeMessage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@messageText", (object?)messageText ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dTimCreation", ParseDateTime(dTimCreation));
            cmd.Parameters.AddWithValue("@dTimLastChange", ParseDateTime(dTimLastChange));
            cmd.Parameters.AddWithValue("@sourceFile", (object?)sourceFile ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        private static object EmptyToNull(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "NULL") return DBNull.Value;
            return value;
        }

        private static object ParseDateTime(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "NULL") return DBNull.Value;
            if (DateTime.TryParse(value, out var dt)) return dt;
            return DBNull.Value;
        }

        private static object ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "NULL") return DBNull.Value;
            if (decimal.TryParse(value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
            return DBNull.Value;
        }

        private static object ParseInt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "NULL") return DBNull.Value;
            if (int.TryParse(value, out var i)) return i;
            return DBNull.Value;
        }

        public IReadOnlyList<WellInfo> GetAllWells()
        {
            var list = new List<WellInfo>();
            using var cmd = new SqlCommand(
                @"SELECT uid, name, time_zone, status_well, d_tim_creation, d_tim_last_change, source_file, processed_at
                  FROM wells ORDER BY name, uid",
                GetConnection());
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new WellInfo
                {
                    Uid = r.GetString(0),
                    Name = r.IsDBNull(1) ? null : r.GetString(1),
                    TimeZone = r.IsDBNull(2) ? null : r.GetString(2),
                    StatusWell = r.IsDBNull(3) ? null : r.GetString(3),
                    DTimCreation = r.IsDBNull(4) ? null : r.GetDateTime(4),
                    DTimLastChange = r.IsDBNull(5) ? null : r.GetDateTime(5),
                    SourceFile = r.IsDBNull(6) ? null : r.GetString(6),
                    ProcessedAt = r.IsDBNull(7) ? null : r.GetDateTime(7)
                });
            }
            return list;
        }

        public void Dispose() => _connection?.Dispose();
    }
}
