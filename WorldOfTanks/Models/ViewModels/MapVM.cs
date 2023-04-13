using WorldOfTanks.Models.GameObject;

namespace WorldOfTanks.Models.ViewModels
{
    public class MapVM
    {
        public string? Info { get; set; }
        public Map? Map { get; set; }
        public List<PassiveMapElement>? MapElementList { get; set; }
    }
}
