using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Entity
{
    [Table("users_servers")]
    public class ServerUserEntity
    {
        public ulong server_id { get; set; }
        public ulong user_id { get; set; }

        public virtual UserEntity? User { get; set; }  // Usuário pode ser nulo
        public virtual ServerEntity Server { get; set; }

    }
}
