using Autoposter.BotDiscord.Attributes;
using Autoposter.BotDiscord.Models;
using Autoposter.BotDiscord.Services;
using Autoposter.BusinessLayer.Contracts;
using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.BusinessLayer.Models;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;

namespace Autoposter.BotDiscord.Modules.UserModules
{
    public class CreateAdvertModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private InteractionHandler _handler;
        private IPostService _postService;
        private IBranchService _branchService;
        private IBotSettingsService _botSettingsService;
        private IServerService _serverService;
        private AppDbContext _context;
        private ILogger<CreateAdvertModule> _logger;

        public CreateAdvertModule(InteractionHandler handler, AppDbContext context,
            IPostService postService, 
            IBranchService branchService,
            IBotSettingsService botSettingsService,
            IServerService serverService,
            ILogger<CreateAdvertModule> logger)
        {
            _context = context;
            _handler = handler;
            _postService = postService;
            _branchService = branchService;
            _botSettingsService = botSettingsService;
            _serverService = serverService;
            _logger = logger;
        }

        [RequireBotRoles]
        [SlashCommand("создать-объявление", "Позволяет пользователю создать объявление, которое будет автоматически поститься")]
        public async Task MakeAdvertModalAsync()
            => await Context.Interaction.RespondWithModalAsync<CreateAdvertModel>("advert_model");

        [RequireBotRoles]
        [ModalInteraction("advert_model")]
        public async Task HandleAdvertModalAsync(CreateAdvertModel model)
        {
            double interval = await GetIntervalAsync();
            if (interval == 0) return;

            if (!await IsConvenientTime(model, interval)) return;

            var userRoles = Context.User.MutualGuilds.FirstOrDefault()!.Users
                .FirstOrDefault(x => x.Id == Context.User.Id)!.Roles.Select(x => x.Id).ToList();
            IEnumerable<Branch> branches = await _branchService.GetAvailableBranchesAsync(userRoles, Context.User.MutualGuilds.FirstOrDefault()!.Id);

            if (await IsCollectionEmpty(branches)) return;

            ComponentBuilder components = new ComponentBuilder().WithSelectMenu(SelectBuilder.CreateBranchesMenu(branches));
            _logger.LogInformation($"(post-creating)The user start creating a post. (user_id: {Context.User.Id}, post_name: {model.Nickname})");
            await RespondAsync($"Выберите канал для постинга:", components: components.Build(), ephemeral: true);
        }

        [RequireBotRoles]
        [ComponentInteraction("select-branch")]
        public async Task HandleSelectBranchAsync(string[] selections)
        {
            Post? post = await _postService.GetLastByUserAsync(Context.User.Id.ToString(), Context.User.MutualGuilds.FirstOrDefault()!.Id);
            post!.BranchId = selections.First();
            await _postService.UpdateAsync(post, Context.User.MutualGuilds.FirstOrDefault()!.Id);

            IEnumerable<Server> servers = await _serverService.GetServers(Context.User.MutualGuilds.FirstOrDefault()!.Id);
            if (await IsCollectionEmpty(servers)) return;

            ComponentBuilder components = new ComponentBuilder().WithSelectMenu(SelectBuilder.CreateServersMenu(servers));
            _logger.LogInformation($"(post-creating) The user select a branch. (user_id: {Context.User.Id}, selected-branch: " +
                $"{selections.First()}, post_name: {post.Name})");
            await RespondAsync($"Укажите ваш сервер: ", components: components.Build(), ephemeral: true);
        }

        [RequireBotRoles]
        [ComponentInteraction("select-server")]
        public async Task HandleSelectServerAsync(string[] selections)
        {
            Post? post = await _postService.GetLastByUserAsync(Context.User.Id.ToString(), Context.User.MutualGuilds.FirstOrDefault()!.Id);
            post!.ServerId = selections.First();
            await _postService.UpdateAsync(post, Context.User.MutualGuilds.FirstOrDefault()!.Id);

            ButtonBuilder acceptButton = new ButtonBuilder()
            {
                CustomId = "accept-photo",
                Label = "Подтвердить отправку",
                Style = ButtonStyle.Success
            };
            ComponentBuilder components = new ComponentBuilder().WithButton(acceptButton);
            _logger.LogInformation($"(post-creating) The user select a server. (user_id: {Context.User.Id}, selected-server: " +
                $"{selections.First()}, post_name: {post.Name})");
            await RespondAsync($"Отправьте изображение вашего лута (не больше 1шт): ", components: components.Build(), ephemeral: true);
        }

        [RequireBotRoles]
        [ComponentInteraction("accept-photo")]
        public async Task HandleLoadImageAsync()
        {
            Post? post = await _postService.GetLastByUserAsync(Context.User.Id.ToString(), Context.User.MutualGuilds.FirstOrDefault()!.Id);

            var messages = await Context.Channel.GetMessagesAsync().FlattenAsync();
            var message = messages.First(x => x.Author.Id == Context.User.Id && x.Attachments is not null);
            var image = message.Attachments.First();

            if (image is null)
            {
                await RespondAsync("Ошибка! Вы обязательно должны загрузить фото!", ephemeral: true);
                return;
            }

            post!.EndingCreate(image.Url);
            await _postService.UpdateAsync(post, Context.User.MutualGuilds.FirstOrDefault()!.Id);

            var servers = await _serverService.GetServers(Context.User.MutualGuilds.FirstOrDefault()!.Id);
            Server? server = servers.FirstOrDefault(x => x.Id == Guid.Parse(post!.ServerId!));

            Embed embed = EmbedFactory.GetGeneralEmbed(new EmbedModel { Post = post, User = Context.User, Server = server!, GuildId = Context.User.MutualGuilds.FirstOrDefault()!.Id });
            await Context.User.SendMessageAsync(text: $"Вы добавили обьявление на наш сервер. **Превью**", embed: embed);
            await RespondAsync($"Обьявление успешно добавлено! ", ephemeral: true);
            _logger.LogInformation($"(post-create) The user create the post. (user_id: {Context.User.Id}, post_name: {post.Name})");
            await ShareCreatedPost(post);
        }
        private async Task<double> GetIntervalAsync()
        {
            double interval = await _botSettingsService.GetIntervalAsync(Context.User.MutualGuilds.FirstOrDefault()!.Id);

            if (interval == 0)
            {
                await RespondAsync("На сервере проводятся настройки интервала, попробуйте добавить обьявление позже", ephemeral: true);
            }

            return interval;
        }

        private async Task<bool> IsConvenientTime(CreateAdvertModel model, double interval)
        {
            Post post = await _postService.GetLastByUserAsync(Context.User.Id.ToString(), Context.User.MutualGuilds.FirstOrDefault()!.Id);
            double timeToCreate = await _postService.TimeToCreate(Context.User.Id, interval, Context.User.MutualGuilds.FirstOrDefault()!.Id);

            if (timeToCreate > 0 && post.IsAvailableToPost())
            {
                await RespondAsync($"Вы не можете создать новое объявление. " +
                    $" Осталось {timeToCreate} минут", ephemeral: true);
                return false;
            }

            await _postService.RemoveAllByUserId(Context.User.Id, Context.User.MutualGuilds.FirstOrDefault()!.Id);
            await _postService.AddAsync(model, Context, Context.User.MutualGuilds.FirstOrDefault()!.Id);

            return true;
        }


        private async Task ShareCreatedPost(Post post)
        {
            var servers = await _serverService.GetServers(Context.User.MutualGuilds.FirstOrDefault()!.Id);
            Server? server = servers.FirstOrDefault(x => x.Id == Guid.Parse(post!.ServerId!));

            Embed embed = EmbedFactory.GetGeneralEmbed(
                    new EmbedModel { Post = post, User = Context.User, Server = server!, GuildId = Context.User.MutualGuilds.FirstOrDefault()!.Id });

            if (post.BranchId is null) return;
            var channel = Context.Client.GetChannel(ulong.Parse(post.BranchId!)) as IMessageChannel;

            if (channel is null) return;

            await channel!.SendMessageAsync(embed: embed);

            post.LastUpdateAt = DateTime.UtcNow;
            await _postService.UpdateAsync(post, Context.User.MutualGuilds.FirstOrDefault()!.Id);
        }

        private async Task<bool> IsCollectionEmpty<T>(IEnumerable<T> collection)
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

        private bool IsCollectionExist<T>(IEnumerable<T> collection)
        {
            return collection.Count() > 0;
        }
    }
}
