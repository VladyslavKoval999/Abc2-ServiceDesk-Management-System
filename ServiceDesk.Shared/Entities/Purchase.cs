using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceDesk.Shared.Entities
{
    public class Purchase
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public int InventoryItemId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public Supplier? Supplier { get; set; }
        public InventoryItem? InventoryItem { get; set; }
    }
}
