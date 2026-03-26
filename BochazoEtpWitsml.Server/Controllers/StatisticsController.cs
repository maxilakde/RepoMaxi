using BochazoEtpWitsml.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace BochazoEtpWitsml.Server.Controllers;

[ApiController]
[Route("api/v1/wells/{wellUid}/statistics")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _service;

    public StatisticsController(IStatisticsService service)
    {
        _service = service;
    }

    [HttpGet("variables")]
    public async Task<ActionResult<IEnumerable<VariableInfo>>> GetVariables(string wellUid)
    {
        var vars = await _service.GetAvailableVariablesAsync(wellUid);
        return Ok(vars);
    }

    [HttpPost("data")]
    public async Task<ActionResult<StatisticsResponse>> GetData(string wellUid, [FromBody] StatisticsRequest request)
    {
        var data = await _service.GetDataAsync(wellUid, request);
        return Ok(data);
    }
}
