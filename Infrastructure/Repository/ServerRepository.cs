using Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

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

            // When joining a previously left server, it will be needed to resync it so mark it as not ready
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
        public async Task SyncServerMembers(ulong serverId, IReadOnlyCollection<ulong> users)
        {
            // Obter a lista de user_ids na base de dados para o servidor
            List<ulong> usuariosNoBanco = await _dbContext.ServerUsers
                .Where(us => us.server_id == serverId)
                .Select(us => us.user_id)
                .ToListAsync();

            // Encontrar os user_ids a serem removidos (presentes no banco, mas não em memória)
            List<ulong> userIdsParaRemover = usuariosNoBanco.Except(users).ToList();

            // Encontrar os user_ids a serem adicionados (presentes em memória, mas não no banco)
            List<ulong> userIdsParaAdicionar = users.Except(usuariosNoBanco).ToList();

            // Remover os usuários que não estão mais na lista em memória
            List<ServerUserEntity> usuariosRemover = _dbContext.ServerUsers
                .Where(us => us.server_id == serverId && userIdsParaRemover.Contains(us.user_id))
                .ToList();

            _dbContext.ServerUsers.RemoveRange(usuariosRemover);

            // Adicionar os novos usuários que estão na lista em memória
            List<ServerUserEntity> usuariosAdicionar = userIdsParaAdicionar.Select(userId => new ServerUserEntity
            {
                server_id = serverId,
                user_id = userId
            }).ToList();

            await _dbContext.ServerUsers.AddRangeAsync(usuariosAdicionar);

            // Salvar as alterações
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveServer(ulong guildId)
        {
            ServerEntity? serverEntity = await _dbContext.Servers.Where(s => s.server_id == guildId).FirstOrDefaultAsync();
            if (serverEntity == null) throw new InvalidOperationException("Server not found");

            // Removing the user relations if the bot left the server, it will not be needed anymore
            // If the bot joins the server again, it will sync the users anyway, so the chance that it would be outdated is 100%
            await _dbContext.ServerUsers.Where((su) => su.server_id == guildId).ExecuteDeleteAsync();

            // Only mark the server as removed and inactive, rather than deleting it all
            // It makes possible to have a historic of servers, keep old settings in case of mistake lefts and be able to ban servers
            serverEntity.removed_at = DateTime.UtcNow;
            serverEntity.active = false;
            serverEntity.ready = false;

            await _dbContext.SaveChangesAsync();
        }

        public async Task BanServer(ulong guildId)
        {
            ServerEntity? serverEntity = await _dbContext.Servers.Where(s => s.server_id == guildId).FirstOrDefaultAsync();
            if (serverEntity == null) throw new InvalidOperationException("Server not found");

            await _dbContext.ServerUsers.Where((su) => su.server_id == guildId).ExecuteDeleteAsync();
            
            // If the server was banned, it is then inactive and left
            serverEntity.removed_at = DateTime.UtcNow;
            serverEntity.banned_at = DateTime.Now;
            serverEntity.active = false;

            await _dbContext.SaveChangesAsync();
        }
    }
}
