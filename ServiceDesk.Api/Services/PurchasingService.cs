using ServiceDesk.Shared;
using ServiceDesk.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ServiceDesk.Api.Services
{
    public class PurchasingService
    {
        private readonly AppDbContext _context;

        public PurchasingService(AppDbContext context)
        {
            _context = context;
        }

        public List<Purchase> GetAllPurchases()
        {
            return _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.InventoryItem)
                .ToList();
        }

        public void RecordPurchase(Purchase purchase)
        {
            _context.Purchases.Add(purchase);
            var inventoryItem = _context.InventoryItems.Find(purchase.InventoryItemId);
            if (inventoryItem != null)
            {
                inventoryItem.QuantityInStock += purchase.Quantity;
            }

            _context.SaveChanges();
        }
    }
}