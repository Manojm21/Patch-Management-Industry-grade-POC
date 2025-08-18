using SharedLibraryAgents.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


      
   

    namespace SharedLibraryAgents.Models
    {
   
    public class PatchDto
        {
            public string Version { get; set; }
            public string Description { get; set; }
            public string DownloadUrl { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PatchTargetTypeEnum TargetType { get; set; }  // Enum
            public string ProductName { get; set; } // if TargetType is Agent
        }
    }




