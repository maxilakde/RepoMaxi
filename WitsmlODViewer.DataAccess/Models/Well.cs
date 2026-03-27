using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WitsmlODViewer.DataAccess.Models;

[Table("wells")]
public class Well
{
    [Key]
    [Column("uid")]
    [StringLength(100)]
    public string Uid { get; set; } = null!;

    [Column("name")]
    [StringLength(255)]
    public string? Name { get; set; }

    [Column("time_zone")]
    [StringLength(50)]
    public string? TimeZone { get; set; }

    [Column("status_well")]
    [StringLength(50)]
    public string? StatusWell { get; set; }

    [Column("d_tim_creation", TypeName = "datetime2")]
    public DateTime? DTimCreation { get; set; }

    [Column("d_tim_last_change", TypeName = "datetime2")]
    public DateTime? DTimLastChange { get; set; }

    [Column("source_file")]
    [StringLength(500)]
    public string? SourceFile { get; set; }

    [Column("processed_at", TypeName = "datetime2")]
    public DateTime? ProcessedAt { get; set; }
}
