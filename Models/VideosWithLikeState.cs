using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TopTopServer.Models
{
    public class VideosWithLikeState
    {
        public VideosWithLikeState(Video video, string liked)
        {
            this.Url = video.Url;
            this.Owner = video.Owner;
            this.Title = video.Title;
            this.UploadDate = video.UploadDate;
            this.IsPrivate = video.IsPrivate;
            this.LikeCount = video.LikeCount;
            this.CommentCount = video.CommentCount;
            this.IsLiked = liked;
            this.ThumbnailUrl = video.ThumbnailUrl;
        }
        public string Url { get; set; }
        public string Owner { get; set; }
        public string Title { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public DateTime UploadDate { get; set; }
        public string ThumbnailUrl { get; set; }
        public int IsPrivate { get; set; }
        public string IsLiked { get; set; }
    }
}
