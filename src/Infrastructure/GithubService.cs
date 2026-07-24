using GithubProfileReadme.Core.Interfaces;
using GithubProfileReadme.Core.Models;
using Microsoft.Extensions.Logging;
using Octokit;

namespace GithubProfileReadme.Infrastructure;

/// <summary>
/// Implementation để fetch dữ liệu từ GitHub API
/// SRP: Chỉ phụ trách việc gọi GitHub API và map data
/// </summary>
public class GithubService : IGithubService
{
    private readonly ILogger<GithubService> _logger;
    private readonly GitHubClient _githubClient;
    private readonly string _username;

    public GithubService(ILogger<GithubService> logger)
    {
        _logger = logger;

        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("GITHUB_TOKEN environment variable is not set.");
        }

        _githubClient = new GitHubClient(new ProductHeaderValue("GithubProfileReadme"))
        {
            Credentials = new Credentials(token)
        };

        _username = Environment.GetEnvironmentVariable("GITHUB_USERNAME") ?? 
                    Environment.GetEnvironmentVariable("GITHUB_ACTOR") ??
                    throw new InvalidOperationException(
                        "GITHUB_USERNAME or GITHUB_ACTOR environment variable is not set.");
    }

    public async Task<GithubStats> GetGithubStatsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Đang lấy thống kê cho user: {Username}", _username);

        try
        {
            var user = await _githubClient.User.Get(_username);
            var stats = new GithubStats
            {
                Username = user.Login,
                TotalRepos = user.PublicRepos
            };

            var repos = new List<Repository>();
            var page = 1;
            const int perPage = 100;
            IReadOnlyList<Repository>? repoPage;
            
            do
            {
                repoPage = await _githubClient.Repository.GetAllForUser(_username, 
                    new ApiOptions { PageSize = perPage, PageCount = page });
                
                if (repoPage == null || !repoPage.Any())
                    break;
                    
                repos.AddRange(repoPage);
                page++;
            } while (repoPage.Count == perPage);

            stats.TotalStars = repos.Sum(r => r.StargazersCount);
            stats.TopLanguages = CalculateTopLanguages(repos);
            
            _logger.LogDebug("Tổng stars: {Stars}, Top languages: {LanguageCount}", 
                stats.TotalStars, stats.TopLanguages.Count);

            // Ước tính commits
            stats.CommitsThisYear = EstimateCommits(repos);

            _logger.LogInformation("Thành công lấy thống kê: {Repos} repos, {Stars} stars", 
                stats.TotalRepos, stats.TotalStars);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thống kê GitHub: {Message}", ex.Message);
            throw;
        }
    }

    private Dictionary<string, long> CalculateTopLanguages(IEnumerable<Repository> repos)
    {
        var languageStats = new Dictionary<string, long>();

        foreach (var repo in repos)
        {
            if (!string.IsNullOrWhiteSpace(repo.Language))
            {
                if (languageStats.ContainsKey(repo.Language))
                    languageStats[repo.Language]++;
                else
                    languageStats[repo.Language] = 1;
            }
        }

        return languageStats
            .OrderByDescending(kv => kv.Value)
            .Take(10)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private int EstimateCommits(List<Repository> repos)
    {
        try
        {
            var totalActivity = repos.Sum(r => r.ForksCount + r.WatchersCount);
            return Math.Max(totalActivity, repos.Count * 5);
        }
        catch
        {
            return repos.Count * 10;
        }
    }
}