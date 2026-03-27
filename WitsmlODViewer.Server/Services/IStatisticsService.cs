namespace WitsmlODViewer.Server.Services;

public interface IStatisticsService
{
    Task<StatisticsResponse> GetDataAsync(string wellUid, StatisticsRequest request);
    Task<IEnumerable<VariableInfo>> GetAvailableVariablesAsync(string wellUid);
}

public class StatisticsRequest
{
    public string IndexType { get; set; } = "depth"; // depth | time
    public decimal? MinDepth { get; set; }
    public decimal? MaxDepth { get; set; }
    public DateTime? MinTime { get; set; }
    public DateTime? MaxTime { get; set; }
    public List<string> Variables { get; set; } = new();
}

public class StatisticsResponse
{
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
    public List<string> Columns { get; set; } = new();
}

public class VariableInfo
{
    public string Mnemonic { get; set; } = "";
    public string Caption { get; set; } = "";
    public string Unit { get; set; } = "";
}
