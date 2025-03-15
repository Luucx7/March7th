using Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class UsersRepository
    {
        private readonly MarchDbContext _dbContext;

        public UsersRepository(MarchDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserEntity?> FindUserById(ulong userId)
        {
            return await this._dbContext.Users.FindAsync(userId);
        }

        public async Task<UserEntity> CreateUser(ulong userId, short birthDay, short birthMonth)
        {
            UserEntity userEntity = new()
            {
                birth_day = birthDay,
                birth_month = birthMonth,
                user_id = userId
            };

            await _dbContext.Users.AddAsync(userEntity);
            await _dbContext.SaveChangesAsync();

            return userEntity;
        }
        public async Task<UserEntity?> UpdateUserBirthday(ulong userId, short birthDay, short birthMonth)
        {
            var userEntity = await _dbContext.Users.FindAsync(userId);
            if (userEntity == null) return null;

            userEntity.birth_month = birthMonth;
            userEntity.birth_day = birthDay;
            userEntity.birthday_updated_amount += 1;
            userEntity.birthday_updated_at = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return userEntity;
        }

    }
}
