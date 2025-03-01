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
    }
}
