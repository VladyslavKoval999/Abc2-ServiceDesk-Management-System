using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ServiceDesk.Shared.Entities
{
    public class WorkOrder
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int EquipmentId { get; set; }
        public string IssueDescription { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending Start";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Customer? Customer { get; set; }
        public Equipment? Equipment { get; set; }
        public int? TechnicianId { get; set; }
        public Technician? Technician { get; set; }
        public string? RequesterName { get; set; }
        public string? RequesterContact { get; set; }
        public string? ContactMethod { get; set; }
        public string? EquipmentState { get; set; }
        public string? Cables { get; set; }
        public string? WorkPerformed { get; set; }
        public DateTime? WorkStartTime { get; set; }
        public DateTime? WorkEndTime { get; set; }
        public decimal LaborCost { get; set; } = 0;
        public decimal PartsCost { get; set; } = 0;

        [NotMapped]
        [JsonIgnore]
        public decimal TotalCost => LaborCost + PartsCost;

        public string? UsedPartsText { get; set; }
        public decimal HourlyRate { get; set; }
        public bool IsWarranty { get; set; } = false;
        public string? ClientSignatureBase64 { get; set; }
    }
}