using DSharpPlus.SlashCommands;
using DiscordBot.config;
using Microsoft.Extensions.Logging;
using DSharpPlus.Entities;

namespace DiscordBot.commands;

public class HelpCommands : ApplicationCommandModule
{
    private static ulong _leadStaffRoleID;
    
    static HelpCommands()
    {
        var jsonReader = new JSONReader();
        jsonReader.ReadJson();
        _leadStaffRoleID = jsonReader.leadStaffRoleID;
    }
    
    [SlashCommand("help", "Получить помощь по боту")]
    public async Task Help(InteractionContext ctx)
    {
        ctx.Client.Logger.LogInformation("Команда help запрошена пользователем {User}", ctx.User.Username);
        
        var message = new DiscordEmbedBuilder()
        {
            Title = ".bot/Help | .bot/Помощь",
            Color = DiscordColor.LightGray,
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"Hello, {ctx.User.Username}! | Привет, {ctx.User.Username}!",
                IconUrl = ctx.User.AvatarUrl
            }
        };
        
        message.AddField("Доступные команды:", "**/help** - вывести это окно", true);
        
        if (ctx.Member.Roles.Any(r => r.Id == _leadStaffRoleID))
            message.AddField("Дополнительные команды для ваших ролей:", "**/stafflist_update** - обновляет список команды проекта", true);
        
        var response = new DiscordInteractionResponseBuilder()
            .AddEmbed(message);
        
        ctx.Client.Logger.LogInformation("Команда help успешно выполнена");
        await ctx.CreateResponseAsync(response);
    }
}