using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KnowledgeStack.Web.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(32)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(128)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    }

    public class Post
    {
        public int Id { get; set; }

        [Required, MaxLength(128)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty; // Markdown

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
        public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    }

    public class Comment
    {
        public int Id { get; set; }
        [Required]
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User? User { get; set; }

        public int PostId { get; set; }
        public Post? Post { get; set; }
    }

    public class Tag
    {
        public int Id { get; set; }
        [Required, MaxLength(32)]
        public string Name { get; set; } = string.Empty;
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }

    public class PostTag
    {
        public int PostId { get; set; }
        public Post? Post { get; set; }
        public int TagId { get; set; }
        public Tag? Tag { get; set; }
    }

    public class PostLike
    {
        public int UserId { get; set; }
        public User? User { get; set; }
        public int PostId { get; set; }
        public Post? Post { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}



