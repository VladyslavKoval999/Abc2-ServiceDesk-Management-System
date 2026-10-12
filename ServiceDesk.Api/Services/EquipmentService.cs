using ServiceDesk.Shared;
using ServiceDesk.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ServiceDesk.Api.Services
{
    public class EquipmentService
    {
        private readonly AppDbContext _context;

        public EquipmentService(AppDbContext context)
        {
            _context = context;
        }

        public List<Equipment> GetAllEquipment()
        {
            return _context.Equipments.Include(e => e.Customer).ToList();
        }

        public void AddEquipment(Equipment equipment)
        {
            _context.Equipments.Add(equipment);
            _context.SaveChanges();
        }

        public Equipment GetEquipmentById(int id)
        {
            return _context.Equipments.Find(id);
        }

        public void UpdateEquipment(Equipment updatedEquipment)
        {
            _context.Equipments.Update(updatedEquipment);
            _context.SaveChanges();
        }

        public void DeleteEquipment(int id)
        {
            var eq = _context.Equipments.Find(id);
            if (eq != null)
            {
                _context.Equipments.Remove(eq);
                _context.SaveChanges();
            }
        }
    }
}