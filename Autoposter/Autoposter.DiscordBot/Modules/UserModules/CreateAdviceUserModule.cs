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
            if (await _roleValidator.Validate(((SocketGuildUser)Context.User).Roles))
            {
                await RespondAsync("Нет доступа!", ephemeral: true);
                return;
            }

            double interval = await _context.BotSettings.Select(x => x.Interval).OrderByDescending(x => x)
                .FirstOrDefaultAsync();

            if (interval == 0)
            {
                await RespondAsync("На сервере проводятся настройки интервала, попробуйте добавить обьявление позже", ephemeral: true);
                return;
            }

            double timeToCreate = await _postService.TimeToCreate(Context.User.Id, interval);

            if (timeToCreate > 0) 
            {
                await RespondAsync($"Вы не можете создать новое объявление. " +
                    $" Осталось {timeToCreate} минут", ephemeral: true);
                return;
            }

            await _postService.RemoveAllByUserId(Context.User.Id);

            await _postService.AddAsync(model, Context);

            List<Branch> branches = await _context.Branches.ToListAsync();
            if (await IsCollectionIsEmpty(branches)) return;

            SelectMenuBuilder selectBranch = new SelectMenuBuilder()
            {
                CustomId = "select-branch",
                Placeholder = "Выберите ветку"
            };

            foreach (Branch branch in branches) selectBranch.AddOption(branch.Name, branch.BranchId.ToString());

            ComponentBuilder components = new ComponentBuilder();
            components.WithSelectMenu(selectBranch);

            await RespondAsync($"Выберите ветку для постинга:", components: components.Build(), ephemeral: true);
        }

        [ComponentInteraction("select-branch")]
        public async Task MakeAdviceSelectBranchResponseAsync(string[] selections)
        {
            if (await _roleValidator.Validate(((SocketGuildUser)Context.User).Roles))
            {
                await RespondAsync("Нет доступа!", ephemeral: true);
                return;
            }

            Post? post = await _context.Posts
                .OrderByDescending(x => x.LastUpdateAt)
                .FirstOrDefaultAsync(x => x.DiscordId == Context.User.Id);

            post!.BranchId = selections.First();

            await _context.SaveChangesAsync();

            List<Server> servers = await _context.Servers.ToListAsync();
            if (await IsCollectionIsEmpty(servers)) return;

            SelectMenuBuilder selectBranch = new SelectMenuBuilder()
            {
                CustomId = "select-server",
                Placeholder = "Выберите сервер"
            };

            foreach (Server server in servers) selectBranch.AddOption(server.Name, server.Id.ToString());

            ComponentBuilder components = new ComponentBuilder();
            components.WithSelectMenu(selectBranch);

            await RespondAsync($"Выберите сервер для постинга: ", components: components.Build(), ephemeral: true);
        }

        [ComponentInteraction("select-server")]
        public async Task MakeAdviceSelectServerResponseAsync(string[] selections)
        {
            if (await _roleValidator.Validate(((SocketGuildUser)Context.User).Roles))
            {
                await RespondAsync("Нет доступа!", ephemeral: true);
                return;
            }

            Post? post = await _context.Posts
                .OrderByDescending(x => x.LastUpdateAt)
                .FirstOrDefaultAsync(x => x.DiscordId == Context.User.Id);

            post!.ServerId = selections.First();

            await _context.SaveChangesAsync();
            ButtonBuilder acceptButton = new ButtonBuilder()
            {
                CustomId = "accept-photo",
                Label = "Accept",
                Style = ButtonStyle.Success
            };
            ComponentBuilder components = new ComponentBuilder();
            components.WithButton(acceptButton);

            await RespondAsync($"Отправьте изображение-визитку вашего аккаунта: ", components: components.Build(), ephemeral: true);
        }

        [ComponentInteraction("accept-photo")]
        public async Task MakeAdviceLoadImageResponseAsync()
        {
            if (await _roleValidator.Validate(((SocketGuildUser)Context.User).Roles))
            {
                await RespondAsync("Нет доступа!", ephemeral: true);
                return;
            }

            Post? post = await _context.Posts
                .OrderByDescending(x => x.LastUpdateAt)
                .FirstOrDefaultAsync(x => x.DiscordId == Context.User.Id);

            var messages = await Context.Channel.GetMessagesAsync().FlattenAsync();
            var message = messages.First(x => x.Author.Id == Context.User.Id && x.Attachments is not null);
            var image = message.Attachments.First();


            post!.ImageUri = image.Url;
            post!.LastUpdateAt = DateTime.UtcNow;
            post!.IsActive = true;

            await _context.SaveChangesAsync();

            await Context.User.SendMessageAsync(text: $"Вы добавили обьявление на наш сервер. **Превью**",
                embed: await GetPreviewEmbed(post));
            await RespondAsync($"Обьявление успешно добавлено! ", ephemeral: true);
            await ShareCreatedPost(post);
        }

        private async Task<bool> IsCollectionIsEmpty<T>(List<T> collection)
        {
            if (!IsCollectionExist(collection))
            {
                await RespondAsync($"Доступные сервера и ветви еще не были добавлены, за помощью обращайтесь в поддержку", ephemeral: true);
                await _postService.RemoveAllByUserId(Context.User.Id);
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
            SocketGuildUser user = Context.Guild.GetUser(post.DiscordId);
            Server? server = await _context.Servers.FirstOrDefaultAsync(x => x.Id == Guid.Parse(post.ServerId!));
            Uri branchUri = new Uri($"https://discord.com/channels/{Context.Channel.Id}/{post.BranchId}");

            var embed = new EmbedBuilder
            {
                Title = $"{post.Name} предлагает свой аккаунт!",
                Author = new EmbedAuthorBuilder()
                {
                    IconUrl = user.GetAvatarUrl(),
                    Name = user.Nickname,
                    Url = "https://discord.com/users/" + post.DiscordId
                },
                ThumbnailUrl = user.GetAvatarUrl(),
                Url = "https://discord.com/users/" + post.DiscordId,
                Description = $"Ник: {post.Name}\nСервер: {server!.Name}\n\n" +
                            $"Объявление торговца:\n {post.Description}\n\nАктивировать услугу: \n{branchUri}"
            };

            embed
                .WithImageUrl(post.ImageUri);

            return embed.Build();
        }

        private async Task<Embed> GetEmbed(Post post)
        {
            Guid serverId;
            Server? server = await _context!.Servers.FirstOrDefaultAsync();
            if (Guid.TryParse(post.ServerId!, out serverId))
                server = await _context!.Servers.FirstOrDefaultAsync(x => x.Id == serverId);
            string autoposterId = _configuration!["DiscordBot:WikiAutoPosterId"] ?? post.BranchId!;
            Uri branchUri = new Uri($"https://discord.com/channels/{_configuration!["DiscordBot:GuildsId"]}/" + autoposterId);

            Context.Client!.Guilds.FirstOrDefault(x => x.Id == post.DiscordId);
            var user = await Context.Client.GetUserAsync(post.DiscordId);

            var embed = new EmbedBuilder
            {
                //Title = $"{post.Name} предлагает свой аккаунт!",
                Author = new EmbedAuthorBuilder()
                {
                    IconUrl = user.GetAvatarUrl(),
                    Name = user.Username,
                    Url = "https://discord.com/users/" + post.DiscordId
                },
                ThumbnailUrl = user.GetAvatarUrl(),
                Url = "https://discord.com/users/" + post.DiscordId,
                Color = 0x0099FF,
                Description = $"Ник: {post.Name}\nСервер: {server!.Name}\n\n" +
                            $"Объявление торговца:\n {post.Description}\n\nАктивировать услугу: \n{branchUri}"
            };

            embed
                .WithImageUrl(post.ImageUri);

            return embed.Build();
        }

        private async Task ShareCreatedPost(Post post)
        {
            Embed embed = await GetEmbed(post);
            if (post.BranchId is null) return;
            var channel = Context.Client.GetChannel(ulong.Parse(post.BranchId!)) as IMessageChannel;

            if (channel is null) return;

            ButtonBuilder acceptButton = new ButtonBuilder()
            {
                Label = "Написать сообщение",
                Style = ButtonStyle.Link,
                Url = "discord://-/users/" + post.DiscordId
            };

            ComponentBuilder components = new ComponentBuilder();
            components.WithButton(acceptButton);

            await channel!.SendMessageAsync(embed: embed, components: components.Build());

            Post? oldPost = await _context!.Posts.FirstOrDefaultAsync(x => x.Id == post.Id);
            post.LastUpdateAt = DateTime.UtcNow;

            _context.Entry(oldPost!).CurrentValues.SetValues(post);
            await _context!.SaveChangesAsync();
        }
    }
}
