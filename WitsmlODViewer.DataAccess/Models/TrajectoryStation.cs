using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WitsmlODViewer.DataAccess.Models;

[Table("trajectory_stations")]
public class TrajectoryStation
{
    public int Id { get; set; }

    [Column("trajectory_uid")]
    [StringLength(100)]
    public string TrajectoryUid { get; set; } = null!;

    [Column("uid")]
    [StringLength(100)]
    public string? Uid { get; set; }

    [Column("d_tim_stn", TypeName = "datetime2")]
    public DateTime? DTimStn { get; set; }

    [Column("type_traj_station")]
    [StringLength(50)]
    public string? TypeTrajStation { get; set; }

    [Column("md", TypeName = "decimal(18,4)")]
    public decimal? Md { get; set; }

    [Column("tvd", TypeName = "decimal(18,4)")]
    public decimal? Tvd { get; set; }

    [Column("incl", TypeName = "decimal(18,4)")]
    public decimal? Incl { get; set; }

    [Column("azi", TypeName = "decimal(18,4)")]
    public decimal? Azi { get; set; }

    [Column("disp_ns", TypeName = "decimal(18,6)")]
    public decimal? DispNs { get; set; }

    [Column("disp_ew", TypeName = "decimal(18,6)")]
    public decimal? DispEw { get; set; }

    [Column("vert_sect", TypeName = "decimal(18,4)")]
    public decimal? VertSect { get; set; }

    [Column("dls", TypeName = "decimal(18,6)")]
    public decimal? Dls { get; set; }

    [Column("rate_turn", TypeName = "decimal(18,6)")]
    public decimal? RateTurn { get; set; }

    [Column("rate_build", TypeName = "decimal(18,6)")]
    public decimal? RateBuild { get; set; }

    [Column("md_delta", TypeName = "decimal(18,4)")]
    public decimal? MdDelta { get; set; }

    [Column("tvd_delta", TypeName = "decimal(18,4)")]
    public decimal? TvdDelta { get; set; }

    [Column("status_traj_station")]
    [StringLength(50)]
    public string? StatusTrajStation { get; set; }
}
