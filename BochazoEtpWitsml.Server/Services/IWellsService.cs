using BochazoEtpWitsml.Server.DTOs;

namespace BochazoEtpWitsml.Server.Services;

public interface IWellsService
{
    Task<IEnumerable<WellDTO>> GetAllAsync();
}
