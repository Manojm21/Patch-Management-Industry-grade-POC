using SharedLibraryAgents.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace SharedLibrary.Models
{

    public class Patch
    {
        public int Id { get; set; }
        public string PatchName { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime ScheduledTime { get; set; }

        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        
        public PatchTargetTypeEnum TargetType { get; set; }
        public string ProductName { get; set; } 
        public string Version { get; set; } 
        public string DownloadUrl { get; set; } // URL for downloading the patch
    }

}


