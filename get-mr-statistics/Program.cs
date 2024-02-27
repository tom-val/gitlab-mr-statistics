using System.Text.Json;
using get_mr_statistics;
using get_mr_statistics.Models;
using Microsoft.Extensions.Configuration;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}.json", optional: true)
    .Build();

var settings = config.Get<Settings>();

var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
};

using var client = new HttpClient();
client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", settings.AccessToken);

var mergeRequestStats = new List<MergeRequestStats>();
foreach (var projectId in settings.Projects)
{
    var pullRequestsResponse = await client.GetAsync($"https://gitlab.com/api/v4/projects/{projectId}/merge_requests?state=all&per_page=100&scope=all&updated_after={settings.FromDate}");
    var pullRequestsJsonResponse = await pullRequestsResponse.Content.ReadAsStreamAsync();
    var mergeRequests = JsonSerializer.Deserialize<List<MergeRequest>>(pullRequestsJsonResponse, jsonOptions);
    CheckPage(pullRequestsResponse);

    foreach (var mergeRequest in mergeRequests!.Where(mr => settings.IgnoreUsers.All(u => u != mr.Author.Username)))
    {
        Console.WriteLine($"Gathering project {projectId} MR {mergeRequest.Iid} information");
        var approvalStateResponse = await client.GetAsync($"https://gitlab.com/api/v4/projects/{projectId}/merge_requests/{mergeRequest.Iid}/approval_state");
        var approvalStateJsonResponse = await approvalStateResponse.Content.ReadAsStreamAsync();
        var approvalState = JsonSerializer.Deserialize<ApprovalState>(approvalStateJsonResponse, jsonOptions);

        var allComments = new List<Note>();
        var totalItems = -1;
        var page = 1;
        do
        {
            var commentsResponse = await client.GetAsync($"https://gitlab.com/api/v4/projects/{projectId}/merge_requests/{mergeRequest.Iid}/notes?per_page=100&page={page}");
            totalItems = int.Parse(commentsResponse.Headers.GetValues("x-total").First());
            var commentsJsonResponse = await commentsResponse.Content.ReadAsStreamAsync();
            allComments.AddRange(JsonSerializer.Deserialize<List<Note>>(commentsJsonResponse, jsonOptions)!);
            page++;
        } while(allComments.Count != totalItems);

        var comments = allComments.Where(mr => settings.IgnoreUsers.All(u => u != mr.Author.Username) && !mr.System);

        var approvedBy = approvalState!.Rules.SelectMany(a => a.ApprovedBy.Select(ab => ab.Username)).Where(approver => settings.IgnoreUsers.All(u => u != approver)).ToArray();
        mergeRequestStats.Add(new MergeRequestStats(projectId, mergeRequest.Iid, mergeRequest.Author.Username, approvedBy, comments.Select(c => c.Author.Username).ToArray()));
    }
}

Console.WriteLine($"Total merge requests: {mergeRequestStats.Count}");
Console.WriteLine("------------------------------------------------------------------------");
Console.WriteLine();

var participationStats = new Dictionary<string, UserParticipationStats>();
foreach (var mergeRequestStat in mergeRequestStats)
{
    var authorParticipant = participationStats.GetOrCreateEmpty(mergeRequestStat.CreatedBy);
    authorParticipant.ParticipatedAsAuthor.Add(mergeRequestStat.Iid);

    foreach (var approve in mergeRequestStat.ApprovedBy)
    {
        var approverParticipant = participationStats.GetOrCreateEmpty(approve);
        approverParticipant.ParticipatedAsApprover.Add(mergeRequestStat.Iid);
    }

    foreach (var comment in mergeRequestStat.CommentedBy)
    {
        var commentParticipant = participationStats.GetOrCreateEmpty(comment);
        commentParticipant.ParticipatedAsCommenter.Add(mergeRequestStat.Iid);
    }
}

foreach (var user in participationStats)
{
    Console.WriteLine($"{user.Key} statistics from {settings.FromDate}");
    Console.WriteLine($"Created MR's: {user.Value.ParticipatedAsAuthorCount}");
    Console.WriteLine($"Wrote comments: {user.Value.ParticipatedAsCommenter.Count}");
    Console.WriteLine($"Wrote comments in MR's count: {user.Value.ParticipatedAsCommenterInMrsCount}");
    Console.WriteLine($"Wrote comments on own MR's: {user.Value.ParticipatedAsCommenterOnOwnMr.Count()}");
    Console.WriteLine($"Wrote comments on other people MR's: {user.Value.ParticipatedAsCommenterOnNotOwnMr.Count()}");
    Console.WriteLine($"Approved MR's: {user.Value.ParticipatedAsApproverCount}");
    Console.WriteLine($"Participated in total MR's: {user.Value.AllParticipatedMrs}");
    Console.WriteLine($"Participation percentage: {(double)user.Value.AllParticipatedMrs / (double)mergeRequestStats.Count:P}");
    Console.WriteLine($"Participation percentage in other people MR's: {(double)user.Value.AllParticipatedOtherMrs  / (double)(mergeRequestStats.Count - user.Value.ParticipatedAsAuthorCount):P}");
    Console.WriteLine("------------------------------------------------------------------------");
    Console.WriteLine();
}

Console.WriteLine("Finished");


void CheckPage(HttpResponseMessage response)
{
    var page = response.Headers.GetValues("x-total-pages").First();
    if (page != "1")
    {
        Console.WriteLine($"!!! Need to add pagination, information may be incorrect. Request: {response.RequestMessage!.RequestUri} !!!");
        throw new Exception("Need to add pagination!");
    }
}
