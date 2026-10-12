using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceDesk.Shared.Entities
{
    public class Supplier
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
    }
}
