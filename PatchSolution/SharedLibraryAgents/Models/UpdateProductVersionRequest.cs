using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibraryAgents.Models
{
    public class UpdateProductVersionRequest
    {
        public string AgentId { get; set; } = string.Empty;
        public string MonitoredProduct { get; set; } = string.Empty;
        public string CurrentVersion { get; set; } = string.Empty;
    }
}
