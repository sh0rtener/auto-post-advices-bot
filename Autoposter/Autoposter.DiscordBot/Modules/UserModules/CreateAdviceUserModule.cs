using Autoposter.DiscordBot.Services;
using Discord;
using Discord.Interactions;
using Autoposter.DomainLayer.Entities.Autoposter;
using Autoposter.BusinessLayer.Data.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Autoposter.BusinessLayer.Models;
using Autoposter.BusinessLayer.Contracts;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Autoposter.BusinessLayer.Validations;
using System.Data;

namespace Autoposter.DiscordBot.Modules.UserModules
{
    public class CreateAdviceUserModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private InteractionHandler _handler;
        private readonly AppDbContext _context;
        private readonly IPostService _postService;
        private IConfiguration? _configuration;
        private DiscordRoleValidator _roleValidator;

        // Constructor injection is also a valid way to access the dependencies
        public CreateAdviceUserModule(InteractionHandler handler, 
            AppDbContext context,
            IPostService postService,
            IConfiguration configuration,
            DiscordRoleValidator roleValidator)
        {
            _configuration = configuration;
            _handler = handler;
            _context = context;
            _postService = postService;
            _roleValidator = roleValidator;
        }

        [SlashCommand("создать-объявление", "Позволяет пользователю создать объявление, которое будет автоматически поститься")]
        public async Task MakeAdviceAsync()
            => await Context.Interaction.RespondWithModalAsync<CreateAdviceModel>("advice_model");

        [ModalInteraction("advice_model")]
        public async Task MakeAdviceResponseAsync(CreateAdviceModel model)
        {
            var guilds = Context.User.MutualGuilds.FirstOrDefault()!;
            var userRoles = guilds
                .Users.FirstOrDefault(x => x.Id == Context.User.Id)!.Roles.ToList();

            if (await _roleValidator.Validate(userRoles))
            {
                await RespondAsync("Нет доступа!", ephemeral: true);
                return;
            }

            double interval = await _context.BotSettings.Where(x => x.GuildId == guilds.Id)
                .Select(x => x.Interval).OrderByDescending(x => x)
                .FirstOrDefaultAsync();

            if (interval == 0)
            {
                await RespondAsync("На сервере проводятся настройки интервала, попробуйте добавить обьявление позже", ephemeral: true);
                return;
            }

            double timeToCreate = await _postService.TimeToCreate(Context.User.Id, interval, guilds.Id);
            Post post = await _postService.GetLastByUserAsync(Context.User.Id.ToString(), guilds.Id);

            if (timeToCreate > 0 && (post.BranchId is not null && post.ServerId is not null && post.ImageUri is not null))
            {
                await RespondAsync($"Вы не можете создать новое объявление. " +
                    $" Осталось {timeToCreate} минут", ephemeral: true);
                return;
            }

            await _postService.RemoveAllByUserId(Context.User.Id, guilds.Id);

            await _postService.AddAsync(model, Context, guilds.Id);

            List<Branch> branches = await _context.Branches
                .AsNoTracking()
                .Where(x => x.GuildId == guilds.Id)
                .Include(x => x.BranchRoles)
                .ToListAsync();

            List<Branch> branchesToRemove = new List<Branch>();

            foreach (Branch branch in branches)
            {
                bool isContain = false;
                foreach (ulong role in branch.BranchRoles.Select(x => x.RoleId))
                {
                    if (userRoles.Select(x => x.Id).Contains(role)) { isContain = true; break; }
                }

                if (!isContain && branch.BranchRoles.Count != 0) branchesToRemove.Add(branch);
            }

            branches = branches.Except(branchesToRemove).ToList();

            if (await IsCollectionIsEmpty(branches)) return;

            SelectMenuBuilder selectBranch = new SelectMenuBuilder()
            {
                CustomId = "select-branch",
                Placeholder = "Выберите канал"
            };

            foreach (Branch branch in branches) selectBranch.AddOption(branch.Name, branch.BranchId.ToString());

            ComponentBuilder components = new ComponentBuilder();
            components.WithSelectMenu(selectBranch);

            await RespondAsync($"Выберите канал для постинга:", components: components.Build(), ephemeral: true);
        }

        [ComponentInteraction("select-branch")]
        public async Task MakeAdviceSelectBranchResponseAsync(string[] selections)
        {
            var guilds = Context.User.MutualGuilds.FirstOrDefault()!;
            var userRoles = guilds
                .Users.FirstOrDefault(x => x.Id == Context.User.Id)!.Roles.ToList();

            if (await _roleValidator.Validate(userRoles))
            {
                await RespondAsync("Нет доступа!", ephemeral: true);
                return;
            }

            Post? post = await _context.Posts
                .OrderByDescending(x => x.LastUpdateAt)
                .FirstOrDefaultAsync(x => x.DiscordId == Context.User.Id && x.GuildId == guilds.Id);

            post!.BranchId = selections.First();

            await _context.SaveChangesAsync();

            List<Server> servers = await _context.Servers.Where(x => x.GuildId == guilds.Id).ToListAsync();
            if (await IsCollectionIsEmpty(servers)) return;

            SelectMenuBuilder selectBranch = new SelectMenuBuilder()
            {
                CustomId = "select-server",
                Placeholder = "Выберите сервер"
            };

            foreach (Server server in servers) selectBranch.AddOption(server.Name, server.Id.ToString());

            ComponentBuilder components = new ComponentBuilder();
            components.WithSelectMenu(selectBranch);

            await RespondAsync($"Укажите ваш сервер: ", components: components.Build(), ephemeral: true);
        }

        [ComponentInteraction("select-server")]
        public async Task MakeAdviceSelectServerResponseAsync(string[] selections)
        {
            var guilds = Context.User.MutualGuilds.FirstOrDefault()!;
            var userRoles = guilds
                .Users.FirstOrDefault(x => x.Id == Context.User.Id)!.Roles.ToList();

            if (await _roleValidator.Validate(userRoles))
            {
                await RespondAsync("Нет доступа!", ephemeral: true);
                return;
            }

            Post? post = await _context.Posts
                .OrderByDescending(x => x.LastUpdateAt)
                .FirstOrDefaultAsync(x => x.DiscordId == Context.User.Id && x.GuildId == guilds.Id);

            post!.ServerId = selections.First();

            await _context.SaveChangesAsync();
            ButtonBuilder acceptButton = new ButtonBuilder()
            {
                CustomId = "accept-photo",
                Label = "Подтвердить отправку",
                Style = ButtonStyle.Success
            };
            ComponentBuilder components = new ComponentBuilder();
            components.WithButton(acceptButton);

            var cxt = await Context.Client.GetChannelAsync(Context.Channel.Id);

            await RespondAsync($"Отправьте изображение вашего лута (не больше 1шт): ", components: components.Build(), ephemeral: true);
        }

        [ComponentInteraction("accept-photo")]
        public async Task MakeAdviceLoadImageResponseAsync()
        {
            var guilds = Context.User.MutualGuilds.FirstOrDefault()!;
            var userRoles = guilds
                .Users.FirstOrDefault(x => x.Id == Context.User.Id)!.Roles.ToList();

            if (await _roleValidator.Validate(userRoles))
            {
                await RespondAsync("Нет доступа!", ephemeral: true);
                return;
            }

            Post? post = await _context.Posts
                .OrderByDescending(x => x.LastUpdateAt)
                .FirstOrDefaultAsync(x => x.DiscordId == Context.User.Id && x.GuildId == guilds.Id);

            var messages = await Context.Channel.GetMessagesAsync().FlattenAsync();
            var message = messages.First(x => x.Author.Id == Context.User.Id && x.Attachments is not null);
            var image = message.Attachments.First();

            if (image is null)
            {
                await RespondAsync("Ошибка! Вы обязательно должны загрузить фото!", ephemeral: true);
                return;
            }

            post!.ImageUri = image.Url;
            post!.LastUpdateAt = DateTime.UtcNow;
            post!.IsActive = true;

            await _context.SaveChangesAsync();

            await Context.User.SendMessageAsync(text: $"Вы добавили обьявление на наш сервер. **Превью**",
                embed: await GetPreviewEmbed(post));
            await RespondAsync($"Обьявление успешно добавлено! ", ephemeral: true);
            await ShareCreatedPost(post);
        }


        [SlashCommand("удалить-объявление", "Позволяет пользователю удалить объявление")]
        public async Task RemoveAdvertAsync()
        {
            var guilds = Context.User.MutualGuilds.FirstOrDefault()!;
            _context.Posts.RemoveRange(await _context.Posts.Where(x => x.DiscordId == Context.User.Id && x.GuildId == guilds.Id).ToArrayAsync());
            await _context.SaveChangesAsync();

            await RespondAsync($"Объявление успешно удалено! ", ephemeral: true);
        }

        private async Task<bool> IsCollectionIsEmpty<T>(List<T> collection)
        {
            var guilds = Context.User.MutualGuilds.FirstOrDefault()!;
            if (!IsCollectionExist(collection))
            {
                await RespondAsync($"Доступные сервера и ветви еще не были добавлены, за помощью обращайтесь в поддержку", ephemeral: true);
                await _postService.RemoveAllByUserId(Context.User.Id, guilds.Id);
                return true;
            }

            return false;
        }

        private bool IsCollectionExist<T>(List<T> collection)
        {
            return collection.Count > 0;
        }

        private async Task<Embed> GetPreviewEmbed(Post post)
        {
            var guilds = Context.User.MutualGuilds.FirstOrDefault()!;
            SocketUser user = Context.Client.GetUser(post.DiscordId);
            Server? server = await _context.Servers.Where(x => x.GuildId == guilds.Id)
                .FirstOrDefaultAsync(x => x.Id == Guid.Parse(post.ServerId!));
            Uri branchUri = new Uri($"https://discord.com/channels/{Context.Channel.Id}/{post.BranchId}");

            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder()
                {
                    IconUrl = user.GetAvatarUrl(),
                    Name = user.Username,
                },
                ThumbnailUrl = user.GetAvatarUrl(),
                Description = $"Ник: {post.Name}\nСервер: {server!.Name}\n\n" +
                            $"Объявление торговца:\n {post.Description}\n\nАктивировать услугу: \n{branchUri}" +
                            $"\n\nНаписать торговцу:\n {user.Mention}\n" +
                            $"Тег текстом:\n {user.Username}"
            };

            embed
                .WithImageUrl(post.ImageUri);

            return embed.Build();
        }

        private async Task<Embed> GetEmbed(Post post)
        {
            var guilds = Context.User.MutualGuilds.FirstOrDefault()!;
            Guid serverId;
            Server? server = await _context!.Servers.Where(x => x.GuildId == guilds.Id).FirstOrDefaultAsync();
            if (Guid.TryParse(post.ServerId!, out serverId))
                server = await _context!.Servers.Where(x => x.GuildId == guilds.Id).FirstOrDefaultAsync(x => x.Id == serverId);
            string autoposterId = _configuration!["DiscordBot:WikiAutoPosterId"] ?? post.BranchId!;
            Uri branchUri = new Uri($"https://discord.com/channels/{_configuration!["DiscordBot:GuildsId"]}/" + autoposterId);

            Context.Client!.Guilds.FirstOrDefault(x => x.Id == post.DiscordId);
            var user = await Context.Client.GetUserAsync(post.DiscordId);

            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder()
                {
                    IconUrl = user.GetAvatarUrl(),
                    Name = user.Username,
                },
                ThumbnailUrl = user.GetAvatarUrl(),
                Color = 0x0099FF,
                Description = $"Ник: {post.Name}\nСервер: {server!.Name}\n\n" +
                            $"Объявление торговца:\n {post.Description}\n\nАктивировать услугу: \n{branchUri}" +
                            $"\n\nНаписать торговцу:\n {user.Mention}\n" +
                            $"Тег текстом:\n {user.Username}"
            };

            embed
                .WithImageUrl(post.ImageUri);

            return embed.Build();
        }

        private async Task ShareCreatedPost(Post post)
        {
            var guilds = Context.User.MutualGuilds.FirstOrDefault()!;
            Embed embed = await GetEmbed(post);
            if (post.BranchId is null) return;
            var channel = Context.Client.GetChannel(ulong.Parse(post.BranchId!)) as IMessageChannel;

            if (channel is null) return;

            await channel!.SendMessageAsync(embed: embed);

            Post? oldPost = await _context!.Posts.FirstOrDefaultAsync(x => x.Id == post.Id && x.GuildId == guilds.Id);
            post.LastUpdateAt = DateTime.UtcNow;

            _context.Entry(oldPost!).CurrentValues.SetValues(post);
            await _context!.SaveChangesAsync();
        }
    }
}
