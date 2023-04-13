using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations;

namespace WorldOfTanks.Models.GameObject
{
    public class PassiveMapElement
    {
        [Key]
        public int PasMapElementId { get; set; }
        [Required]
        [StringLength(6, MinimumLength = 2, ErrorMessage = "The length of the string should be from 2 to 6 characters")]
        public string? Name { get; set; }
        [Required]
        public string? Description { get; set; }
        public string? Image { get; set; }
        [Required]
        [Display(Name = "HP")]
        [Range(0, int.MaxValue, ErrorMessage = "range value 1-2147483647")]
        public int Heath { get; set; }
        //вязкость
        [Required]
        [Range(-100, 100, ErrorMessage = "the range of values is from -100 to 100")]
        public int Viscosity { get; set; }
        public bool Resp { get; set; }
        [Required]
        [Range(0, 8, ErrorMessage = "range value 0-8")]
        public int RespTeam { get; set; }
        public bool HQ { get; set; }
        [Required]
        [Range(0, 8, ErrorMessage = "range value 0-8")]
        public int HQTeam { get; set; }
        //подложка
        public bool Background { get; set; }
        //пулепроницаемость
        [Display(Name = "Bullet Permeability")]
        public bool BulletPermeability { get; set; }
        //машинопроницаемость
        [Display(Name = "Machine Permeability")]
        public bool MachinePermeability { get; set; }
        //неуязвимость
        public bool Invulnerability { get; set; }
        
        




    }
}
