namespace DiscordBot.config;

public class JSONReader
{
        public string token { get; set; }
        public string prefix { get; set; }
        public ulong channelTeamRoster { get; set; }
        public ulong projectLeadRoleID { get; set; }
        public int timeToUpdateStaffListInMinutes { get; set; }
        public ulong leadStaffRoleID { get; set; }
        public int timeToCreateNewCourtChannelInMinutes { get; set; }
        public ulong channelSupportTicketCreate { get; set; }
        public ulong ticketsCategoryID { get; set; }

        public async Task ReadJson()
        {
                using (StreamReader sr = new StreamReader("config.json"))
                {
                        string json = await sr.ReadToEndAsync();
                        JSONStructure jsonStructure = Newtonsoft.Json.JsonConvert.DeserializeObject<JSONStructure>(json);
                        
                        token = jsonStructure.Token;
                        prefix = jsonStructure.Prefix;
                        channelTeamRoster = jsonStructure.ChannelTeamRoster;
                        projectLeadRoleID = jsonStructure.ProjectLeadRoleID;
                        timeToUpdateStaffListInMinutes = jsonStructure.TimeToUpdateStaffListInMinutes;
                        leadStaffRoleID = jsonStructure.LeadStaffRoleID;
                        timeToCreateNewCourtChannelInMinutes = jsonStructure.TimeToCreateNewCourtChannelInMinutes;
                        channelSupportTicketCreate = jsonStructure.ChannelSupportTicketCreate;
                        ticketsCategoryID = jsonStructure.ChannelSupportTicketCreate;
                }
        }
}

internal sealed class JSONStructure
{
        public string Token { get; set; }
        public string Prefix { get; set; }    
        public ulong ChannelTeamRoster { get; set; }
        public ulong ProjectLeadRoleID { get; set; }
        public int TimeToUpdateStaffListInMinutes { get; set; }
        public ulong LeadStaffRoleID { get; set; }
        public int TimeToCreateNewCourtChannelInMinutes { get; set; }
        public ulong ChannelSupportTicketCreate { get; set; }
        public ulong TicketsCategoryID { get; set; }
}