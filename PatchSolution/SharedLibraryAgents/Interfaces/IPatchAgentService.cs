using SharedLibraryAgents.Models;

namespace SharedLibraryAgents.Interfaces
{
    public interface IPatchAgentService
    {
        public Task CheckAndApplyPatchAsync(CancellationToken cancellationToken = default);
        public Task<PatchDto?> GetLatestPatchAsync(string agentId, string productName);
        
        public Task DownloadAndApplyPatchAsync(PatchDto patch, string productName);
        public Task SaveCurrentPatchVersionAsync(string agentId,string productName, string version);
        public Task<string> GetCurrentPatchVersionAsync(string agentId,string productName);
        public Task ReportPatchStatusAsync(PatchStatusDto status);

    }
}
