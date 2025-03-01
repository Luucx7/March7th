using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Entity
{
    [Table("users")]
    public class UserEntity
    {
        [Key]
        public ulong user_id { get; set; }
        [Required]
        public int birth_day { get; set; }
        [Required]
        public int birth_month { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime created_at { get; set; }
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime birthday_updated_at { get; set; }

        public virtual ICollection<ServerUserEntity> UserServers { get; set; } = new List<ServerUserEntity>();
    }
}
