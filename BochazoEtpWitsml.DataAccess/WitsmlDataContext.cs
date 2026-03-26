using BochazoEtpWitsml.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace BochazoEtpWitsml.DataAccess;

public class WitsmlDataContext : DbContext
{
    public WitsmlDataContext()
    {
    }

    public WitsmlDataContext(DbContextOptions<WitsmlDataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Well> Wells { get; set; }
    public virtual DbSet<Rig> Rigs { get; set; }
    public virtual DbSet<Trajectory> Trajectories { get; set; }
    public virtual DbSet<TrajectoryStation> TrajectoryStations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Well>(entity =>
        {
            entity.HasKey(e => e.Uid);
        });
        modelBuilder.Entity<Rig>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
        modelBuilder.Entity<Trajectory>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
        modelBuilder.Entity<TrajectoryStation>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
