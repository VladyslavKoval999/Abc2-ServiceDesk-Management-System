using ServiceDesk.Shared;
using ServiceDesk.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ServiceDesk.Api.Services
{
    public class TechnicianService
    {
        private readonly AppDbContext _context;

        public TechnicianService(AppDbContext context)
        {
            _context = context;
        }

        public List<Technician> GetAllTechnicians()
        {
            return _context.Technicians.ToList();
        }

        public void AddTechnician(Technician technician)
        {
            _context.Technicians.Add(technician);
            _context.SaveChanges();
        }

        public bool DeleteTechnician(int id)
        {
            var technician = _context.Technicians.Find(id);

            if (technician == null)
            {
                return false;
            }

            bool hasTasks = _context.WorkOrders.Any(w => w.TechnicianId == id);
            if (hasTasks)
            {
                throw new Exception("Cannot delete technician because they are assigned to existing tasks. Reassign their tasks first.");
            }

            _context.Technicians.Remove(technician);
            _context.SaveChanges();

            return true;
        }
    }
}