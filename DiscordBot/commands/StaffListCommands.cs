using DiscordBot.config;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;

namespace DiscordBot.commands;

public class StaffListCommands : ApplicationCommandModule
{
    private static ulong _channelTeamRoster;
    private static ulong _leadStaffRoleID;
    
    static StaffListCommands()
    {
        var jsonReader = new JSONReader();
        jsonReader.ReadJson();
        _channelTeamRoster = jsonReader.channelTeamRoster;
        _leadStaffRoleID = jsonReader.leadStaffRoleID;
    }
    
    [SlashCommand("stafflist_update", "Досрочно обновляет список команды проекта")]
    public async Task StaffListUpdate(InteractionContext ctx)
    {
        
        if (!ctx.Member.Roles.Any(r => r.Id == _leadStaffRoleID))
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
        
        var channel = await ctx.Client.GetChannelAsync(_channelTeamRoster);

        await Program.ProjectLeadStaffUpdate();
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