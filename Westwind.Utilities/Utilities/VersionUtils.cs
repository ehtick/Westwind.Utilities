using System;

namespace Westwind.Utilities;

public static class VersionExtensions
{
    /// <summary>
    /// Formats a version by stripping all zero values
    /// up to the trimTokens count provided. By default
    /// displays Major.Minor and then displays any
    /// Build and/or revision if non-zero	
    /// 
    /// More info: https://weblog.west-wind.com/posts/2024/Jun/13/C-Version-Formatting
    /// </summary>
    /// <param name="version">Version to format</param>
    /// <param name="minTokens">Minimum number of component tokens of the version to display</param>
    /// <param name="maxTokens">Maximum number of component tokens of the version to display</param>
    public static string FormatVersion(this Version version, int minTokens = 2, int maxTokens = 2)
    {       
        if (minTokens < 1)
            minTokens = 1;
        if (minTokens > 4)
            minTokens = 4;
        if (maxTokens < minTokens)
            maxTokens = minTokens;
        if (maxTokens > 4)
            maxTokens = 4;

        var items = new [] { version.Major, version.Minor, version.Build, version.Revision };

        int tokens = maxTokens;
        while (tokens > minTokens && items[tokens - 1] == 0)
        {
            tokens--;
        }
        return version.ToString(tokens);
    }

    /// <summary>
    /// Strips off any pre-release or build metadata from a version string
    /// </summary>
    /// <param name="version">A vesion string that can contain meta data like -beta -preview1 etc. Anything after - is stripped</param>
    /// <returns></returns>
    public static string CleanupVersionString(this string version)
    {
        if (string.IsNullOrEmpty(version))
            return version;

        // Remove any pre-release or build metadata
        int dashIndex = version.IndexOf('-');
        if (dashIndex >= 0)
            version = version.Substring(0, dashIndex);

        return version;
    }


    /// <summary>
    /// Formats a version by stripping all zero values
    /// up to the trimTokens count provided. By default
    /// displays Major.Minor and then displays any
    /// Build and/or revision if non-zero	
    /// </summary>
    /// <param name="version">Version to format</param>
    /// <param name="minTokens">Minimum number of component tokens to display</param>
    /// <param name="maxTokens">Maximum number of component tokens to display</param>
    public static string FormatVersion(string version, int minTokens = 2, int maxTokens = 2)
    {
        version = version.CleanupVersionString();

        var ver = new Version(version);
        return ver.FormatVersion(minTokens, maxTokens);
    }


    /// <summary>
    /// Compare two version strings.
    /// </summary>
    /// <param name="versionToCompare">Semantic Version string</param>
    /// <param name="versionToCompareAgainst">Semantic Version string</param>
    /// <returns>0 - equal, 1 - greater than compareAgainst,  -1 - smaller than, -2  - Version Format error </returns>
    public static int CompareVersions(string versionToCompare, string versionToCompareAgainst)
    {
        if (string.IsNullOrEmpty(versionToCompare) || string.IsNullOrEmpty(versionToCompareAgainst))
            return -2;

        try
        {
            var v1 = new Version(versionToCompare.CleanupVersionString());
            var v2 = new Version(versionToCompareAgainst.CleanupVersionString());
            return v1.CompareTo(v2);
        }
        catch
        {
            return -2;
        }
    }
}