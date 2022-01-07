using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TopTopServer.Models
{
    public class VideoWithOwnerDetails
    {
        public VideoWithOwnerDetails(VideosWithLikeState video, User user)
        {
            VideoDetails = video;
            OwnerDetails = user;
        }
        public VideosWithLikeState VideoDetails { get; set; }
        public User OwnerDetails { get; set; }
    }
}
