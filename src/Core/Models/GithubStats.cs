namespace GithubProfileReadme.Core.Models;

/// <summary>
/// Model chứa thống kê GitHub của user
/// SRP: Chỉ chứa data, không chứa logic xử lý
/// </summary>
public class GithubStats
{
    /// <summary>
    /// Tổng số public repositories
    /// </summary>
    public int TotalRepos { get; set; }
    
    /// <summary>
    /// Tổng số stars đã nhận được
    /// </summary>
    public int TotalStars { get; set; }
    
    /// <summary>
    /// Số commits trong năm nay
    /// </summary>
    public int CommitsThisYear { get; set; }
    
    /// <summary>
    /// Danh sách các ngôn ngữ lập trình được sử dụng và số lượng
    /// Key: Tên ngôn ngữ, Value: Số bytes đã code
    /// </summary>
    public Dictionary<string, long> TopLanguages { get; set; } = new();
    
    /// <summary>
    /// Username của GitHub account
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Số lượng followers
    /// </summary>
    public int Followers { get; set; }

    /// <summary>
    /// Số lượng following
    /// </summary>
    public int Following { get; set; }

    /// <summary>
    /// Bio/Giới thiệu
    /// </summary>
    public string Bio { get; set; } = string.Empty;

    /// <summary>
    /// Website/Blog URL
    /// </summary>
    public string Blog { get; set; } = string.Empty;

    /// <summary>
    /// Vị trí địa lý
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Tổng số Pull Requests đã tạo
    /// </summary>
    public int TotalPullRequests { get; set; }

    /// <summary>
    /// Tổng số Issues đã tạo
    /// </summary>
    public int TotalIssues { get; set; }
}