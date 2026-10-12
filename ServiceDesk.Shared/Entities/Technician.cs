using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceDesk.Shared.Entities
{
    public class Technician
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public decimal HourlyRate { get; set; } = 0.0m;
    }
}
