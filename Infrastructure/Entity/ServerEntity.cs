using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace Infrastructure.Entity
{
    [Table("servers")]
    public class ServerEntity
    {
        [Key]
        public ulong server_id { get; set; }
        public string server_zone { get; set; }

        [Required]
        public bool active { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public bool ready { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime added_at { get; set; }
        public DateTime? removed_at { get; set; }
        public DateTime? banned_at { get; set; }

        public virtual ICollection<ServerUserEntity> UserServers { get; set; } = new List<ServerUserEntity>();
    }

    //public class ServerEntityTypeConfiguration : IEntityTypeConfiguration<ServerEntity>
    //{
    //    public void Configure(EntityTypeBuilder<ServerEntity> builder)
    //    {
    //        builder.Property(s => s.active).IsRequired();
            
    //    }
    //}
}
