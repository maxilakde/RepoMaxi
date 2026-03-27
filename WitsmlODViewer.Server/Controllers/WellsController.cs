using WitsmlODViewer.Server.DTOs;
using WitsmlODViewer.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace WitsmlODViewer.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class WellsController : ControllerBase
{
    private readonly IWellsService _wellsService;

    public WellsController(IWellsService wellsService)
    {
        _wellsService = wellsService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WellDTO>>> GetWells()
    {
        var wells = await _wellsService.GetAllAsync();
        return Ok(wells);
    }
}
