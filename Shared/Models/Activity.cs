using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blazorSBIFS.Shared.Models
{
    public class Activity
    {
        public int ActivityID { get; set; }
        public Group Group { get; set; }
        public int OwnerID { get; set; }
        public double Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<User> Participants { get; set; } = new List<User>();
    }
}
