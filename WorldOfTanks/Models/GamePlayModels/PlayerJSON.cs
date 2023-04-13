using WorldOfTanks.Models.GameObject;

namespace WorldOfTanks.Models.GamePlayModels
{
    public class PlayerJSON
    {
        public string? Name { get; set; }
        public bool Invulnerability { get; set; }
        public int RebirthPoints { get; set; }
        public int Kills { get; set; }
        public int Death { get; set; }
        public int DamageSum { get; set; }
        public int FriendlyFire { get; set; }
        public int TankId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Direction { get; set; }
        public int DirectionTower { get; set; }
    }
}
