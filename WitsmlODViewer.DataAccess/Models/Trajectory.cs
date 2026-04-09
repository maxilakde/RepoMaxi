using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WitsmlODViewer.DataAccess.Models;

[Table("trajectories")]
public class Trajectory
{
    public int Id { get; set; }

    [Column("uid")]
    [StringLength(100)]
    public string Uid { get; set; } = null!;

    [Column("well_uid")]
    [StringLength(100)]
    public string? WellUid { get; set; }

    [Column("wellbore_uid")]
    [StringLength(100)]
    public string? WellboreUid { get; set; }

    [Column("naming_system")]
    [StringLength(100)]
    public string? NamingSystem { get; set; }

    [Column("definitive")]
    [StringLength(20)]
    public string? Definitive { get; set; }
}
