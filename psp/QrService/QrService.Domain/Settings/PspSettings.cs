using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrService.Domain.Settings
{
    public class PspSettings
    {
        public const string SectionName = "PSP";
        public string PspId { get; set; } = string.Empty;
        public string PspPassword { get; set; } = string.Empty;
    }
}
