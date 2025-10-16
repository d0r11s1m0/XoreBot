using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DiscordBot.events;

public class CloseTicketButtonHandler : IEventHandler<ComponentInteractionCreatedEventArgs>
{
    public async Task HandleEventAsync(DiscordClient client, ComponentInteractionCreatedEventArgs e)
    {
        if (e.Id == "close_ticket")
        {
            await e.Interaction.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .WithContent("🔒 Тикет закрывается...")
                    .AsEphemeral(true));

            await Task.Delay(3000);
            await e.Channel.DeleteAsync("Закрыт по кнопке");
        }
    }
}