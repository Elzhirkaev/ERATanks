using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations;

namespace WorldOfTanks.Models.GameObject
{
    public class ActiveMapElement
    {
        [Key]
        public int ActMapElementId { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Description { get; set; }
        public string? ImageOnMap { get; set; }
        public string? ImageOnObj { get; set; }
        //время действия активки
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "range value 1-2147483647")]
        public int TimeOfAction { get; set; }
        //время жизни активки
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "range value 1-2147483647")]
        public int LifeTime { get; set; }
        
        
        



    }
}
