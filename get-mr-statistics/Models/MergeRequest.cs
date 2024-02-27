namespace get_mr_statistics.Models;

public record MergeRequest
{
    public int Id { get; set; }
    public int Iid { get; set; }
    public string Title { get; set; }
    public Author Author { get; set; }
}