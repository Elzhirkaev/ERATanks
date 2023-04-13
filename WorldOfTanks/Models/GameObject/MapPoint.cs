using System.ComponentModel.DataAnnotations;

namespace WorldOfTanks.Models.GameObject
{
    public class MapPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public double XDouble { get; set; }
        public double YDouble { get; set; }
        //направление
        public int Direction { get; set; }
    }
}
