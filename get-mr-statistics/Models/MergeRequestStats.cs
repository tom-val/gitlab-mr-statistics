namespace get_mr_statistics.Models;

public record MergeRequestStats(string ProjectId, int Iid, string CreatedBy, string[] ApprovedBy, string[] CommentedBy);