namespace GithubProfileReadme.Core.Interfaces;

/// <summary>
/// Interface để xử lý đọc/ghi file
/// SRP: Chỉ phụ trách việc đọc/ghi file từ filesystem
/// DIP: Phụ thuộc vào abstraction
/// </summary>
public interface IFileHandler
{
    /// <summary>
    /// Đọc nội dung ASCII art từ file
    /// </summary>
    /// <param name="filePath">Đường dẫn đến file ASCII art</param>
    /// <returns>Nội dung ASCII art</returns>
    Task<string> ReadAsciiArtAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đọc template SVG từ file
    /// </summary>
    /// <param name="filePath">Đường dẫn đến file template SVG</param>
    /// <returns>Nội dung template SVG</returns>
    Task<string> ReadSvgTemplateAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ghi SVG hoàn chỉnh ra file profile.svg
    /// </summary>
    /// <param name="content">Nội dung SVG hoàn chỉnh</param>
    /// <param name="outputPath">Đường dẫn output (mặc định là profile.svg ở thư mục gốc)</param>
    Task WriteProfileSvgAsync(string content, string outputPath = "profile.svg", CancellationToken cancellationToken = default);
}