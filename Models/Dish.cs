using System.ComponentModel.DataAnnotations;

namespace ByteBite.Models
{
    public class Dish
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        public string ImagePath { get; set; }

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
    }
}
