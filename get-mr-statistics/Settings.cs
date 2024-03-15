using System.ComponentModel.DataAnnotations;

namespace get_mr_statistics;

public class Settings
{
    [Required]
    public string AccessToken { get; set; } = null!;

    [Required]
    public string FromDate { get; set; } = null!;

    public bool UseSlack { get; set; }

    public string SlackWebhookUrl { get; set; }

    public List<string> Projects { get; set; } = new();

    public List<string> IgnoreUsers { get; set; } = new();
}
