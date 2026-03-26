using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BochazoEtpWitsml.DataAccess.Models;

[Table("rigs")]
public class Rig
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

    [Column("name")]
    [StringLength(255)]
    public string? Name { get; set; }

    [Column("owner")]
    [StringLength(255)]
    public string? Owner { get; set; }

    [Column("type_rig")]
    [StringLength(50)]
    public string? TypeRig { get; set; }
}
