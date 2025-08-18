using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibraryAgents.Models
{
    public class AgentMonitoredProducts
    {
        public int Id { get; set; }
        public string AgentId { get; set; }
        public string MonitoredProduct {  get; set; }

        public string CurrentVersion { get; set; }



    }
}
