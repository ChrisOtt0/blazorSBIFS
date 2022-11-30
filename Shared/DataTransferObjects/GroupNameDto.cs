using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blazorSBIFS.Shared.DataTransferObjects
{
    public class GroupNameDto : IJson
    {
        public int GroupID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
