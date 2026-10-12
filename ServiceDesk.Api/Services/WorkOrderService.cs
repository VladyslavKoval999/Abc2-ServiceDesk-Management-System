using ServiceDesk.Shared;
using ServiceDesk.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ServiceDesk.Api.Services
{
    public class WorkOrderService
    {
        private readonly AppDbContext _context;

        public WorkOrderService(AppDbContext context)
        {
            _context = context;
        }

        public List<WorkOrder> GetAllWorkOrders()
        {
            return _context.WorkOrders
                .Include(w => w.Customer)
                .Include(w => w.Equipment)
                .Include(w => w.Technician)
                .ToList();
        }

        public void AddWorkOrder(WorkOrder workOrder)
        {
            _context.WorkOrders.Add(workOrder);
            _context.SaveChanges();
        }

        public void UpdateWorkOrder(WorkOrder workOrder)
        {
            _context.WorkOrders.Update(workOrder);
            _context.SaveChanges();
        }

        public void DeleteWorkOrder(int id)
        {
            var workOrder = _context.WorkOrders.Find(id);
            if (workOrder != null)
            {
                _context.WorkOrders.Remove(workOrder);
                _context.SaveChanges();
            }
        }
    }
}