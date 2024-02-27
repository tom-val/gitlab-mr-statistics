namespace get_mr_statistics.Models;

public class UserParticipationStats
{
    public List<int> ParticipatedAsAuthor { get; set; } = [];
    public int ParticipatedAsAuthorCount => ParticipatedAsAuthor.Distinct().Count();
    public List<int> ParticipatedAsCommenter { get; set; } = [];
    public int ParticipatedAsCommenterInMrsCount => ParticipatedAsCommenter.Distinct().Count();
    public IEnumerable<int> ParticipatedAsCommenterOnOwnMr => ParticipatedAsCommenter.Where(commenter => ParticipatedAsAuthor.Any(author => author == commenter));
    public IEnumerable<int> ParticipatedAsCommenterOnNotOwnMr => ParticipatedAsCommenter.Where(commenter => ParticipatedAsAuthor.All(author => author != commenter));
    public List<int> ParticipatedAsApprover { get; set; } = [];
    public int ParticipatedAsApproverCount => ParticipatedAsApprover.Distinct().Count();
    public int AllParticipatedMrs => ParticipatedAsAuthor.Union(ParticipatedAsCommenter).Union(ParticipatedAsApprover).Distinct().Count();
    public int AllParticipatedOtherMrs => ParticipatedAsApprover.Union(ParticipatedAsCommenterOnNotOwnMr).Distinct().Count();
}
