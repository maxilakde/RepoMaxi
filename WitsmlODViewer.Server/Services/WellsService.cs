using WitsmlODViewer.DataAccess;
using WitsmlODViewer.Server.DTOs;
using Microsoft.EntityFrameworkCore;

namespace WitsmlODViewer.Server.Services;

public class WellsService : IWellsService
{
    private readonly WitsmlDataContext _context;

    public WellsService(WitsmlDataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WellDTO>> GetAllAsync()
    {
        var wells = await _context.Wells
            .OrderBy(w => w.Name)
            .ThenBy(w => w.Uid)
            .ToListAsync();

        var rigsByWell = await _context.Rigs
            .Where(r => r.WellUid != null)
            .GroupBy(r => r.WellUid!)
            .ToDictionaryAsync(g => g.Key, g => g.OrderBy(r => r.Name).First());

        return wells.Select(w => new WellDTO
        {
            Uid = w.Uid,
            Name = w.Name,
            TimeZone = w.TimeZone,
            StatusWell = w.StatusWell,
            DTimCreation = w.DTimCreation,
            DTimLastChange = w.DTimLastChange,
            SourceFile = w.SourceFile,
            ProcessedAt = w.ProcessedAt,
            Rig = rigsByWell.TryGetValue(w.Uid, out var rig)
                ? new RigInfoDTO { Name = rig.Name, Owner = rig.Owner, TypeRig = rig.TypeRig }
                : null,
            HoleDepth = null,   // Feed en tiempo real (placeholder SARRAS)
            DrillDepth = null,
            CurrentActivity = null
        });
    }
}
