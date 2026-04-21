using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ByteBite.Models
{
    public class DiningTable
    {
        public int Id { get; set; }

        [Required]
        public int TableNumber { get; set; }

        public int Capacity { get; set; }

        public bool IsOccupied { get; set; } = false;

        public virtual ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}