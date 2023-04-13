using Microsoft.AspNetCore.Mvc.Rendering;
using WorldOfTanks.Models.GameObject;

namespace WorldOfTanks.Models.ViewModels
{
    public class TankVM
    {
        public Tank? Tank { get; set; }
        public IEnumerable<SelectListItem>? WeaponSelectList { get; set; }
    }
}
