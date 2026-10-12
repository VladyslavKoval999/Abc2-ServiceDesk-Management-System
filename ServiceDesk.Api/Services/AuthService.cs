using ServiceDesk.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ServiceDesk.Api.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public bool Authenticate(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == password && u.IsActive);

            return user != null;
        }
    }
}
