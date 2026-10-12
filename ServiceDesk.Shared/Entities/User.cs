using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceDesk.Shared.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee";
        public bool IsActive { get; set; } = true;
    }
}
