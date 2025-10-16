using DiscordBot.Config;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DiscordBot.services;

public class TicketService
{
    private static AppConfig _config = null!;
    
    /// <summary>
    /// Обновляет сообщение о доступных тикетах при запуске бота, в специально объявленном для этого канале.
    /// </summary>
    /// <param name="channelSupportTicketCreate">ID канала в котором отображается информацию о тикетах.</param>
    /// <param name="client">DiscordClient, например из Program.Client.</param>
    /// <returns></returns>
    public async Task SupportTicketUpdate(ulong channelSupportTicketCreate, DiscordClient client)
    {
        var jsonReader = new JsonReader();
        _config = await jsonReader.ReadJsonAsync();
        
        var channel = await client.GetChannelAsync(channelSupportTicketCreate);

        var messages = new List<DiscordMessage>();
        await foreach (var message in channel.GetMessagesAsync(100))
        {
            messages.Add(message);
        }

        var botMessages = messages.Where(m => m.Author?.Id == client.CurrentUser?.Id).ToList();
        if (botMessages.Any())
        {
            await channel.DeleteMessagesAsync(botMessages);
            client.Logger.LogInformation("Удалено {Count} сообщений бота из канала {ChannelName}", botMessages.Count,
                channel.Name);
        }

        var options = new List<DiscordSelectComponentOption>
        {
                new("🎮 Игровая поддержка", "🎮 Игровая поддержка", "Помощь с техническими проблемами в игре"),
                new("⚠️ Жалоба на игрока", "⚠️ Жалоба на игрока", "Нарушение игровых правил игроком"),
                new("🚨 Жалоба на администрацию", "🚨 Жалоба на администрацию", "Жалоба на команду проекта"),
                new("💬 Жалоба/обжалование DISCORD", "💬 Жалоба, обжалование DISCORD", "Вопросы по наказаниям Discord/Жалобы"),
                new("⚖️ Обжалование наказания", "⚖️ Обжалование наказания", "Обжалование игрового наказания"),
                new("🔒 Приватное обращение", "🔒 Приватное обращение", "Конфиденциальный вопрос к руководству"),
                new("💙 Заявка в команду", "💙 Заявка в команду", "Подать заявку на волонтера")
        };
        
        var ticketsMenu = new DiscordSelectComponent("support_ticket_menu", "Выберите тип тикета", options);
        
        var createSupportTicket = new DiscordEmbedBuilder()
        {
            Title = "🎫 Create support ticket | Создать тикет поддержки",
            Description =
                "📝 Чтобы создать тикет, откройте контекстное меню и выберите нужную опцию, после чего следуйте инструкциям бота.\n\n💬 To create a ticket, open the context menu and select the desired option, then follow the bot's instructions.",
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

        createSupportTicket.AddField("🎮 Игровая поддержка",
            "Нужна помощь по игре? Игра не запускается или имеются другие технические проблемы? Смело обращайтесь\n-# ⚠️ Результат НЕ гарантирован",
            true);
        createSupportTicket.AddField("⚠️ Жалоба на игрока", "Жалоба на нарушение **игровых** правил игроком проекта.",
            true);
        createSupportTicket.AddField("🚨 Жалоба на члена команды проекта",
            "Жалоба на нарушение игровых правил/правил администрации членом команды проекта (Викирайтеры, админы и т.д.).",
            true);
        createSupportTicket.AddField("💬 Жалоба на пользователя Discord / Обжалования Discord-наказаний",
            "Жалоба на нарушения правил Discord пользователем.", true);
        createSupportTicket.AddField("⚖️ Обжалование игровых наказаний",
            "Если вы считаете, что наказание выдано неверно — обращайтесь сюда.\n🚫 Помните: обман администрации приведет к блокировке.",
            true);
        createSupportTicket.AddField("🔒 Приватное обращение к руководству проекта",
            "В случае, если у вас есть серьезный вопрос — обращайтесь. Обращение получит **только** руководство проекта.",
            true);
        createSupportTicket.AddField("💙 Заявка на волонтёра",
            "Если желаете стать волонтером проекта (администрация, модерация и т.д.) — подавайте заявку сюда.", true);

        var messageBuilder = new DiscordMessageBuilder()
            .AddEmbed(createSupportTicket)
            .AddActionRowComponent(ticketsMenu);

        await channel.SendMessageAsync(messageBuilder);
    }
}