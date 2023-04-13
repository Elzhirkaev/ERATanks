using System.ComponentModel.DataAnnotations;
using WorldOfTanks.Models.GameObject;

namespace WorldOfTanks.Models.ViewModels
{
    public class PlayerVM
    {
        public string? Name { get; set; }
        public bool Host { get; set; }
        public int Team { get; set; }
        public bool Ready { get; set; }
    }
}
