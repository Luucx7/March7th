using Castle.Core.Logging;
using Infrastructure;
using Infrastructure.Entity;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Services
{
    public class UserBirthdayService
    {
        private readonly ILogger<UserBirthdayService> _logger;
        private readonly UsersRepository _usersRepository;

        public UserBirthdayService(UsersRepository usersRepository, ILogger<UserBirthdayService> logger)
        {
            _usersRepository = usersRepository;
            _logger = logger;
        }

        public async Task<UserEntity> CreateUser(ulong userId, short day, short month)
        {
            return await this._usersRepository.CreateUser(userId, day, month);
        }

        public async Task<(UserEntity?, UpdateBirthdayResult)> UpdateBirthday(ulong userId, short day, short month)
        {
            UserEntity? userEntity = await this._usersRepository.FindUserById(userId);
            if (userEntity == null) return (userEntity, UpdateBirthdayResult.UserNotFound);

            if (userEntity.birthday_updated_amount >= 2) return (userEntity, UpdateBirthdayResult.UpdateLimitReached);
            if (IsBirthdayUpdateOnCooldown(userEntity)) return (userEntity, UpdateBirthdayResult.CooldownActive);

            userEntity = await _usersRepository.UpdateUserBirthday(userId, day, month);

            UpdateBirthdayResult result = UpdateBirthdayResult.Success;
            if (userEntity == null) result = UpdateBirthdayResult.UserNotFound;

            return (userEntity, result);
        }
        public bool IsBirthdayUpdateOnCooldown(UserEntity userEntity)
        {
            TimeSpan cooldownPeriod = TimeSpan.FromDays(60);
            DateTime lastUpdate = userEntity.birthday_updated_at;

            return (DateTime.UtcNow - lastUpdate) <= cooldownPeriod;
        }
    }

    public enum UpdateBirthdayResult
    {
        Success,
        UserNotFound,
        UpdateLimitReached,
        CooldownActive
    }
}
