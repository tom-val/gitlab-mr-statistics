namespace get_mr_statistics.Models;

public record ApprovalRules(bool Approved, List<ApprovedBy> ApprovedBy);
