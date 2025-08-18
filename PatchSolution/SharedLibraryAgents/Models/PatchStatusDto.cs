
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SharedLibraryAgents.Enums;

namespace SharedLibraryAgents.Models
{
    public class PatchStatusDto
    {
        public string AgentId { get; set; } = string.Empty;

        public string PatchVersion { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PatchStatusEnum Status { get; set; }
        public string ProductName { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PatchTargetTypeEnum TargetType { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    }
}

