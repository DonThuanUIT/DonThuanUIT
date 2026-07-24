using GithubProfileReadme.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GithubProfileReadme.Infrastructure;

/// <summary>
/// Implementation để đọc/ghi file từ filesystem
/// SRP: Chỉ phụ trách việc đọc/ghi file
/// </summary>
public class FileHandler : IFileHandler
{
    private readonly ILogger<FileHandler> _logger;

    public FileHandler(ILogger<FileHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Đọc nội dung ASCII art từ file
    /// </summary>
    public async Task<string> ReadAsciiArtAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Đường dẫn file ASCII art không được null hoặc empty", nameof(filePath));
        }

        try
        {
            // Thử các đường dẫn khác nhau để phù hợp với cả lúc chạy local và trong GitHub Actions
            var possiblePaths = new[]
            {
                filePath,
                Path.Combine(Directory.GetCurrentDirectory(), filePath),
                Path.Combine(Directory.GetCurrentDirectory(), "Assets", Path.GetFileName(filePath))
            };

            string? fullPath = possiblePaths.FirstOrDefault(File.Exists);

            if (fullPath == null)
            {
                throw new FileNotFoundException(
                    $"Không tìm thấy file ASCII art tại các đường dẫn: {string.Join(", ", possiblePaths)}");
            }

            _logger.LogInformation("Đang đọc ASCII art từ: {Path}", fullPath);
            var content = await File.ReadAllTextAsync(fullPath, cancellationToken);
            
            _logger.LogInformation("Đã đọc thành công ASCII art với {Length} ký tự", content.Length);
            return content;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Không tìm thấy file ASCII art: {Path}", filePath);
            throw;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Lỗi đọc file ASCII art: {Message}", ex.Message);
            throw new InvalidOperationException($"Không thể đọc file ASCII art: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi không xác định khi đọc ASCII art: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Đọc template SVG từ file
    /// </summary>
    public async Task<string> ReadSvgTemplateAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Đường dẫn file SVG template không được null hoặc empty", nameof(filePath));
        }

        try
        {
            // Thử các đường dẫn khác nhau để phù hợp với cả lúc chạy local và trong GitHub Actions
            var possiblePaths = new[]
            {
                filePath,
                Path.Combine(Directory.GetCurrentDirectory(), filePath),
                Path.Combine(Directory.GetCurrentDirectory(), "Assets", Path.GetFileName(filePath))
            };

            string? fullPath = possiblePaths.FirstOrDefault(File.Exists);

            if (fullPath == null)
            {
                throw new FileNotFoundException(
                    $"Không tìm thấy file SVG template tại các đường dẫn: {string.Join(", ", possiblePaths)}");
            }

            _logger.LogInformation("Đang đọc SVG template từ: {Path}", fullPath);
            var content = await File.ReadAllTextAsync(fullPath, cancellationToken);
            
            _logger.LogInformation("Đã đọc thành công SVG template với {Length} ký tự", content.Length);
            return content;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Không tìm thấy file SVG template: {Path}", filePath);
            throw;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Lỗi đọc file SVG template: {Message}", ex.Message);
            throw new InvalidOperationException($"Không thể đọc file SVG template: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi không xác định khi đọc SVG template: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Ghi SVG hoàn chỉnh ra file profile.svg (ở thư mục gốc của repository)
    /// </summary>
    public async Task WriteProfileSvgAsync(string content, string outputPath = "profile.svg", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentException("Đường dẫn output không được null hoặc empty", nameof(outputPath));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Nội dung SVG không được null hoặc empty", nameof(content));
        }

        try
        {
            // Nếu đang ở thư mục src/, ghi lên thư mục parent
            var currentDir = Directory.GetCurrentDirectory();
            string fullPath;
            
            if (Path.GetFileName(currentDir).Equals("src", StringComparison.OrdinalIgnoreCase))
            {
                fullPath = Path.Combine(Path.GetDirectoryName(currentDir)!, outputPath);
            }
            else
            {
                fullPath = Path.Combine(currentDir, outputPath);
            }

            _logger.LogInformation("Đang ghi SVG ra file: {Path}", fullPath);

            // Tạo thư mục nếu chưa tồn tại
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogInformation("Đã tạo thư mục: {Directory}", directory);
            }

            // Ghi file
            await File.WriteAllTextAsync(fullPath, content, cancellationToken);

            // Xác minh file đã được tạo
            var fileInfo = new FileInfo(fullPath);
            _logger.LogInformation("Đã ghi thành công profile.svg: {Size} bytes, thời gian: {Time}", 
                fileInfo.Length, fileInfo.LastWriteTime);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Lỗi ghi file SVG: {Message}", ex.Message);
            throw new InvalidOperationException($"Không thể ghi file SVG: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi không xác định khi ghi file SVG: {Message}", ex.Message);
            throw;
        }
    }
}