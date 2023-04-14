using System.ComponentModel.DataAnnotations;
using WorldOfTanks.Models.GameObject;
using WorldOfTanks.Models.GamePlayModels;

namespace WorldOfTanks.Models.GameLobbyModels
{
    public class GameLobby
    {
        public string? LobbyId { get; set; }
        [Required]
        [Display(Name ="Lobby Name")]
        [StringLength(8, MinimumLength = 2, ErrorMessage = "The length of the string should be from 2 to 8 characters")]
        public string? Name { get; set; }
        public DateTime CreationTime { get; set; }
        public bool InGame { get; set; }
        [Required]
        [Range(1, 64, ErrorMessage = "range value 1-64")]
        public int MaxPlayer { get; set; }
        public List<Player>? PlayerList { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "range value 1-2147483647")]
        public int MapId { get; set; }
        public int NumOfTeams { get; set; }
        [Required]
        [Range(100, 10000, ErrorMessage = "range value 100-10000")]
        public int RebirthPoints { get; set; }
        public bool FriendlyFire { get; set; }

        public List<string>? chatMessageList;

        public void AddMessage(string? message)
        {
            if (message != null)
            {
                if (chatMessageList!.Count > 50)
                {
                    chatMessageList.Remove(chatMessageList[0]);
                }
                chatMessageList!.Add(message);
            }
        }
    }
}
