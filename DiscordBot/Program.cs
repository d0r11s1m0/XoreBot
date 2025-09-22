using DiscordBot.config;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DiscordBot
{
    internal class Program
    {
        public static DiscordClient Client { get; set; }
        private static ulong _channelTeamRoster;
        private static ulong _projectLeadRoleID;
        private static Timer _updateTimer;
        private static int _timeToUpdateStaffListInMinutes;
        private static string _token;
        public static ulong _channelSupportTicketCreate;
        public static ulong _ticketsCategoryID;
        
        public static async Task Main(string[] args)
        {
            var jsonReader = new JSONReader();
            await jsonReader.ReadJson();
            
            _channelTeamRoster = jsonReader.channelTeamRoster;
            _projectLeadRoleID = jsonReader.projectLeadRoleID;
            _timeToUpdateStaffListInMinutes = jsonReader.timeToUpdateStaffListInMinutes;
            _token = jsonReader.token;
            _channelSupportTicketCreate = jsonReader.channelSupportTicketCreate;
            _ticketsCategoryID = jsonReader.ticketsCategoryID;
            
            DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(_token, DiscordIntents.All);
            
            builder.UseSlashCommands(options =>
            {
                options.RegisterCommands<commands.StaffListCommands>();
                options.RegisterCommands<commands.HelpCommands>();
                //options.RegisterCommands<commands.CourtCommands>();
            });
            
            builder.ConfigureEventHandlers(b =>
                b.AddEventHandlers<TicketSelectionHandler>(ServiceLifetime.Singleton));
            
            Client = builder.Build();
            
            await Client.ConnectAsync();
            await ClientOnReady(Client);
            await Task.Delay(-1);
        }

        private static async Task ClientOnReady(DiscordClient sender)
        {
            var channel = await sender.GetChannelAsync(_channelTeamRoster);
            await ProjectLeadStaffUpdate();
            Client.Logger.LogInformation("Произведен update команды проекта, причина: включение бота. Канал: {ChannelName}", channel.Name);

            StartPeriodicUpdate();
            SupportTicketUpdate();
        }

        private static async Task SupportTicketUpdate()
        {
            var channel = await Client.GetChannelAsync(_channelSupportTicketCreate);

            var guild = channel.Guild;

            var messages = new List<DiscordMessage>();
            await foreach (var message in channel.GetMessagesAsync(100))
            {
                messages.Add(message);
            }
            var botMessages = messages.Where(m => m.Author?.Id == Client.CurrentUser?.Id).ToList();
            if (botMessages.Any())
            {
                await channel.DeleteMessagesAsync(botMessages);
                Client.Logger.LogInformation("Удалено {Count} сообщений бота из канала {ChannelName}", botMessages.Count, channel.Name);
            }
            
            var ticketsMenu = new DiscordSelectComponent("support_ticket_menu", "Выберите тип тикета", new List<DiscordSelectComponentOption>
            {
                new DiscordSelectComponentOption("🎮 Игровая поддержка", "🎮 Игровая поддержка", "Помощь с техническими проблемами в игре"),
                new DiscordSelectComponentOption("⚠️ Жалоба на игрока", "⚠️ Жалоба на игрока", "Нарушение игровых правил игроком"),
                new DiscordSelectComponentOption("🚨 Жалоба на администрацию", "🚨 Жалоба на администрацию", "Жалоба на команду проекта"),
                new DiscordSelectComponentOption("💬 Жалоба/обжалование DISCORD", "💬 Жалоба, обжалование DISCORD", "Вопросы по наказаниям Discord/Жалобы"),
                new DiscordSelectComponentOption("⚖️ Обжалование наказания", "⚖️ Обжалование наказания", "Обжалование игрового наказания"),
                new DiscordSelectComponentOption("🔒 Приватное обращение", "🔒 Приватное обращение", "Конфиденциальный вопрос к руководству"),
                new DiscordSelectComponentOption("💙 Заявка в команду", "💙 Заявка в команду", "Подать заявку на волонтера")
            });

            
            var createSupportTicket = new DiscordEmbedBuilder()
            {
                Title = "🎫 Create support ticket | Создать тикет поддержки",
                Description = "📝 Чтобы создать тикет, откройте контекстное меню и выберите нужную опцию, после чего следуйте инструкциям бота.\n\n💬 To create a ticket, open the context menu and select the desired option, then follow the bot's instructions.",
                Color = new DiscordColor("#4169E1"),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = "https://i.ibb.co/B2Hbz9t0/ccc838a0eb13959932053779759d7893-1.webp"
                },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"💫 Данные предоставлены {Client.CurrentUser.Username} <3",
                    IconUrl = Client.CurrentUser.AvatarUrl
                }
            };
            
            createSupportTicket.AddField("🎮 Игровая поддержка", "Нужна помощь по игре? Игра не запускается или имеются другие технические проблемы? Смело обращайтесь\n-# ⚠️ Результат НЕ гарантирован", true);
            createSupportTicket.AddField("⚠️ Жалоба на игрока", "Жалоба на нарушение **игровых** правил игроком проекта.", true);
            createSupportTicket.AddField("🚨 Жалоба на члена команды проекта", "Жалоба на нарушение игровых правил/правил администрации членом команды проекта (Викирайтеры, админы и т.д.).", true);
            createSupportTicket.AddField("💬 Жалоба на пользователя Discord / Обжалования Discord-наказаний", "Жалоба на нарушения правил Discord пользователем.", true);
            createSupportTicket.AddField("⚖️ Обжалование игровых наказаний", "Если вы считаете, что наказание выдано неверно — обращайтесь сюда.\n🚫 Помните: обман администрации приведет к блокировке.", true);
            createSupportTicket.AddField("🔒 Приватное обращение к руководству проекта", "В случае, если у вас есть серьезный вопрос — обращайтесь. Обращение получит **только** руководство проекта.", true);
            createSupportTicket.AddField("💙 Заявка на волонтёра", "Если желаете стать волонтером проекта (администрация, модерация и т.д.) — подавайте заявку сюда.", true);

            var messageBuilder = new DiscordMessageBuilder()
                .AddEmbed(createSupportTicket)
                .AddActionRowComponent(ticketsMenu);

            await channel.SendMessageAsync(messageBuilder);
        }

        public class TicketSelectionHandler : IEventHandler<ComponentInteractionCreatedEventArgs>
        {
            public async Task HandleEventAsync(DiscordClient client, ComponentInteractionCreatedEventArgs eventArgs)
            {
                var guild = eventArgs.Guild;
                var category = await guild.GetChannelAsync(1419731855649017876);
                
                if (eventArgs.Id != "support_ticket_menu")
                    return;
                
                string selectedValue = eventArgs.Values.FirstOrDefault();
                
                // ЗАГЛУШКА, СДЕЛАЙ НОРМАЛЬНЫЙ СЧЕТ ТИКЕТОВ
                Random rnd = new Random();
                int randomNumber = rnd.Next(1000, 9999);
                
                var channelName = $"{selectedValue}-{randomNumber}";

                var responseBuilder = new DiscordEmbedBuilder()
                {
                    Title = "🎫 Обращение создано",
                    Color = new DiscordColor("#4169E1"),
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = "https://i.ibb.co/B2Hbz9t0/ccc838a0eb13959932053779759d7893-1.webp"
                    },
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"💫 Данные предоставлены {Client.CurrentUser.Username} <3",
                        IconUrl = Client.CurrentUser.AvatarUrl
                    }
                };
                
                var inTicketMessage = new DiscordEmbedBuilder()
                {
                    Title = "Добро пожаловать в ваш тикет!",
                    Color = DiscordColor.LightGray,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Hello, {eventArgs.User.Username}! | Привет, {eventArgs.User.Username}!",
                        IconUrl = eventArgs.User.AvatarUrl
                    }
                };
                
                switch (selectedValue)
                {
                    case "🎮 Игровая поддержка":
                        var newChannel = await guild.CreateChannelAsync(
                            channelName,
                            DiscordChannelType.Text,
                            category,
                            "Тикет поддержки от " + eventArgs.User.Username);

                        inTicketMessage.AddField("🎮 Игровая поддержка", "Пожалуйста, опишите вашу проблему как можно подробнее. Чем больше информации вы предоставите, тем быстрее мы сможем вам помочь.\n\n**Пример:**\n- Описание проблемы\n- Шаги для воспроизведения\n- Любые сообщения об ошибках\n\nНаши специалисты свяжутся с вами в ближайшее время. Спасибо за ваше терпение!", false);
                        
                        string channelLink = $"https://discord.com/channels/{guild.Id}/{newChannel.Id}";
                        responseBuilder.AddField("🎮 Создаем тикет игровой поддержки...", $"Ссылка на тикет: {channelLink}", true);
                        await newChannel.SendMessageAsync(inTicketMessage);
                        break;
                    case "⚠️ Жалоба на игрока":
                        var newChannelRep = await guild.CreateChannelAsync(
                            channelName,
                            DiscordChannelType.Text,
                            category,
                            "Тикет поддержки от " + eventArgs.User.Username);

                        inTicketMessage.AddField("⚠️ Жалоба на игрока", "Пожалуйста, предоставьте как можно больше информации о нарушении. Чем больше деталей вы предоставите, тем эффективнее мы сможем рассмотреть вашу жалобу.\n\n**Форма:**\n- Игровой ник/Сикей нарушителя\n- Описание нарушения, какое правило по вашему мнению нарушено\n- Номер раунда\n- Ваш сикей\n\nМы рассмотрим вашу жалобу в ближайшее время. Спасибо за ваше сотрудничество!", false);
                        string channelLinkRep = $"https://discord.com/channels/{guild.Id}/{newChannelRep.Id}";
                        responseBuilder.AddField("⚠️ Создаем жалобу на игрока...", $"Ссылка на тикет: {channelLinkRep}", true);
                        await newChannelRep.SendMessageAsync(inTicketMessage);
                        break;
                    case "🚨 Жалоба на администрацию":
                        var newChannelSt = await guild.CreateChannelAsync(
                            channelName,
                            DiscordChannelType.Text,
                            category,
                            "Тикет поддержки от " + eventArgs.User.Username);

                        inTicketMessage.AddField("🚨 Жалоба на администрацию", "Пожалуйста, предоставьте как можно больше информации о нарушении. Чем больше деталей вы предоставите, тем эффективнее мы сможем рассмотреть вашу жалобу.\n\n**Форма:**\n- Ник/Сикей администратора\n- Описание нарушения, какое правило по вашему мнению нарушено\n- Номер раунда (если применимо)\n- Ваш сикей\n\nМы рассмотрим вашу жалобу в ближайшее время. Спасибо за ваше сотрудничество!", false);
                        string channelLinkSt = $"https://discord.com/channels/{guild.Id}/{newChannelSt.Id}";
                        responseBuilder.AddField("🚨 Создаем жалобу на администрацию...", $"Ссылка на тикет: {channelLinkSt}", true);
                        await newChannelSt.SendMessageAsync(inTicketMessage);
                        break;
                    case "💬 Жалоба, обжалование DISCORD":
                        var newChannelDi = await guild.CreateChannelAsync(
                            channelName,
                            DiscordChannelType.Text,
                            category,
                            "Тикет поддержки от " + eventArgs.User.Username);

                        inTicketMessage.AddField("💬 Жалоба на пользователя дискорда", "Пожалуйста, предоставьте как можно больше информации о нарушении. Чем больше деталей вы предоставите, тем эффективнее мы сможем рассмотреть вашу жалобу.\n\n**Форма для ЖАЛОБЫ:**\n- Дискорд ник пользователя(@пинг)\n- Описание нарушения, какое правило по вашему мнению нарушено\n\nМы рассмотрим вашу жалобу в ближайшее время. Спасибо за ваше сотрудничество!", true);
                        inTicketMessage.AddField("💬 Обжалование дискорд-наказания", "Пожалуйста, предоставьте как можно больше информации о вашем наказании. Чем больше деталей вы предоставите, тем эффективнее мы сможем рассмотреть ваше обжалование.\n\n**Форма для ОБЖАЛОВАНИЯ:**\n- Описание ситуации, по которой было выдано наказание\n- Почему вы считаете, что наказание было выдано ошибочно\n\nМы рассмотрим ваше обжалование в ближайшее время. Спасибо за ваше сотрудничество!", true);
                        string channelLinkDi = $"https://discord.com/channels/{guild.Id}/{newChannelDi.Id}";
                        responseBuilder.AddField("💬 Создаем жалобу/обжалование дискорда...", $"Ссылка на тикет: {channelLinkDi}", true);
                        await newChannelDi.SendMessageAsync(inTicketMessage);
                        break;
                    case "⚖️ Обжалование наказания":
                        var newChannelAp = await guild.CreateChannelAsync(
                            channelName,
                            DiscordChannelType.Text,
                            category,
                            "Тикет поддержки от " + eventArgs.User.Username);

                        inTicketMessage.AddField("⚖️ Обжалование наказания", "Пожалуйста, предоставьте как можно больше информации о вашем наказании. Чем больше деталей вы предоставите, тем эффективнее мы сможем рассмотреть ваше обжалование.\n\n**Форма:**\n- Ваш сикей\n- Описание ситуации, по которой было выдано наказание\n- Почему вы считаете, что наказание было выдано ошибочно\n- Номер раунда, в котором было выдано наказание(в случае, если наказание выдано за нарушение в раунде)\n -Администратор, наказавший вас(если знаете)\n\nМы рассмотрим ваше обжалование в ближайшее время. Спасибо за ваше сотрудничество!", false);
                        string channelLinkAp = $"https://discord.com/channels/{guild.Id}/{newChannelAp.Id}";
                        responseBuilder.AddField("⚖️ Создаем обжалование...", $"Ссылка на тикет: {channelLinkAp}", true);
                        await newChannelAp.SendMessageAsync(inTicketMessage);
                        break;
                    case "🔒 Приватное обращение":
                        var newChannelPr = await guild.CreateChannelAsync(
                            channelName,
                            DiscordChannelType.Text,
                            category,
                            "Тикет поддержки от " + eventArgs.User.Username);

                        inTicketMessage.AddField("🔒 Приватное обращение", "Пожалуйста, опишите ваш вопрос или проблему как можно подробнее. Чем больше информации вы предоставите, тем лучше мы сможем вам помочь.\n\n**Форма:**\n- Ваш игровой сикей\n- Описание вашего вопроса или проблемы\n- Любые дополнительные детали, которые вы считаете важными\n\nВаше обращение будет рассмотрено руководством проекта в ближайшее время. Спасибо за ваше доверие!", false);
                        string channelLinkPr = $"https://discord.com/channels/{guild.Id}/{newChannelPr.Id}";
                        responseBuilder.AddField("🔒 Создаем приватное обращение...", $"Ссылка на тикет: {channelLinkPr}", true);
                        await newChannelPr.SendMessageAsync(inTicketMessage);
                        break;
                    case "💙 Заявка в команду":
                        var newChannelVo = await guild.CreateChannelAsync(
                            channelName,
                            DiscordChannelType.Text,
                            category,
                            "Тикет поддержки от " + eventArgs.User.Username);

                        inTicketMessage.AddField("💙 Заявка в команду", "Пожалуйста, предоставьте как можно больше информации о себе и вашей мотивации. Чем больше деталей вы предоставите, тем эффективнее мы сможем рассмотреть вашу заявку.\n\n**Форма:**\n- Ваш игровой сикей\n- Ваш возраст\n- Ваш часовой пояс\n- Почему вы хотите стать волонтером?\n- Какой отдел и должность?(+ пинг главы отдела)\n- Сколько времени вы готовы уделять волонтерской деятельности?\n- Какой у вас есть опыт под вашу должность?\n\nМы рассмотрим вашу заявку в ближайшее время. Спасибо за ваше желание помочь проекту!", false);
                        string channelLinkVo = $"https://discord.com/channels/{guild.Id}/{newChannelVo.Id}";
                        responseBuilder.AddField("💙 Создаем заявку в команду...", $"Ссылка на тикет: {channelLinkVo}", true);
                        await newChannelVo.SendMessageAsync(inTicketMessage);
                        break;
                    default:
                        responseBuilder.AddField("Неизвестная опция", "Как вы сюда попали?");
                        break;
                }
                
                var responseSuccess = new DiscordInteractionResponseBuilder()
                    .AddEmbed(responseBuilder);

                responseSuccess.AsEphemeral(true);
                
                Client.Logger.LogInformation("Создан канал {Count}, тип: {Type}, пользователь: {User}", eventArgs.Channel.Name, selectedValue, eventArgs.User.Username);
                
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, responseSuccess);
            }
        }

        
        private static void StartPeriodicUpdate()
        {
            _updateTimer = new Timer(async _ =>
            {
                var channel = await Client.GetChannelAsync(_channelTeamRoster);
                await ProjectLeadStaffUpdate();
                Client.Logger.LogInformation("Произведен update команды проекта, причина: периодическое обновление. Канал: {ChannelName}", channel.Name);
            }, null, TimeSpan.FromMinutes(_timeToUpdateStaffListInMinutes), TimeSpan.FromMinutes(_timeToUpdateStaffListInMinutes));
        }
        
        public static async Task ProjectLeadStaffUpdate()
        {   
            var channel = await Client.GetChannelAsync(_channelTeamRoster);

            var guild = channel.Guild;

            var messages = new List<DiscordMessage>();
            await foreach (var message in channel.GetMessagesAsync(100))
            {
                messages.Add(message);
            }
            var botMessages = messages.Where(m => m.Author?.Id == Client.CurrentUser?.Id).ToList();
            if (botMessages.Any())
            {
                await channel.DeleteMessagesAsync(botMessages);
                Client.Logger.LogInformation("Удалено {Count} сообщений бота из канала {ChannelName}", botMessages.Count, channel.Name);
            }
            var projectLeadRole = guild.Roles.Values.FirstOrDefault(r => r.Id == _projectLeadRoleID);

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
                    Text = $"Данные предоставлены {Client.CurrentUser.Username} <3",
                    IconUrl = Client.CurrentUser.AvatarUrl
                }
            };

            string projectLeadMentions = ".";

            if (projectLeadRole != null)
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
}