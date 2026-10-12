using System;

namespace ServiceDesk.Shared.Entities
{
    public class Quotation
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string EquipmentDetails { get; set; } = string.Empty;
        public string ProblemDescription { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public string Status { get; set; } = "Pending Start";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Customer? Customer { get; set; }
        public string? ClientSignatureBase64 { get; set; }
    }
}