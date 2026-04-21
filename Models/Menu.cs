using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ByteBite.Models
{
    public class Menu
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ICollection<Category> Categories { get; set; } = new HashSet<Category>();
    }
}