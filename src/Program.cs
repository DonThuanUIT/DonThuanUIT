using GithubProfileReadme.Core.Interfaces;
using GithubProfileReadme.Core.Models;
using GithubProfileReadme.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ============================================
// DEPENDENCY INJECTION CONTAINER
// Tuân thủ DIP: Phụ thuộc vào abstraction, không phụ thuộc vào concrete implementation
// ============================================
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Cấu hình Options nếu cần (ví dụ: GitHub API settings)
        
        // Đăng ký Services
        services.AddSingleton<IFileHandler, FileHandler>();
        services.AddSingleton<ISvgGenerator, SvgGenerator>();
        services.AddSingleton<IGithubService, GithubService>();
        
        // Đăng ký Hosted Service để chạy khi ứng dụng khởi động
        services.AddHostedService<AppService>();
    })
    .Build();

// Chạy ứng dụng
await host.RunAsync();


// ============================================
// APPSERVICE - Orchestrator cho toàn bộ luồng xử lý
// Chịu trách nhiệm điều phối các service theo thứ tự
// ============================================
public class AppService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AppService> _logger;

    public AppService(IServiceProvider serviceProvider, ILogger<AppService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ứng dụng bắt đầu chạy lúc: {Time}", DateTime.UtcNow);

        try
        {
            // Tạo scope để lấy các service scoped/singleton
            using var scope = _serviceProvider.CreateScope();
            
            var fileHandler = scope.ServiceProvider.GetRequiredService<IFileHandler>();
            var svgGenerator = scope.ServiceProvider.GetRequiredService<ISvgGenerator>();
            var githubService = scope.ServiceProvider.GetRequiredService<IGithubService>();

            // Bước 1: Đọc các file assets (ASCII art và SVG template)
            _logger.LogInformation("Đang đọc file assets...");
            var asciiArt = await fileHandler.ReadAsciiArtAsync("Assets/ascii_art.txt");
            var svgTemplate = await fileHandler.ReadSvgTemplateAsync("Assets/template.svg");

            // Bước 2: Lấy dữ liệu từ GitHub API
            _logger.LogInformation("Đang lấy dữ liệu từ GitHub API...");
            var githubStats = await githubService.GetGithubStatsAsync();
            
            _logger.LogInformation("Thông tin GitHub: {Repos} repos, {Stars} stars, {Commits} commits", 
                githubStats.TotalRepos, 
                githubStats.TotalStars, 
                githubStats.CommitsThisYear);

            // Bước 3: Generate SVG với dữ liệu đã có
            _logger.LogInformation("Đang generate SVG...");
            var finalSvg = svgGenerator.GenerateSvg(svgTemplate, asciiArt, githubStats);

            // Bước 4: Ghi file profile.svg
            _logger.LogInformation("Đang ghi file profile.svg...");
            await fileHandler.WriteProfileSvgAsync(finalSvg);

            _logger.LogInformation("Hoàn thành! Đã tạo profile.svg thành công.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Có lỗi xảy ra trong quá trình xử lý: {Message}", ex.Message);
            // Re-throw để GitHub Actions biết có lỗi
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ứng dụng dừng lúc: {Time}", DateTime.UtcNow);
        return Task.CompletedTask;
    }
}