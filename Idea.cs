using System;
using System.Collections.Generic;

namespace BTL.DNU.IdeaSpark.Web.Models
{
    public class Idea
    {
        public bool? IsApproved { get; set; }  // null = chưa duyệt, true = duyệt, false = từ chối

        // Nhãn trạng thái
        public string? StatusLabel { get; set; } 
        public int IdeaId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsAnonymous { get; set; }
        public string? SubmitterEmail { get; set; }
        public string Status { get; set; } = "Received";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int VoteCount { get; set; } = 0;
        public string? Content { get; set; } 
        public List<Comment>? Comments { get; set; }
        public string? ImageUrl { get; set; }
       
    }
}