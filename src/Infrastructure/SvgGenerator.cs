using GithubProfileReadme.Core.Interfaces;
using GithubProfileReadme.Core.Models;
using Microsoft.Extensions.Logging;
using System.Security;

namespace GithubProfileReadme.Infrastructure;

public class SvgGenerator : ISvgGenerator
{
    private readonly ILogger<SvgGenerator> _logger;

    public SvgGenerator(ILogger<SvgGenerator> logger)
    {
        _logger = logger;
    }

    public string GenerateSvg(string svgTemplate, string asciiArt, GithubStats stats)
    {
        if (string.IsNullOrWhiteSpace(svgTemplate))
            throw new ArgumentException("SVG template không được null hoặc empty", nameof(svgTemplate));
        if (asciiArt == null)
            throw new ArgumentNullException(nameof(asciiArt));
        if (stats == null)
            throw new ArgumentNullException(nameof(stats));

        try
        {
            var result = svgTemplate;

            result = result.Replace("{USERNAME}", EscapeXml(stats.Username));
            result = result.Replace("{TOTAL_REPOS}", stats.TotalRepos.ToString());
            result = result.Replace("{STARS}", stats.TotalStars.ToString());
            result = result.Replace("{COMMITS_THIS_YEAR}", stats.CommitsThisYear.ToString());
            result = result.Replace("{ASCII_ART_CONTENT}", EscapeXml(asciiArt));

            if (stats.TopLanguages != null && stats.TopLanguages.Any())
            {
                var languageString = string.Join(", ", stats.TopLanguages.Keys.Take(5));
                result = result.Replace("{TOP_LANGUAGES}", EscapeXml(languageString));
            }
            else
            {
                result = result.Replace("{TOP_LANGUAGES}", "N/A");
            }

            var currentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm UTC");
            result = result.Replace("{UPDATE_TIME}", EscapeXml(currentTime));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Loi khi generate SVG: {Message}", ex.Message);
            throw new InvalidOperationException("Khong the tao SVG tu template", ex);
        }
    }

    private string EscapeXml(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        try
        {
            return System.Security.SecurityElement.Escape(input) ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}