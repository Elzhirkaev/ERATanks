using Microsoft.AspNetCore.Identity;

namespace WorldOfTanks.Models.Register
{
    public class ApplicationUser : IdentityUser
    {
        public string? NickName { get; set; }
        public string? PlayerName { get; set; }
        public string? PlayerId { get; set; }
        public bool PlayerHost { get; set; }
        public string? LobbyId { get; set; }
    }
}
