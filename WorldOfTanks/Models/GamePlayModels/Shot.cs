using WorldOfTanks.Models.GameObject;

namespace WorldOfTanks.Models.GamePlayModels
{
    public class Shot
    {
        public int PlayerNum { get; set; }
        public int ShotNum { get; set; }
        public int Speed { get; set; }
        public int Damage { get; set; }
        public List<MapPoint>? MapPoints { get; set; }
        public int[][]? Coords { get; set; }
        public bool Remove { get; set; }
    }
}
