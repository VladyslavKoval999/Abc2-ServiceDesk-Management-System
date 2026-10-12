using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceDesk.Shared.Entities
{
    public class Equipment
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public Customer? Customer { get; set; }
    }
}