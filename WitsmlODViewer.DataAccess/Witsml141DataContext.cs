using WitsmlODViewer.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace WitsmlODViewer.DataAccess;

public class Witsml141DataContext : DbContext
{
    public Witsml141DataContext()
    {
    }

    public Witsml141DataContext(DbContextOptions<Witsml141DataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Well> Wells { get; set; }
    public virtual DbSet<Rig> Rigs { get; set; }
    public virtual DbSet<Trajectory> Trajectories { get; set; }
    public virtual DbSet<TrajectoryStation> TrajectoryStations { get; set; }
    public virtual DbSet<WellAlarm> WellAlarms { get; set; }
    public virtual DbSet<WellAlarmEvent> WellAlarmEvents { get; set; }
    public virtual DbSet<WellAlarmSubscription> WellAlarmSubscriptions { get; set; }
    public virtual DbSet<WellAlarmAudit> WellAlarmAudits { get; set; }

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
        modelBuilder.Entity<WellAlarm>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.WellUid);
        });
        modelBuilder.Entity<WellAlarmEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.WellAlarmId);
            entity.HasOne(e => e.WellAlarm).WithMany().HasForeignKey(e => e.WellAlarmId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<WellAlarmSubscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.WellAlarmId, e.SubscriberKey }).IsUnique();
            entity.HasOne(e => e.WellAlarm).WithMany().HasForeignKey(e => e.WellAlarmId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<WellAlarmAudit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.WellAlarmId);
        });
    }
}
