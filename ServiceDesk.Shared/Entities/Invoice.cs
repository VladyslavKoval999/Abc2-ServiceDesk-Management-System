using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceDesk.Shared.Entities
{
    public class Invoice
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime IssueDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Unpaid";
        public WorkOrder? WorkOrder { get; set; }
    }
}
