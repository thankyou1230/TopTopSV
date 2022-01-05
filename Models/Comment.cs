using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopTopServer.Models
{
    [Table("Comment")]
    public class Comment
    {
        public Comment(string user, string video, string CommentContent, DateTime commentTime)
        {
            this.User = user;
            this.Video = video;
            this.CommentContent = CommentContent;
            this.CommentTime = commentTime;
        }

        public string User { get; set; }
        public string Video { get; set; }
        public string CommentContent { get; set; }
        public DateTime CommentTime { get; set; }
    }
}
