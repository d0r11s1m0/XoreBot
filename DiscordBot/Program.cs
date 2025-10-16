using DiscordBot.Config;
using DiscordBot.events;
using DiscordBot.services;
using DSharpPlus;
using DSharpPlus.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DiscordBot
{
    internal class Program
    {
        public static DiscordClient Client { get; set; } = null!;
        private static AppConfig _config = null!;
        
        public static async Task Main(string[] args)
        {
            var jsonReader = new JsonReader();
            _config = await jsonReader.ReadJsonAsync();

            DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(_config.Token, DiscordIntents.All);
            
            builder.UseCommands((_, commands) =>
            {
                commands.AddCommands<commands.StaffListCommands>();
                commands.AddCommands<commands.HelpCommands>();
            });

            builder.ConfigureEventHandlers(events =>
            {
                events.AddEventHandlers<TicketsEventHandler.TicketSelectionHandler>(ServiceLifetime.Singleton);
                events.AddEventHandlers<CloseTicketButtonHandler>(ServiceLifetime.Singleton);
            });
            
            Client = builder.Build();
            
            await Client.ConnectAsync();
            await ClientOnReady(Client);
            await Task.Delay(-1);
        }
        
        private static async Task ClientOnReady(DiscordClient sender)
        {
            var ticketService = new TicketService();
            var staffListService = new StaffListService();
            var channel = await sender.GetChannelAsync(_config.ChannelTeamRoster);

            await staffListService.ProjectLeadStaffUpdate(_config.ChannelTeamRoster, Client);
            Client.Logger.LogInformation("Произведен update команды проекта, причина: включение бота. Канал: {ChannelName}", channel.Name);

            staffListService.StartPeriodicUpdate();
            await ticketService.SupportTicketUpdate(_config.ChannelSupportTicketCreate, Client);
        }
    }
}
