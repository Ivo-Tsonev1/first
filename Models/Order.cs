using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ByteBite.Models
{
    public class Order
    {
        public int Id { get; set; }

        public DateTime OrderTime { get; set; }

        public bool IsPaid { get; set; } = false;

        public decimal TotalPrice { get; set; }

        public int DiningTableId { get; set; }
        public virtual DiningTable DiningTable { get; set; }

        public string WaiterId { get; set; }
        public virtual IdentityUser Waiter { get; set; }

        public virtual ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}