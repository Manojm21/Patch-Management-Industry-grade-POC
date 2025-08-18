using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibraryAgents.Models
{
    public class PatchPathSettings
    {
        public string PatchDownloadBasePath { get; set; } = "C:/patches";
        public string PatchVersionStoragePath { get; set; } = "patch_versions";
    }
}
