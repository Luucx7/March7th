namespace Infrastructure.Repository
{
    public class UsersRepository
    {
        private readonly MarchDbContext _dbContext;

        public UsersRepository(MarchDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
