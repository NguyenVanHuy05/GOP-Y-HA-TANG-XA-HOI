using BTL.DNU.IdeaSpark.Web.Models;

namespace BTL.DNU.IdeaSpark.Models;

public class Vote
{
    public int VoteId { get; set; }
    public int IdeaId { get; set; }
    public string UserEmail { get; set; }

    public DateTime VotedAt { get; set; }

    public Idea Idea { get; set; }  // navigation
}
