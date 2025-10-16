namespace DiscordBot.Config;

public class AppConfig
{
    public string Token { get; set; } = null!;
    public string Prefix { get; set; } = null!;
    public Timer UpdateTimer = null!;
    public ulong ChannelTeamRoster { get; set; }
    public ulong ProjectLeadRoleId { get; set; }
    public int TimeToUpdateStaffListInMinutes { get; set; }
    public ulong LeadStaffRoleId { get; set; }
    public int TimeToCreateNewCourtChannelInMinutes { get; set; }
    public ulong ChannelSupportTicketCreate { get; set; }
    public ulong TicketsCategoryId { get; set; }
}