using GithubProfileReadme.Core.Interfaces;
using GithubProfileReadme.Core.Models;
using GithubProfileReadme.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ============================================
// DEPENDENCY INJECTION CONTAINER
// ============================================
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IFileHandler, FileHandler>();
        services.AddSingleton<ISvgGenerator, SvgGenerator>();
        services.AddSingleton<IGithubService, GithubService>();
    })
    .Build();

// Chạy ứng dụng
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Ứng dụng bắt đầu chạy lúc: {Time}", DateTime.UtcNow);

try
{
    await using var scope = host.Services.CreateAsyncScope();
    
    var fileHandler = scope.ServiceProvider.GetRequiredService<IFileHandler>();
    var svgGenerator = scope.ServiceProvider.GetRequiredService<ISvgGenerator>();
    var githubService = scope.ServiceProvider.GetRequiredService<IGithubService>();

    logger.LogInformation("Đang đọc template SVG...");
    var svgTemplate = await fileHandler.ReadSvgTemplateAsync("Assets/template.svg");

    logger.LogInformation("Đang lấy dữ liệu từ GitHub API...");
    var githubStats = await githubService.GetGithubStatsAsync();
    
    logger.LogInformation("Thông tin GitHub: {Repos} repos, {Stars} stars, {Commits} commits, {Followers} followers", 
        githubStats.TotalRepos, 
        githubStats.TotalStars, 
        githubStats.CommitsThisYear,
        githubStats.Followers);

    logger.LogInformation("Đang generate SVG...");
    var finalSvg = svgGenerator.GenerateSvg(svgTemplate, githubStats);

    logger.LogInformation("Đang ghi file profile.svg...");
    await fileHandler.WriteProfileSvgAsync(finalSvg);

    logger.LogInformation("Hoàn thành! Đã tạo profile.svg thành công.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Có lỗi xảy ra: {Message}", ex.Message);
    throw;
}
finally
{
    logger.LogInformation("Ứng dụng dừng lúc: {Time}", DateTime.UtcNow);
}