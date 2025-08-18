using Microsoft.Extensions.Logging;

namespace PatchAgent.CustomerA.Utilities
{
    //For the customers we need to have thsi so that we respect their storage space.. deleting older version(more than 2 if there)
    public class CleanupUtility
    {
        private readonly ILogger<CleanupUtility> _logger;
        private readonly VersionUtility _versionUtility;
        private readonly LoggingUtility _loggingUtility;

        public CleanupUtility(
            ILogger<CleanupUtility> logger,
            VersionUtility versionUtility,
            LoggingUtility loggingUtility)
        {
            _logger = logger;
            _versionUtility = versionUtility;
            _loggingUtility = loggingUtility;
        }

        public async Task CleanupOldPatchesAsync(string productBasePath, string productName, int keepLatestCount = 2)
        {
            try
            {
                if (!Directory.Exists(productBasePath))
                    return;

                var versionDirectories = Directory.GetDirectories(productBasePath)
                    .Select(dir => new
                    {
                        Path = dir,
                        FolderName = Path.GetFileName(dir),
                        Version = _versionUtility.CleanAndParseVersion(Path.GetFileName(dir)),
                        CreatedTime = Directory.GetCreationTime(dir)
                    })
                    .Where(x => x.Version != null)
                    .OrderByDescending(x => x.Version)
                    .ThenByDescending(x => x.CreatedTime)
                    .ToList();

                if (versionDirectories.Count <= keepLatestCount)
                {
                    _logger.LogDebug("No cleanup needed for {Product}. Found {Count} versions, keeping {Keep}",
                        productName, versionDirectories.Count, keepLatestCount);
                    return;
                }

                var foldersToDelete = versionDirectories.Skip(keepLatestCount).ToList();
                var deletedCount = 0;

                foreach (var folder in foldersToDelete)
                {
                    try
                    {
                        Directory.Delete(folder.Path, recursive: true);
                        deletedCount++;
                        _logger.LogInformation("Cleaned up old patch folder: {Path}", folder.Path);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old patch folder: {Path}", folder.Path);
                    }
                }

                if (deletedCount > 0)
                {
                    await _loggingUtility.LogInfoAsync($"🧹 Cleaned up {deletedCount} old patch folders for {productName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during patch cleanup for {Product}", productName);
            }
        }
    }
}