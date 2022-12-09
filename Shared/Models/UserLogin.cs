using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blazorSBIFS.Shared.Models
{
    public class UserLogin
    {
        public int UserLoginID { get; set; }
        public string? Email { get; set; } = string.Empty;
        public string? Password { get; set; } = string.Empty;
        public int? UserID { get; set; } = 0;
        public User? User { get; set; }
    }
}
