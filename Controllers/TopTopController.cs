using Firebase.Auth;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TopTopServer.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;

namespace TopTopServer.Controllers
{
    [EnableCors("CorsPolicy")]
    public class TopTopController : Controller
    {
        private readonly TopTopDBContext _context;
        private FirebaseAuthProvider authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyAeobHFw2yHBP0bgrRCTQMRyv6F3BKjnx8"));
        private readonly string AzureConnectionString = "DefaultEndpointsProtocol=https;AccountName=toptop;AccountKey=H00fR4sQvDMxlwnZbaOCZxaNvYDnWZt9YPojFdQG2yayGaS5NWuWJO3R8a//Fys7F/zmn7dLHWWv3I6ctXhi3A==;EndpointSuffix=core.windows.net";
        public TopTopController(TopTopDBContext context)
        {
            _context = context;
        }

        /*==============================================================================
                             SIGN UP NEW USER
        ================================================================================*/
        [HttpPost]
        [Route("SignUp")]
        public async Task<IActionResult> SignUp()
        {
            var email = Request.Form["email"];
            var password = Request.Form["password"];
            var nickname = Request.Form["nickname"];
            string defaultAvatar = "https://tleliteracy.com/wp-content/uploads/2017/02/default-avatar.png";
            bool isConflict = false;
            try
            {
                //var authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyAeobHFw2yHBP0bgrRCTQMRyv6F3BKjnx8"));
                await authProvider.CreateUserWithEmailAndPasswordAsync(email, password, nickname, true).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        isConflict = true;
                    }

                });
                if (!isConflict)
                {
                    _context.Users.Add(new Models.User(email, defaultAvatar, nickname, "", DateTime.Now, 0));
                    _context.SaveChanges();
                    return Ok();
                }
                else
                {
                    return Conflict();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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

        /*==============================================================================
                             GET A USER'S PROFILE BY EMAIL
        ================================================================================*/
        [HttpPost]
        [Route("FollowRequest")]
        public async Task<IActionResult> FollowRequest()
        {
            try
            {
                var request = HttpContext.Request;
                var followFrom = Request.Form["FollowFrom"];
                var followTo = Request.Form["FollowTo"];
                //send follow reuqest
                var followRequest = await _context.Follows.AddAsync(new Follow(followFrom, followTo));
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        /*==============================================================================
                             GET A USER'S PROFILE BY EMAIL
        ================================================================================*/
        [HttpPost]
        [Route("UnfollowRequest")]
        public IActionResult UnfollowRequest()
        {
            try
            {
                var request = HttpContext.Request;
                var unfollowFrom = Request.Form["UnfollowFrom"];
                var unfollowTo = Request.Form["UnfollowTo"];
                //send follow reuqest
                _context.Follows.Remove(new Follow(unfollowFrom, unfollowTo));
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        /*==============================================================================
                            UPDATE USER PROFILE WITH UPLOAD AVATAR
        ================================================================================*/
        [HttpPost]
        [Route("UpdateProfileWithUploadAvatar")]
        public async Task<IActionResult> UpdateProfileWithUploadAvatar()
        {
            try
            {
                var request = HttpContext.Request;
                var email = Request.Form["email"];
                var nickname = Request.Form["nickname"];
                var bio = Request.Form["bio"];
                var fileName = request.Form.Files[0].FileName;
                using (var memoryStream = new MemoryStream())
                {
                    request.Form.Files[0].CopyTo(memoryStream);
                    // Create a BlobServiceClient object which will be used to create a container client
                    BlobServiceClient blobServiceClient = new BlobServiceClient(AzureConnectionString);
                    // Create the container and return a container client object
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("toptop");
                    // Get a reference to a blob
                    BlobClient blobClient = containerClient.GetBlobClient(fileName);
                    // Upload data from the local file
                    memoryStream.Position = 0;
                    await blobClient.UploadAsync(memoryStream);
                    //Update database
                    var result = _context.Users.ToList().FirstOrDefault(user => user.Email == email);
                    result.NickName = nickname;
                    result.Bio = bio;
                    result.Avatar = blobClient.Uri.ToString();
                    _context.Update(result);
                    _context.SaveChanges();
                    return Ok();
                }
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        
        /*==============================================================================
                            UPDATE USER PROFILE WITHOUT UPLOAD AVATAR
        ================================================================================*/
        [HttpPost]
        [Route("UpdateProfileWithoutUploadAvatar")]
        public IActionResult UpdateProfileWithoutUploadAvatar()
        {
            try
            {
                var request = HttpContext.Request;
                var email = Request.Form["email"];
                var nickname = Request.Form["nickname"];
                var bio = Request.Form["bio"];
                var avatar = Request.Form["avatar"];
                var result = _context.Users.ToList().FirstOrDefault(user => user.Email == email);
                result.NickName = nickname;
                result.Bio = bio;
                result.Avatar = avatar;
                _context.Update(result);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        /*==============================================================================
                                        GET NEWEST VIDEO LIST
        ================================================================================*/
        [HttpPost]
        [Route("GetNewVideos")]
        public IActionResult GetNewVideos()
        {
            try
            {
                var request = HttpContext.Request;
                var email = Request.Form["email"];
                var result = _context.Videos.ToList().Where(video => video.Owner != email).OrderBy(video=>video.UploadDate);
                var likedVideos = _context.Likes.ToList().Where(like => like.User == email);
                List<VideosWithLikeState> LikedVideosList = new List<VideosWithLikeState>();
                foreach (Video video in result)
                {
                    if (likedVideos.Where(liked => liked.Video == video.Url).Count() > 0)
                    {
                        LikedVideosList.Add(new VideosWithLikeState(video, 1));
                    }
                    else
                    {
                        LikedVideosList.Add(new VideosWithLikeState(video, 0));
                    }
                }
                var response = JsonConvert.SerializeObject(LikedVideosList);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        /*==============================================================================
                            GET FOLLOWING'S NEWEST VIDEO LIST
        ================================================================================*/
        [HttpPost]
        [Route("GetNewFollowingVideos")]
        public IActionResult GetNewFollowingVideos()
        {
            try
            {
                var request = HttpContext.Request;
                var email = Request.Form["email"];
                var result = _context.Videos.ToList().Join
                    (
                        _context.Follows.ToList().Where(follow => follow.Follower == email),
                        video => video.Owner,
                        userfollow => userfollow.Following,
                        (video, userfollow) => video
                    ).ToList();
                var likedVideos = _context.Likes.ToList().Where(like => like.User == email);
                List<VideosWithLikeState> LikedVideosList = new List<VideosWithLikeState>();
                foreach(Video video in result)
                {
                    if(likedVideos.Where(liked=>liked.Video==video.Url).Count()>0)
                    {
                        LikedVideosList.Add(new VideosWithLikeState(video, 1));
                    } 
                    else
                    {
                        LikedVideosList.Add(new VideosWithLikeState(video, 0));
                    }    
                }    
                var response = JsonConvert.SerializeObject(LikedVideosList);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}
