namespace WorldOfTanks.Models.GameLobbyModels
{
    public class SessionPlayer
    {
        public string? PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public bool PlayerHost { get; set; }
        public string? LobbyId { get; set; }
        public DateTime DateTimeGetSetData { get; set; }
        public DateTime DateTimeSetMessage { get; set; }
    }
}
