using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blazorSBIFS.Shared.DataTransferObjects
{
    public class CalculationDto : IJson
    {
        public string? Str { get; set; }
        public string? Html { get; set; }
        public object? Pdf { get; set; }
    }
}
