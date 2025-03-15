using Discord.Interactions;
using Discord.Services;
using Infrastructure;
using Infrastructure.Entity;
using Infrastructure.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Discord.Commands
{
    public class SetBirthdayCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ILogger<SetBirthdayCommand> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly UserBirthdayService _userBirthdayService;

        private short? Day { get; set; }
        private short? Month { get; set; }

        public SetBirthdayCommand(UserBirthdayService usersRepository, IServiceProvider serviceProvider, ILogger<SetBirthdayCommand> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            this._userBirthdayService = usersRepository;
        }

        [SlashCommand("aniversario", "Define seu aniversário",  true, Interactions.RunMode.Async)]
        public async Task CommandExecuted(short day, short month)
        {
            if (!isValidDayAndMonth(day, month))
            {
                await RespondInvalidDate();
                return;
            }

            ulong userId = base.Context.User.Id;
            this.Day = day;
            this.Month = month;

            var scope = this._serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MarchDbContext>();

            UserEntity? userEntity = await dbContext.Users.FindAsync(userId);
            if (userEntity == null)
            {
                userEntity = await _userBirthdayService.CreateUser(userId, (short)day, (short)month);

                await RespondBirthdayDefined();
            } else
            {
                var (entity, result) = await _userBirthdayService.UpdateBirthday(userId, (short)day, (short)month);

                switch (result)
                {
                    case UpdateBirthdayResult.Success:
                        await RespondBirthdayDefined();
                        break;
                    case UpdateBirthdayResult.UpdateLimitReached:
                        await RespondUpdateAmountLimited();
                        break;
                    case UpdateBirthdayResult.CooldownActive:
                        await RespondUpdateCooldown(entity!.birthday_updated_at); // it never returns cooldownactive if entity is null
                        break;
                    case UpdateBirthdayResult.UserNotFound:
                        await RespondError();
                        break;
                }
            }
        }

        public async Task RespondInvalidDate()
        {
            var builder = new EmbedBuilder()
                .WithTitle("Erro!")
                .WithThumbnailUrl("https://github.com/Luucx7/temp-repo/blob/main/march-sorry.png?raw=true")
                .WithDescription("Valor inválidos")
                .WithColor(Color.Red);

            await RespondAsync(embed: builder.Build());
        }

        private async Task RespondBirthdayDefined()
        {
            string dayValue = this.Day.GetValueOrDefault(0) == 1 ? "1º" : this.Day.GetValueOrDefault(0).ToString().PadLeft(2, '0');
            string monthName = MonthNameInPortuguese(this.Month.GetValueOrDefault(0));

            short day = this.Day.GetValueOrDefault(0);
            short month = this.Month.GetValueOrDefault(0);

            var embedMessage = new EmbedBuilder()
                .WithColor(Color.Parse("7FFF00", ColorType.CssHexColor))
                .WithTitle("Aniversário definido");

            if (day == 7 && month == 3)
            {
                embedMessage
                    .WithImageUrl("https://github.com/Luucx7/temp-repo/blob/main/march-heart-art.jpg?raw=true")
                    .WithDescription($"Seu aniversário foi definido para {dayValue} de {monthName}!\nO mesmo que o meu!");
            } else if (day == 1 && month == 1) {
                embedMessage
                    .WithThumbnailUrl("https://cdn.pixabay.com/photo/2020/04/05/22/00/fireworks-5007820_1280.jpg")
                    .WithDescription($"Seu aniversário foi definido para {dayValue} de {monthName}!\nFeliz ano novo!");
            } else if (day == 1 && month == 4)
            {
                embedMessage
                    .WithThumbnailUrl("https://static.wikia.nocookie.net/houkai-star-rail/images/9/97/Sticker_PPG_04_Silver_Wolf_01.png")
                    .WithDescription($"Hackeei o celular dela, mas vou salvar aqui teu aniversário como 1º de Abril");
            } else if (day == 25 && month == 12) {
                embedMessage
                    .WithThumbnailUrl("https://upload-os-bbs.hoyolab.com/upload/2022/05/11/90891105/71b68d1c8b7e58b4912639001e40ae1d_4811405901003149858.png")
                    .WithFooter("Image by Hoyolab @Nashuuuu")
                    .WithDescription($"Seu aniversário foi definido para {dayValue} de {monthName}!\nFeliz natal!");
            } else if (day == 31 && month == 12) {
                embedMessage
                    .WithThumbnailUrl("https://cdn.pixabay.com/photo/2020/04/05/22/00/fireworks-5007820_1280.jpg")
                    .WithDescription($"Seu aniversário foi definido para {dayValue} de {monthName}!\nFeliz reveillón!");
            } else
            {
                embedMessage
                    .WithThumbnailUrl("https://github.com/Luucx7/temp-repo/blob/main/march-thumbsup.png?raw=true")
                    .WithDescription($"Seu aniversário foi definido para {dayValue} de {monthName}");
            }



            await RespondAsync(embed: embedMessage.Build());
        }
        private async Task RespondUpdateAmountLimited()
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle("Limite de alterações atingido")
                .WithThumbnailUrl("https://github.com/Luucx7/temp-repo/blob/main/march-angry.png?raw=true")
                .WithDescription("Porque você está mudando tanto de aniversário? Você já mudou duas vezes!")
                .WithColor(Color.Red);

            await RespondAsync(embed: builder.Build());
        }
        private async Task RespondUpdateCooldown(DateTime lastUpdate)
        {
            string lastUpdateDateString = lastUpdate.ToString("dd/MM/yyyy HHhmm");

            DateTime nextUpdateDate = lastUpdate.AddDays(60);
            string nextUpdateDateString = nextUpdateDate.ToString("dd/MM/yyyy HHhmm");

            EmbedBuilder builder = new EmbedBuilder()
                .WithThumbnailUrl("https://github.com/Luucx7/temp-repo/blob/main/march-angry.png?raw=true")
                .WithTitle("Erro!")
                .WithDescription($"Eu não confio em você!\nVocê atualizou seu aniversário em {lastUpdateDateString} e só vou deixar atualizar denovo em {nextUpdateDateString}")
                .WithColor(Color.Red);

            await RespondAsync(embed: builder.Build());
        }

        private async Task RespondError()
        {
            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithTitle("Erro!")
                .WithDescription("Ocorreu um erro desconhecido! Me desculpa")
                .WithColor(Color.DarkRed)
                .WithThumbnailUrl("https://github.com/Luucx7/temp-repo/blob/main/march-sorry.png?raw=true");

            await RespondAsync(embed: embedBuilder.Build());
        }

        private static bool isValidDayAndMonth(short day, short month)
        {
            switch(month)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    return day >= 1 && day <= 31;
                case 4:
                case 6:
                case 9:
                case 11:
                    return day >= 1 && day <= 30;
                case 2:
                    return day >= 1 && day <= 29;
                default:
                    return false;
            }
        }
        private static string MonthNameInPortuguese(short month)
        {
            return month switch
            {
                1 => "Janeiro",
                2 => "Fevereiro",
                3 => "Março",
                4 => "Abril",
                5 => "Maio",
                6 => "Junho",
                7 => "Julho",
                8 => "Agosto",
                9 => "Setembro",
                10 => "Outubro",
                11 => "Novembro",
                12 => "Dezembro",
                _ => throw new ArgumentOutOfRangeException(nameof(month), "Mês inválido.")
            };
        }

    }
}
