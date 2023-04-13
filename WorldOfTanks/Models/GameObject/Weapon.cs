using System.ComponentModel.DataAnnotations;

namespace WorldOfTanks.Models.GameObject
{
    public class Weapon
    {
        [Key]
        public int WeaponId { get; set; }
        [Required]
        [StringLength(8, MinimumLength = 2, ErrorMessage = "The length of the string should be from 2 to 8 characters")]
        public string? Name { get; set; }
        [Required]
        public string? Description { get; set; }
        public string? Image { get; set; }
        [Required]
        [Range(1, 10000, ErrorMessage = "range value 1-10000")]
        public int Damage { get; set; }
        //скорострельность
        [Required]
        [Range(1, 600, ErrorMessage = "range value 1-600")]
        public int FiringRate { get; set; }
        //скорость снаряда
        [Required]
        [Range(1, 10000, ErrorMessage = "range value 1-10000")]
        public int BulletSpeed { get; set; }
    }
}
