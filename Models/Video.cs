using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopTopServer.Models
{
    [Table("Video")]
    public class Video
    {
        public Video(string url, string owner, string title, DateTime uploadDate,string ThumbnailUrl, int isPrivate, int likeCount = 0, int commentCount = 0)
        {
            this.Url = url;
            this.Owner = owner;
            this.Title = title;
            this.UploadDate = uploadDate;
            this.IsPrivate = isPrivate;
            this.LikeCount = likeCount;
            this.CommentCount = commentCount;
            this.ThumbnailUrl = ThumbnailUrl;
        }
        [Key]
        public string Url { get; set; }
        public string Owner { get; set; }
        public string Title { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public DateTime UploadDate { get; set; }
        public string ThumbnailUrl { get; set; }
        public int IsPrivate { get; set; }
    }
}
