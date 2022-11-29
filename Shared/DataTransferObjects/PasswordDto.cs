using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blazorSBIFS.Shared.DataTransferObjects
{
    public class PasswordDto : IJson
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty; 
    }
}
