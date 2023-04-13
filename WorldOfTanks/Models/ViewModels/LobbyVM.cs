using WorldOfTanks.Models.GameLobbyModels;
using WorldOfTanks.Models.GameObject;

namespace WorldOfTanks.Models.ViewModels
{
    public class LobbyVM
    {
        public GameLobby? GameLobby { get; set; }
        public string? Error { get; set; }
        public List<PassiveMapElement>? MapElementList { get; set; }
        public List<Map>? MapList { get; set; }

    }
}
