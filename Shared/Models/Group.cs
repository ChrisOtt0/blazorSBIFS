using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using blazorSBIFS.Shared.DataTransferObjects;

namespace blazorSBIFS.Shared.Models
{
    public class Group : IJson
    {
        public int GroupID { get; set; }
        public int OwnerID { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<User> Participants { get; set; } = new List<User>();
        public List<Activity> Activities { get; set; } = new List<Activity>();
    }
}
