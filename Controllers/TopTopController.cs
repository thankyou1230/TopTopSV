using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TopTopServer.Models;

namespace TopTopServer.Controllers
{
    [EnableCors("CorsPolicy")]
    public class TopTopController : Controller
    {
        private readonly TopTopDBContext _context;

        public TopTopController(TopTopDBContext context)
        {
            _context = context;
        }

        /*==============================================================================
                             GET A USER'S PROFILE BY EMAIL
        ================================================================================*/
        [HttpPost]
        [Route("GetUserProfile")]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                var request = HttpContext.Request;
                var email = Request.Form["email"];
                //main Profile
                var profile = await _context.Users.FindAsync(email);
                //follower
                var followersList = await _context.Follows.ToListAsync();
                var userFollowers = followersList.Where(follow => follow.Following == email);
                var userFollowersProfiles = userFollowers.Join
                    (
                        _context.Users,
                        follow => follow.Follower,
                        user => user.Email,
                        (follow,user) => user
                    );
                //following
                var followingsList = await _context.Follows.ToListAsync();
                var userFollowings = followingsList.Where(follow => follow.Follower == email);
                var userFollowingsProfiles = userFollowings.Join
                    (
                        _context.Users,
                        follow => follow.Following,
                        user => user.Email,
                        (follow, user) => user
                    );
                //like Count
                var videos = await _context.Videos.ToListAsync();
                var likeCount = videos.Where(video => video.Owner == email).Sum(i => i.LikeCount);
                //user's video
                var allVideos = await _context.Videos.ToListAsync();
                var videosList = allVideos.Where(video => video.Owner == email);
                //Liked video
                var likesList = await _context.Likes.ToListAsync();
                var likedVideos = likesList.Where(like => like.User == email);
                var likedVideosWithDetails = likedVideos.Join
                    (
                        _context.Videos,
                        like => like.Video,
                        video => video.Url,
                        (like, video) => video
                    )
                    .Where(video => video.IsPrivate == 0);
                var response = JsonConvert.SerializeObject(new {MainProfile=profile, Followers=userFollowersProfiles, Followings=userFollowingsProfiles, LikeCount=likeCount, Videos=videosList, LikedVideos=likedVideosWithDetails });
                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
