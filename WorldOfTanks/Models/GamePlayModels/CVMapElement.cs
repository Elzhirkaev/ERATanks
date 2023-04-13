namespace WorldOfTanks.Models.GamePlayModels
{
    public class CVMapElement
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public int Heath { get; set; }
        public bool Resp { get; set; }
        public int RespTeam { get; set; }
        public bool HQ { get; set; }
        public int HQTeam { get; set; }
        public bool BulletPermeability { get; set; }
        public bool MachinePermeability { get; set; }
        public bool Invulnerability { get; set; }
    }
}
