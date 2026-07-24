using GithubProfileReadme.Core.Models;

namespace GithubProfileReadme.Core.Interfaces;

/// <summary>
/// Interface để lấy thống kê từ GitHub API
/// SRP: Chỉ phụ trách việc fetch data từ GitHub
/// DIP: Phụ thuộc vào abstraction thay vì concrete implementation
/// </summary>
public interface IGithubService
{
    /// <summary>
    /// Lấy thống kê GitHub của user được chỉ định
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ request</param>
    /// <returns>Thống kê GitHub của user</returns>
    Task<GithubStats> GetGithubStatsAsync(CancellationToken cancellationToken = default);
}