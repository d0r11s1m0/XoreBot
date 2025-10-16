using DiscordBot.Config;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DiscordBot.events;

public class TicketsEventHandler
{
    public class TicketSelectionHandler : IEventHandler<ComponentInteractionCreatedEventArgs>
    {
        private AppConfig _config = null!;
        private DiscordEmbedBuilder _responseEmbed = null!;

        public async Task HandleEventAsync(DiscordClient client, ComponentInteractionCreatedEventArgs eventArgs)
        {
            string? selectedValue = eventArgs.Values.FirstOrDefault();
            var jsonReader = new JsonReader();
            _config = await jsonReader.ReadJsonAsync();

            if (eventArgs.Id != "support_ticket_menu")
                return;

            _responseEmbed = new DiscordEmbedBuilder()
            {
                Title = "🎫 Обращение создано",
                Color = new DiscordColor("#4169E1"),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = "https://i.ibb.co/B2Hbz9t0/ccc838a0eb13959932053779759d7893-1.webp"
                },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"💫 Данные предоставлены {client.CurrentUser.Username} <3",
                    IconUrl = client.CurrentUser.AvatarUrl
                }
            };

            if (TicketMappings.TryGetValue(selectedValue!, out var ticketInfo))
            {
                var embed = TicketEmbedsFactory.GetEmbed(ticketInfo.Type);
                await CreateTicketChannelAsync(ticketInfo.Title, embed, eventArgs, client);
            }
            else
            {
                await eventArgs.Interaction.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("❓ Неизвестный тип тикета.")
                        .AsEphemeral(true));
                return;
            }

            var responseBuilder = new DiscordInteractionResponseBuilder()
                .AddEmbed(_responseEmbed)
                .AsEphemeral(true);

            await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, responseBuilder);
        }

        private async Task CreateTicketChannelAsync(string title, DiscordEmbed welcomeEmbed, ComponentInteractionCreatedEventArgs eventArgs, DiscordClient client)
        {
            Random rnd = new Random();
            int randomNumber = rnd.Next(1000, 9999);
            var guild = eventArgs.Guild;
            var category = await guild.GetChannelAsync(_config.TicketsCategoryId);
            string? selectedValue = eventArgs.Values.FirstOrDefault();
            var channelName = $"{selectedValue}-{randomNumber}";

            var newChannel = await guild.CreateChannelAsync(channelName, DiscordChannelType.Text, category,
                $"Тикет поддержки от {eventArgs.User.Username}");

            var ticketsResponseButtons = new DiscordButtonComponent(DiscordButtonStyle.Danger, "close_ticket", "Закрыть тикет");
            var ticketsResponseButtonsRow = new DiscordActionRowComponent(new List<DiscordComponent> { ticketsResponseButtons });

            var combinedMessage = new DiscordMessageBuilder()
                .AddEmbed(welcomeEmbed)
                .AddActionRowComponent(ticketsResponseButtonsRow);

            var link = $"https://discord.com/channels/{guild.Id}/{newChannel.Id}";
            _responseEmbed.AddField(title, $"Ссылка на тикет: {link}", true);
            await newChannel.SendMessageAsync(combinedMessage);
        }

        private static readonly Dictionary<string, (string Title, TicketType Type)> TicketMappings = new()
        {
            ["🎮 Игровая поддержка"] = ("🎮 Создаем тикет игровой поддержки...", TicketType.GameSupport),
            ["⚠️ Жалоба на игрока"] = ("⚠️ Создаем жалобу на игрока...", TicketType.PlayerReport),
            ["🚨 Жалоба на администрацию"] = ("🚨 Создаем жалобу на администрацию...", TicketType.AdminReport),
            ["💬 Жалоба, обжалование DISCORD"] = ("💬 Создаем жалобу/обжалование Discord...", TicketType.DiscordAppeal),
            ["⚖️ Обжалование игрового наказания"] = ("⚖️ Создаем апелляцию...", TicketType.Appeal),
            ["🔒 Приватное обращение"] = ("🔒 Создаем приватное обращение...", TicketType.Private),
            ["💙 Заявка в команду"] = ("💙 Создаем заявку в команду...", TicketType.Volunteer)
        };
    }
}