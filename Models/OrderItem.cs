using System.ComponentModel.DataAnnotations;

namespace ByteBite.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public int DishId { get; set; }
        public virtual Dish Dish { get; set; }

        public int Quantity { get; set; }
    }
}