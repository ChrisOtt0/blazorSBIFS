using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blazorSBIFS.Shared.DataTransferObjects
{
    public class CalculationDto : IJson
    {
        public string Results { get; set; } = string.Empty;
    }
}
