using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using blazorSBIFS.Shared.Models;

namespace blazorSBIFS.Shared.DataTransferObjects
{
    public class GroupDto : IJson
    {
        public int GroupID { get; set; }
    }
}
