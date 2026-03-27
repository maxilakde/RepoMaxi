using WitsmlODViewer.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace WitsmlODViewer.Server.Services;

public class StatisticsService : IStatisticsService
{
    private readonly WitsmlDataContext _context;

    private static readonly Dictionary<string, (string Caption, string Unit)> VariableMeta = new(StringComparer.OrdinalIgnoreCase)
    {
        ["md"] = ("Profundidad medida", "m"),
        ["tvd"] = ("Profundidad vertical", "m"),
        ["incl"] = ("Inclinación", "°"),
        ["azi"] = ("Acimut", "°"),
        ["disp_ns"] = ("Desplazamiento N-S", "m"),
        ["disp_ew"] = ("Desplazamiento E-O", "m"),
        ["vert_sect"] = ("Sección vertical", "m"),
        ["dls"] = ("Severidad del ángulo", "°/30m"),
        ["d_tim_stn"] = ("Fecha/Hora estación", ""),
    };

    public StatisticsService(WitsmlDataContext context)
    {
        _context = context;
    }

    public Task<IEnumerable<VariableInfo>> GetAvailableVariablesAsync(string wellUid)
    {
        return Task.FromResult(VariableMeta.Select(kv => new VariableInfo
        {
            Mnemonic = kv.Key,
            Caption = kv.Value.Caption,
            Unit = kv.Value.Unit
        }));
    }

    public async Task<StatisticsResponse> GetDataAsync(string wellUid, StatisticsRequest request)
    {
        var stations = await _context.TrajectoryStations
            .Where(ts => _context.Trajectories
                .Where(t => t.WellUid == wellUid)
                .Select(t => t.Uid)
                .Contains(ts.TrajectoryUid))
            .OrderBy(ts => ts.Md ?? 0)
            .ToListAsync();

        var variables = request.Variables.Count > 0
            ? request.Variables.Where(v => VariableMeta.ContainsKey(v)).ToList()
            : new List<string> { "md", "tvd", "incl", "azi" };

        if (variables.Count == 0)
            variables = new List<string> { "md", "tvd", "incl", "azi" };

        var filtered = stations.AsEnumerable();
        if (request.IndexType == "depth" && (request.MinDepth.HasValue || request.MaxDepth.HasValue))
        {
            if (request.MinDepth.HasValue)
                filtered = filtered.Where(s => (s.Md ?? 0) >= request.MinDepth.Value);
            if (request.MaxDepth.HasValue)
                filtered = filtered.Where(s => (s.Md ?? 0) <= request.MaxDepth.Value);
        }
        else if (request.IndexType == "time" && (request.MinTime.HasValue || request.MaxTime.HasValue))
        {
            if (request.MinTime.HasValue)
                filtered = filtered.Where(s => s.DTimStn >= request.MinTime);
            if (request.MaxTime.HasValue)
                filtered = filtered.Where(s => s.DTimStn <= request.MaxTime);
        }

        var rows = new List<Dictionary<string, object?>>();
        foreach (var s in filtered)
        {
            var row = new Dictionary<string, object?>();
            foreach (var v in variables)
            {
                object? val = v.ToLowerInvariant() switch
                {
                    "md" => s.Md,
                    "tvd" => s.Tvd,
                    "incl" => s.Incl,
                    "azi" => s.Azi,
                    "disp_ns" => s.DispNs,
                    "disp_ew" => s.DispEw,
                    "vert_sect" => s.VertSect,
                    "dls" => s.Dls,
                    "d_tim_stn" => s.DTimStn,
                    _ => null
                };
                row[v] = val;
            }
            rows.Add(row);
        }

        return new StatisticsResponse { Rows = rows, Columns = variables };
    }
}
