using DiscordBot.Config;
using DiscordBot.services;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using DSharpPlus.SlashCommands;

namespace DiscordBot.commands;

public class StaffListCommands : ApplicationCommandModule
{
    private static AppConfig _config = null!;
    
    static async Task StaffListCommandsFields()
    {
        var jsonReader = new JsonReader();
        _config = await jsonReader.ReadJsonAsync();
        
    }
    
    [SlashCommand("stafflist_update", "Досрочно обновляет список команды проекта")]
    public async Task StaffListUpdate(InteractionContext ctx)
    {
        var Client = Program.Client;

        if (!ctx.Member.Roles.Any(r => r.Id == _config.LeadStaffRoleId))
        {
            ctx.Client.Logger.LogError($"❌ Отказано к доступу к команде, причина: отсутствует нужная роль. Запрашивающий: {ctx.User.Username}");
            var responseEmbedNo = new DiscordEmbedBuilder()
            {
                Title = ".bot/StaffListUpdate | .bot/ОбновлениеСпискаКомандыПроекта",
                Color = DiscordColor.DarkRed,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Hello, {ctx.User.Username}! | Привет, {ctx.User.Username}!",
                    IconUrl = ctx.User.AvatarUrl
                }
            };

            responseEmbedNo.AddField("В доступе отказано ❌", "Вы не обладает нужной ролью для доступа к этой команде", true);
        
            var responseNo = new DiscordInteractionResponseBuilder()
                .AddEmbed(responseEmbedNo);

            await ctx.CreateResponseAsync(responseNo);
            return;
        }
        
        var channel = await ctx.Client.GetChannelAsync(_config.ChannelTeamRoster);
        var staffListService = new StaffListService();

        await staffListService.ProjectLeadStaffUpdate(_config.ChannelTeamRoster, Client);
        var responseEmbed = new DiscordEmbedBuilder()
        {
            Title = ".bot/StaffListUpdate | .bot/ОбновлениеСпискаКомандыПроекта",
            Color = DiscordColor.DarkGreen,
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"Hello, {ctx.User.Username}! | Привет, {ctx.User.Username}!",
                IconUrl = ctx.User.AvatarUrl
            }
        };

        responseEmbed.AddField("Успешно!", "Успешно обновили список команды проекта!", true);
        
        var response = new DiscordInteractionResponseBuilder()
            .AddEmbed(responseEmbed);

        await ctx.CreateResponseAsync(response);
        ctx.Client.Logger.LogInformation($"Произведен update команды проекта, причина: команда. Канал: {{ChannelName}}. Запрашивающий: {ctx.User.Username}", channel.Name);
    }
}