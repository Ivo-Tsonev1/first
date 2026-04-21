using System.ComponentModel.DataAnnotations;

namespace ByteBite.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int MenuId { get; set; }
        public virtual Menu Menu { get; set; }
    }
}