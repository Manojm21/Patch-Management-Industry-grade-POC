
using SharedLibraryAgents.Enums;

public class PatchStatusReport
{
    public int Id { get; set; }
    public string AgentId { get; set; }
    public string PatchVersion { get; set; }
    public PatchStatusEnum Status { get; set; }
    public string ProductName { get; set; }
    public PatchTargetTypeEnum TargetType { get; set; }
    public DateTime Timestamp { get; set; }
}
