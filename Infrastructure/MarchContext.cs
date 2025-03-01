using Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure
{
    public class MarchDbContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public MarchDbContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseLazyLoadingProxies();
            options.UseNpgsql(Configuration.GetConnectionString("MarchDatabase"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServerUserEntity>()
                .HasKey(us => new { us.user_id, us.server_id });

            modelBuilder.Entity<ServerUserEntity>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserServers)
                .HasForeignKey(us => us.user_id)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);  // Permite que `UserId` não tenha um usuário na tabela `users`

            modelBuilder.Entity<ServerUserEntity>()
                .HasOne(us => us.Server)
                .WithMany(s => s.UserServers)
                .HasForeignKey(us => us.server_id)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<ServerEntity> Servers { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<ServerUserEntity> ServerUsers { get; set; }
    }
}
