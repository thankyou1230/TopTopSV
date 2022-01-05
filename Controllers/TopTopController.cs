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
                var profile = await _context.Users.FindAsync(email);
                if (profile != null)
                    return Ok(profile);
                else
                    return NotFound();
                
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        /*==============================================================================
                             GET USER'S NUMBERS STATISTED BY EMAIL
        ================================================================================*/
        [HttpPost]
        [Route("GetUserNumbers")]
        public async Task<IActionResult> GetUserNumbers()
        {
            try
            {
                int followCount, followingCount, likeCount = 0;
                var request = HttpContext.Request;
                var email = Request.Form["email"];
                //like
                var videos = await _context.Videos.ToListAsync();
                likeCount = videos.Where(video => video.Owner == email).Sum(i => i.LikeCount);
                //follow
                var follows = await _context.Follows.ToListAsync();
                followCount = follows.Where(follow => follow.Following == email).Count();
                //following
                followingCount = follows.Where(follow => follow.Follower == email).Count();
                var response = JsonConvert.SerializeObject(new { Follow = followCount, Following = followingCount, Like = likeCount });
                return Ok(response);

            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        /*==============================================================================
                             GET A USER'S VIDEO LIST BY EMAIL
        ================================================================================*/
        [HttpPost]
        [Route("GetUserVideoList")]
        public async Task<IActionResult> GetUserVideoList()
        {
            try
            {
                var request = HttpContext.Request;
                var email = Request.Form["email"];
                var allVideos = await _context.Videos.ToListAsync();
                var videoList = allVideos.Where(video => video.Owner == email);
                if (videoList.FirstOrDefault() != null)
                {
                    return Ok(videoList);
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        /*==============================================================================
                             GET A USER'S LIKED VIDEO LIST BY EMAIL
        ================================================================================*/
        [HttpPost]
        [Route("GetUserLikedVideoList")]
        public async Task<IActionResult> GetUserLikedVideoList()
        {
            try
            {
                var request = HttpContext.Request;
                var email = Request.Form["email"];
                var allVideos = await _context.Likes.ToListAsync();
                var likedVideos = allVideos.Where(video => video.User == email);
                var likedVideosWithDetails = likedVideos.Join
                    (
                        _context.Videos,
                        like => like.Video,
                        video => video.Url,
                        (like, video) => new {video.Url,video.Owner,video.Title,video.LikeCount,video.CommentCount, video.UploadDate, video.IsPrivate}
                    );
                likedVideosWithDetails = likedVideosWithDetails.Where(video => video.IsPrivate == 0);
                if (likedVideosWithDetails.FirstOrDefault() != null)
                {
                    return Ok(likedVideosWithDetails);
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
