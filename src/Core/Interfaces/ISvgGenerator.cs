using GithubProfileReadme.Core.Models;

namespace GithubProfileReadme.Core.Interfaces;

/// <summary>
/// Interface để generate SVG từ template
/// SRP: Chỉ phụ trách việc tạo SVG content từ template
/// DIP: Phụ thuộc vào abstraction
/// </summary>
public interface ISvgGenerator
{
    /// <summary>
    /// Generate SVG hoàn chỉnh từ template và thống kê GitHub
    /// </summary>
    /// <param name="svgTemplate">Chuỗi SVG template</param>
    /// <param name="stats">Thống kê GitHub</param>
    /// <returns>Chuỗi SVG hoàn chỉnh</returns>
    string GenerateSvg(string svgTemplate, GithubStats stats);
}
