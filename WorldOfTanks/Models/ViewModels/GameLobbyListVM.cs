using WorldOfTanks.Models.GameLobbyModels;

namespace WorldOfTanks.Models.ViewModels
{
    public class GameLobbyListVM
    {
        public string? PlayerName { get; set; }
        public string? Message { get; set; }
        public List<GameLobby>? GameLobbyList { get; set; }
    }
}
