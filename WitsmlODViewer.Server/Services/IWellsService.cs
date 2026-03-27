using WitsmlODViewer.Server.DTOs;

namespace WitsmlODViewer.Server.Services;

public interface IWellsService
{
    Task<IEnumerable<WellDTO>> GetAllAsync();
}
