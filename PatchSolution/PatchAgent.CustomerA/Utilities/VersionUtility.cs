using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using System.Text.RegularExpressions;

namespace PatchAgent.CustomerA.Utilities
{
    //version check logic
    public class VersionUtility
    {
        private readonly ILogger<VersionUtility> _logger;

        public VersionUtility(ILogger<VersionUtility> logger)
        {
            _logger = logger;
        }

        public NuGetVersion? CleanAndParseVersion(string versionString)
        {
            if (string.IsNullOrEmpty(versionString))
                return null;

            try
            {
                var cleanVersion = versionString;

                // Remove common prefixes
                if (cleanVersion.StartsWith("Patch-", StringComparison.OrdinalIgnoreCase))
                {
                    cleanVersion = cleanVersion.Substring(6);
                }

                if (cleanVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                {
                    cleanVersion = cleanVersion.Substring(1);
                }

                // Use regex to extract version numbers
                var versionMatch = Regex.Match(cleanVersion, @"(\d+\.\d+(?:\.\d+)?(?:\.\d+)?)");
                if (versionMatch.Success)
                {
                    cleanVersion = versionMatch.Groups[1].Value;
                }

                return NuGetVersion.Parse(cleanVersion);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse version string: {Version}", versionString);
                return null;
            }
        }

        public bool IsUpdateAvailable(string currentVersion, string latestVersion)
        {
            var current = CleanAndParseVersion(currentVersion);
            var latest = CleanAndParseVersion(latestVersion);

            if (current == null || latest == null)
            {
                _logger.LogWarning("Unable to parse version strings. Current: {Current}, Latest: {Latest}",
                    currentVersion, latestVersion);
                return false;
            }

            return latest > current;
        }

        public string GetHigherVersion(string version1, string version2)
        {
            var ver1 = CleanAndParseVersion(version1);
            var ver2 = CleanAndParseVersion(version2);

            if (ver1 == null && ver2 == null) return "0.0.0";
            if (ver1 == null) return version2;
            if (ver2 == null) return version1;

            return ver1 > ver2 ? version1 : version2;
        }
    }
}