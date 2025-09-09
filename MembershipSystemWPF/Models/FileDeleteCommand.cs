using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MembershipSystemWPF.Models
{
    public class FileDeleteCommand
    {
        public string FilePath { get; set; } = string.Empty;
        public string ContentToRemove { get; set; } = string.Empty;
        public string LogMessage { get; set; } = string.Empty;
    }
}
