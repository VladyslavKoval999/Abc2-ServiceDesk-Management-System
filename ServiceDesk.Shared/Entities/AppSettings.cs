using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceDesk.Shared.Entities
{
    public class AppSettings
    {
        public string Language { get; set; } = "English";
        public string Currency { get; set; } = "EUR (€)";
        public string CompanyName { get; set; } = "Abc2 ServiceDesk";
        public string CompanyTaxId { get; set; } = "";
    }
}
