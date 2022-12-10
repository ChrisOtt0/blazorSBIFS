using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using blazorSBIFS.Shared.DataTransferObjects;

namespace blazorSBIFS.Shared.Models
{
    public class Activity : IJson
    {
        public int ActivityID { get; set; }
        public Group? Group { get; set; }
        public int? OwnerID { get; set; } = 0;
        public double? Amount { get; set; } = 0.0;
        public string? Description { get; set; } = string.Empty;
        public List<User>? Participants { get; set; } = new List<User>();
    }
}
