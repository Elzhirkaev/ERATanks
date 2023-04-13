using System.ComponentModel.DataAnnotations;

namespace WorldOfTanks.Models.GameObject
{
    [Serializable]
    public class Map
    {
        [Key]
        public int MapId { get; set; }
        [Required]
        [StringLength(8, MinimumLength = 2, ErrorMessage = "The length of the string should be from 2 to 8 characters")]
        public string? Name { get; set; }
        public string? Author { get; set; }
        [Required]
        public string? Description { get; set; }
        public string? MapElementBGIdList { get; set; }
        public string? MapElementCVIdList { get; set; }
        [Required]
        public string? MapPointListBG { get; set; }
        [Required]
        public string? MapPointListCV { get; set; }
        [Required]
        [Range(200, 2000, ErrorMessage = "range value 200-2000")]
        public int Width { get; set; }
        [Required]
        [Range(200, 2000, ErrorMessage = "range value 200-2000")]
        public int Height { get; set; }
        [Required]
        [Range(1, 8, ErrorMessage = "range value 1-8")]
        public int NumOfTeams { get; set; }
    }
}
