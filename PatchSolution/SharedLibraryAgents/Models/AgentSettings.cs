using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibraryAgents.Models
{
   


    public class AgentSettings
    {
        public string AgentId { get; set; }
        public string PatchServerUrl { get; set; }
        public int CheckIntervalSeconds { get; set; } = 60;

        }


    }


