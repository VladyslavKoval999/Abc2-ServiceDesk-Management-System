using ServiceDesk.Shared;
using ServiceDesk.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ServiceDesk.Api.Services
{
    public class QuotationService
    {
        private readonly AppDbContext _context;

        public QuotationService(AppDbContext context)
        {
            _context = context;
        }

        public List<Quotation> GetAllQuotations()
        {
            return _context.Quotations
                .Include(q => q.Customer)
                .OrderByDescending(q => q.CreatedAt)
                .ToList();
        }

        public void AddQuotation(Quotation quotation)
        {
            _context.Quotations.Add(quotation);
            _context.SaveChanges();
        }

        public void UpdateQuotation(Quotation quotation)
        {
            var existingQuotation = _context.Quotations.FirstOrDefault(q => q.Id == quotation.Id);

            if (existingQuotation == null)
            {
                throw new Exception("Quotation not found.");
            }

            existingQuotation.CustomerId = quotation.CustomerId;
            existingQuotation.EquipmentDetails = quotation.EquipmentDetails;
            existingQuotation.ProblemDescription = quotation.ProblemDescription;
            existingQuotation.EstimatedCost = quotation.EstimatedCost;
            existingQuotation.Status = quotation.Status;
            existingQuotation.ClientSignatureBase64 = quotation.ClientSignatureBase64;

            _context.SaveChanges();
        }

        public void UpdateStatus(int id, string newStatus)
        {
            var quotation = _context.Quotations.Find(id);
            if (quotation != null)
            {
                quotation.Status = newStatus;
                _context.SaveChanges();
            }
        }

        public void DeleteQuotation(int id)
        {
            var quotation = _context.Quotations.Find(id);
            if (quotation != null)
            {
                _context.Quotations.Remove(quotation);
                _context.SaveChanges();
            }
        }
    }
}