using DiscordBot.Config;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DiscordBot.services;

public class StaffListService
{
    private static AppConfig _config = null!;
    
    /// <summary>
    /// Запускает первоначальное обновление и периодический таймер.
    /// Вызывается один раз при старте приложения.
    /// </summary>
    public void StartPeriodicUpdate()
    {
        var Client = Program.Client;
        _config.UpdateTimer = new Timer(async _ =>
        {
            var channel = await Client.GetChannelAsync(_config.ChannelTeamRoster);
            await ProjectLeadStaffUpdate(_config.ChannelTeamRoster, Client);
            Client.Logger.LogInformation("Произведен update команды проекта, причина: периодическое обновление. Канал: {ChannelName}", channel.Name);
        }, null, TimeSpan.FromMinutes(_config.TimeToUpdateStaffListInMinutes), TimeSpan.FromMinutes(_config.TimeToUpdateStaffListInMinutes));
    }
    
    /// <summary>
    /// Обновляет информацию про состав команды проекта, в специально объявленном для этого канале.
    /// </summary>
    /// <param name="channelTeamRoster">ID канала в котором отображается информацию о команде проекта.</param>
    /// <param name="client">DiscordClient, например из Program.Client.</param>
    /// <returns></returns>
    public async Task ProjectLeadStaffUpdate(ulong channelTeamRoster, DiscordClient client)
    {   
        var jsonReader = new JsonReader();
        _config = await jsonReader.ReadJsonAsync();
        
        var channel = await client.GetChannelAsync(channelTeamRoster);
        var guild = channel.Guild;

        var messages = new List<DiscordMessage>();
        await foreach (var message in channel.GetMessagesAsync(100))
        {
            messages.Add(message);
        }
        var botMessages = messages.Where(m => m.Author?.Id == client.CurrentUser?.Id).ToList();
        if (botMessages.Any())
        {
            await channel.DeleteMessagesAsync(botMessages);
            client.Logger.LogInformation("Удалено {Count} сообщений бота из канала {ChannelName}", botMessages.Count, channel.Name);
        }
            
        DiscordRole? projectLeadRole = guild.Roles.Values.FirstOrDefault(r => r.Id == _config.ProjectLeadRoleId);

        var staffUpdateMessage = new DiscordEmbedBuilder()
        {
            Title = "Project leads list | Руководство проекта",
            Color = new DiscordColor("#4169E1"),
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = "https://i.ibb.co/B2Hbz9t0/ccc838a0eb13959932053779759d7893-1.webp"
            },
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"Данные предоставлены {client.CurrentUser.Username} <3",
                IconUrl = client.CurrentUser.AvatarUrl
            }
        };

        string projectLeadMentions = ".";

        if (projectLeadRole is not null)
        {
            var projectLeadWithRole = guild.Members.Values
                .Where(member => member.Roles.Contains(projectLeadRole))
                .ToList();

            if (projectLeadWithRole.Any())
            {
                projectLeadMentions = string.Join(" ", projectLeadWithRole.Select(member => member.Mention));
            }
        }

        staffUpdateMessage.AddField("Project Leads | Руководители проекта", projectLeadMentions, true);
            
        await channel.SendMessageAsync(embed: staffUpdateMessage);
    }
}