using System.ComponentModel.DataAnnotations;
using WorldOfTanks.Models.GameObject;
using WorldOfTanks.Models.GamePlayModels;

namespace WorldOfTanks.Models.GameLobbyModels
{
    public class Player
    {
        public string? UserId { get; set; }
        public string? PlayerId { get; set; }
        public bool Host { get; set; }
        public bool Ready { get; set; }
        [Display(Name ="Player Name")]
        public string? Name { get; set; }
        public int PlayerNum { get; set; }
        public int Team { get; set; }
        public int Kills { get; set; }
        public int Death { get; set; }
        public int DamageSum { get; set; }
        public int DamageFriendly { get; set; }
        public int RebirthPoints { get; set; }
        public Tank? Tank { get; set; }
        public bool OnMove { get; set; }
        public bool OnGet { get; set; }
        public bool FireReady { get; set; }
        public int LostVictory { get; set; }
        public double XDouble { get; set; }
        public double YDouble { get; set; }
        public double Speed { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        private double directionDouble;
        public double DirectionDouble 
        {
            get
            {
                if (directionDouble < 0)
                {
                    directionDouble = 360 + directionDouble;
                }
                return directionDouble;
            }
            set
            {
                directionDouble = value % 360;
            }
        }
        private int direction;
        public int Direction
        {
            get
            {
                if (direction < 0)
                {
                    direction = 360 + direction;
                } 
                return direction; 
            } 
            set 
            { 
                direction = value % 360; 
            }
        }
        private int directionTower;
        public int DirectionTower 
        {
            get
            {
                if (directionTower < 0)
                {
                    directionTower = 360 + directionTower;
                }
                return directionTower;
            }
            set
            {
                directionTower = value % 360;
            } 
        }
        private int shotCount;
        public int ShotCount 
        {
            get
            {
                return shotCount;
            }
            set
            {
                shotCount = value % 100000;
            }
        }
        public DateTime ShotTime { get; set; }
        public DateTime RespTime { get; set; }
        public bool Invulnerability { get; set; }
        public bool TankPointClear { get; set; }
        public int[,][]? TankPoint { get; set; }
        public Dictionary<string, int>? RemoveTankList { get; set; }
        public List<int>? PlayerVision { get; set; }
        public List<int>? CVVision { get; set; }
        public List<MapPoint>? BeamList1 { get; set; }
        public GameDataJSON? GameDataJSON { get; set; }
    }
}
