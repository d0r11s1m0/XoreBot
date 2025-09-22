using DiscordBot.config;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using DSharpPlus.SlashCommands;

namespace DiscordBot.commands;

public class CourtCommands : ApplicationCommandModule
{
    private static int _timeToCreateNewCourtChannelInMinutes;
    
    [SlashCommand("create_court", "Создает судебное заседание(отдельный канал)")]
    public async Task CreateCourt(InteractionContext ctx)
    {
        var jsonReader = new JSONReader();
        await jsonReader.ReadJson();
        _timeToCreateNewCourtChannelInMinutes = jsonReader.timeToCreateNewCourtChannelInMinutes;

        var guild = ctx.Guild;
        var channelName = $"Судебное заседание, от {ctx.User.Username}";
        await guild.CreateChannelAsync(channelName, DiscordChannelType.Text);

        var responseCreateCourt = new DiscordEmbedBuilder()
        {
            Title = ".bot/CreateNewCourt | .bot/СозданиеСудебногоЗаседания",
            Color = DiscordColor.DarkGreen,
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"Hello, {ctx.User.Username}! | Привет, {ctx.User.Username}!",
                IconUrl = ctx.User.AvatarUrl
            }
        };

        responseCreateCourt.AddField("✅ Судебное заседание создано", $"Создан канал: {channelName}", true);

        var responseSuccess = new DiscordInteractionResponseBuilder()
            .AddEmbed(responseCreateCourt);

        await ctx.CreateResponseAsync(responseSuccess);
    }
}
