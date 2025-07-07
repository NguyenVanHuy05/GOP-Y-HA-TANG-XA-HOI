using System;

namespace BTL.DNU.IdeaSpark.Web.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public int IdeaId { get; set; }
        public string CommenterEmail { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CommentedAt { get; set; } = DateTime.Now;
        public string AuthorEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public User User { get; set; } 
        public Idea? Idea { get; set; }
        public int UserId { get; set; }
        public int? ParentCommentId { get; set; }
        public Comment ParentComment { get; set; }
        public List<Comment> Replies { get; set; }

        public int LikeCount { get; set; } = 0;
    }
}