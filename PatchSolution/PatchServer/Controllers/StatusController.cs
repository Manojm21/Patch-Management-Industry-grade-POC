using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;
using PatchServer.Data;
using SharedLibraryAgents.Enums;
using SharedLibraryAgents.Models;

namespace PatchServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly ILogger<StatusController> _logger;
        private readonly PatchDbContext _context;

        public StatusController(ILogger<StatusController> logger, PatchDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("Server is running");
        }

        [HttpPost("report")]
        public async Task<IActionResult> ReportPatchStatus([FromBody] PatchStatusDto status)
        {
            if (status == null || string.IsNullOrEmpty(status.AgentId) || string.IsNullOrEmpty(status.PatchVersion))
            {
                return BadRequest("Invalid status report.");
            }

            _logger.LogInformation("[PATCH STATUS] Agent: {AgentId}, Product: {Product}, Version: {Version}, Status: {Status}, Timestamp: {Timestamp}",
                status.AgentId, status.ProductName, status.PatchVersion, status.Status, status.Timestamp);

            var exists = await _context.PatchStatusReports.AnyAsync(p =>
                p.AgentId == status.AgentId &&
                p.ProductName == status.ProductName &&
                p.PatchVersion == status.PatchVersion &&
                p.Status == status.Status);

            if (exists)
            {
                return Ok(new { message = "Duplicate status ignored." });
            }

            var report = new PatchStatusReport
            {
                AgentId = status.AgentId,
                PatchVersion = status.PatchVersion,
                Status = status.Status,
                ProductName = status.ProductName,
                TargetType = status.TargetType,
                Timestamp = status.Timestamp
            };

            _context.PatchStatusReports.Add(report);
            await _context.SaveChangesAsync();

            // ✅ Update CurrentVersion if patch was applied
            if (status.Status == PatchStatusEnum.Applied)
            {
                var agent = await _context.AgentMonitoredProducts
                    .FirstOrDefaultAsync(a =>
                        a.AgentId == status.AgentId &&
                        a.MonitoredProduct.Contains(status.ProductName));

                if (agent != null)
                {
                    agent.CurrentVersion = status.PatchVersion;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Updated agent {AgentId} current version to {Version}", agent.AgentId, agent.CurrentVersion);
                }
                else
                {
                    _logger.LogWarning("Agent {AgentId} with monitored product {Product} not found for version update.",
                        status.AgentId, status.ProductName);
                }
            }

            return Ok(new { message = "Patch status saved and agent updated successfully." });
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await _context.PatchStatusReports
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();

            return Ok(reports);
        }

    }
}
