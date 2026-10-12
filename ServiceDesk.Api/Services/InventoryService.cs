using ServiceDesk.Shared;
using ServiceDesk.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ServiceDesk.Api.Services
{
    public class InventoryService
    {
        private readonly AppDbContext _context;

        public InventoryService(AppDbContext context)
        {
            _context = context;
        }

        public List<InventoryItem> GetAllItems()
        {
            return _context.InventoryItems.ToList();
        }

        public void AddItem(InventoryItem item)
        {
            _context.InventoryItems.Add(item);
            _context.SaveChanges();
        }

        public InventoryItem GetItemById(int id)
        {
            return _context.InventoryItems.Find(id);
        }

        public void UpdateItem(InventoryItem updatedItem)
        {
            _context.InventoryItems.Update(updatedItem);
            _context.SaveChanges();
        }

        public void DeleteItem(int id)
        {
            var item = _context.InventoryItems.Find(id);
            if (item != null)
            {
                _context.InventoryItems.Remove(item);
                _context.SaveChanges();
            }
        }
    }
}