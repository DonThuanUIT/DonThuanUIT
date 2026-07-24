using GithubProfileReadme.Core.Interfaces;
using GithubProfileReadme.Core.Models;
using Microsoft.Extensions.Logging;

namespace GithubProfileReadme.Infrastructure;

/// <summary>
/// Implementation để generate SVG từ template và data
/// SRP: Chỉ phụ trách việc thay thế placeholder trong SVG template
/// </summary>
public class SvgGenerator : ISvgGenerator
{
    private readonly ILogger<SvgGenerator> _logger;

    public SvgGenerator(ILogger<SvgGenerator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generate SVG hoàn chỉnh bằng cách thay thế các placeholder
    /// </summary>
    public string GenerateSvg(string svgTemplate, string asciiArt, GithubStats stats)
    {
        if (string.IsNullOrWhiteSpace(svgTemplate))
        {
            throw new ArgumentException("SVG template không được null hoặc empty", nameof(svgTemplate));
        }

        if (asciiArt == null)
        {
            throw new ArgumentNullException(nameof(asciiArt), "ASCII art không được null");
        }

        if (stats == null)
        {
            throw new ArgumentNullException(nameof(stats), "GithubStats không được null");
        }

        try
        {
            var result = svgTemplate;

            // Thay thế các placeholder với actual data
            result = result.Replace("{USERNAME}", EscapeXml(stats.Username));
            result = result.Replace("{TOTAL_REPOS}", stats.TotalRepos.ToString());
            result = result.Replace("{STARS}", stats.TotalStars.ToString());
            result = result.Replace("{COMMITS_THIS_YEAR}", stats.CommitsThisYear.ToString());
            result = result.Replace("{ASCII_ART_CONTENT}", EscapeXml(asciiArt));

            // Thay thế các placeholder cho languages
            if (stats.TopLanguages != null && stats.TopLanguages.Any())
            {
                var languageString = string.Join(", ", stats.TopLanguages.Keys.Take(5));
                result = result.Replace("{TOP_LANGUAGES}", EscapeXml(languageString));

                // Có thể thêm các placeholder chi tiết hơn nếu cần
                var languagesDetail = string.Join("\n", stats.TopLanguages
                    .Take(5)
                    .Select(kv => $"{kv.Key}: {kv.Value:#,##0}"));
                
                // Nếu template có placeholder chi tiết
                if (result.Contains("{TOP_LANGUAGES_DETAIL}"))
                {
                    result = result.Replace("{TOP_LANGUAGES_DETAIL}", EscapeXml(languagesDetail));
                }
            }
            else
            {
                result = result.Replace("{TOP_LANGUAGES}", "N/A");
                if (result.Contains("{TOP_LANGUAGES_DETAIL}"))
                {
                    result = result.Replace("{TOP_LANGUAGES_DETAIL}", "N/A");
                }
            }

            // Thay thế timestamp để biết khi nào được update
            var currentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm UTC");
            if (result.Contains("{UPDATE_TIME}"))
            {
                result = result.Replace("{UPDATE_TIME}", EscapeXml(currentTime));
            }

            _logger.LogInformation("Đã generate SVG thành công với {Placeholders} placeholders đã được thay thế", 
                CountPlaceholders(svgTemplate));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi generate SVG: {Message}", ex.Message);
            throw new InvalidOperationException("Không thể tạo SVG từ template", ex);
        }
    }

    /// <summary>
    /// Escape XML characters để đảm bảo SVG hợp lệ
    /// </summary>
    private string EscapeXml(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var result = input
            .Replace("&", "&")
            .Replace("<", "<")
            .Replace(">", ">")
            .Replace(""", """)
            .Replace("'", "'");
        
        return result;
    }

    /// <summary>
    /// Đếm số lượng placeholder trong template (để log)
    /// </summary>
    private int CountPlaceholders(string template)
    {
        if (string.IsNullOrEmpty(template))
            return 0;

        return template.Count(c => c == '{' && template.IndexOf('}', template.IndexOf(c)) > -1);
    }
}