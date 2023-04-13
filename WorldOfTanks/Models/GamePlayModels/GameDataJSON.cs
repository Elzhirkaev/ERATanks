using WorldOfTanks.Models.GameObject;

namespace WorldOfTanks.Models.GamePlayModels
{
    public class GameDataJSON
    {
        public PJSON? PJSON { get; set; }
        public List<PlayerJSON>? PlayerListJSON { get; set; }
        public List<int>? TeamVisionListJSON { get; set; }
        public List<MapPoint>? PlayerShotListJSON { get; set; }
        public List<MapPoint>? FireExplosionListJSON { get; set; }
        public List<MapPoint>? ExplosionListJSON { get; set; }
        public List<string>? RemoveCVListJSON { get; set; }
    }
}
