using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedLibraryAgents.Interfaces;
using System.Diagnostics;

namespace PatchAgent.CustomerA
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IPatchAgentService _patchAgentService;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IPatchAgentService patchAgentService, IConfiguration configuration)
        {
            _logger = logger;
            _patchAgentService = patchAgentService;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int interval = _configuration.GetSection("AgentSettings").GetValue<int>("CheckIntervalSeconds");
            _logger.LogInformation("PatchAgent.CustomerA started with {Interval} second intervals", interval);
            
            int cycleCount = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                cycleCount++;
                var cycleStart = DateTime.Now;
                var stopwatch = Stopwatch.StartNew();
                
                _logger.LogInformation("=== Starting patch check cycle #{CycleCount} at {StartTime} ===", 
                    cycleCount, cycleStart.ToString("HH:mm:ss"));

                try
                {
                    await _patchAgentService.CheckAndApplyPatchAsync(stoppingToken);
                    
                    stopwatch.Stop();
                    _logger.LogInformation("=== Patch check cycle #{CycleCount} completed in {ElapsedSeconds:F1} seconds ===", 
                        cycleCount, stopwatch.Elapsed.TotalSeconds);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    _logger.LogError(ex, "=== Patch check cycle #{CycleCount} failed after {ElapsedSeconds:F1} seconds ===", 
                        cycleCount, stopwatch.Elapsed.TotalSeconds);
                }

                // Log the wait period
                _logger.LogInformation("Waiting {IntervalSeconds} seconds until next check (next cycle at approximately {NextTime})", 
                    interval, DateTime.Now.AddSeconds(interval).ToString("HH:mm:ss"));
                    
                await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
            }
            
            _logger.LogInformation("PatchAgent.CustomerA stopped after {TotalCycles} cycles", cycleCount);
        }
    }
}