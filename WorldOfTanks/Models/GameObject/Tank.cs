using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorldOfTanks.Models.GameObject
{
    public class Tank
    {
        [Key]
        public int TankId { get; set; }
        [Required]
        [StringLength(8, MinimumLength = 2, ErrorMessage = "The length of the string should be from 2 to 8 characters")]
        public string? Name { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        [Range(0, 1000, ErrorMessage = "range value 1-1000")]
        public int RebirthPoints { get; set; }
        public string? Image { get; set; }
        [Required]
        [Range(1, 1000, ErrorMessage = "range value 1-1000")]
        public int SpeedTank { get; set; }
        [Required]
        [Range(1, 1000, ErrorMessage = "range value 1-1000")]
        public int SpeedUp { get; set; }
        [Required]
        [Range(1, 1000, ErrorMessage = "range value 1-1000")]
        public int SpeedRotation { get; set; }
        [Required]
        [Range(1, 10000, ErrorMessage = "range value 1-10000")]
        public int Health { get; set; }
        [Required]
        [Display(Name ="Weapon Type")]
        public int WeaponId { get; set; }
        [ForeignKey("WeaponId")]
        public virtual Weapon? Weapon { get; set; }
        
    }
}
