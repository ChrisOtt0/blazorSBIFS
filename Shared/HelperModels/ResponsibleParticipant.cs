using blazorSBIFS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blazorSBIFS.Shared.HelperModels
{
    public class ResponsibleParticipant
    {
        public User? User { get; set; }
        public string? Name { get; set; } = string.Empty;
        public bool IsResponsible { get; set; } = true;
    }
}
