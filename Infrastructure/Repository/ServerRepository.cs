using Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class ServerRepository
    {
        private readonly MarchDbContext _dbContext;
        public ServerRepository(MarchDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task ReactivateServer(ulong serverId)
        {
            ServerEntity? server = await _dbContext.Servers.FirstOrDefaultAsync(s => s.server_id == serverId);
            if (server == null) return;

            server.ready = false;
            server.active = true;
            server.removed_at = null;
            server.added_at = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task ServerReady(ulong serverId)
        {
            ServerEntity? server = await _dbContext.Servers.FirstOrDefaultAsync(s => s.server_id == serverId);
            if (server == null) return;

            server.ready = true;

            await _dbContext.SaveChangesAsync();
        }
    }
}
