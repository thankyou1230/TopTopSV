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
using User = TopTopServer.Models.User;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace TopTopServer.Controllers
{
    [EnableCors("CorsPolicy")]
    public class TopTopController : Controller
    {
        private readonly TopTopDBContext _context;
        private FirebaseAuthProvider authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyAeobHFw2yHBP0bgrRCTQMRyv6F3BKjnx8"));
        private readonly string AzureConnectionString = "DefaultEndpointsProtocol=https;AccountName=toptop2;AccountKey=Sc9YQKUCsoya0vINewPQHN8MjlnjjF0z8K42xGdN6ZthKPY2QxJde9FFDG+wFvfHbXxyQkV6PcVj4W0kYf7HWA==;EndpointSuffix=core.windows.net";
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
                                        GET SPECIFED VIDEO
        ================================================================================*/
        [HttpPost]
        [Route("GetSpecifedVideo")]
        public IActionResult GetSpecifedVideo()
        {
            try
            {
                var request = HttpContext.Request;
                var email = Request.Form["email"];
                var videoUrl = Request.Form["videourl"];
                var result = _context.Videos.ToList().Where(video => video.Url == videoUrl);
                var likedVideos = _context.Likes.ToList().Where(like => like.User == email);
                var userList = _context.Users.ToList();
                List<VideoWithOwnerDetails> LikedVideosList = new List<VideoWithOwnerDetails>();
                foreach (Video video in result)
                {
                    User ownerDetails = userList.Where(user => user.Email == video.Owner).FirstOrDefault();
                    if (likedVideos.Where(liked => liked.Video == video.Url).Count() > 0)
                    {
                        LikedVideosList.Add(new VideoWithOwnerDetails(new VideosWithLikeState(video, "True"), ownerDetails));
                    }
                    else
                    {
                        LikedVideosList.Add(new VideoWithOwnerDetails(new VideosWithLikeState(video, "False"), ownerDetails));
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
                var result = _context.Videos.ToList().Where(video => video.Owner != email && video.IsPrivate==0).OrderByDescending(video=>video.UploadDate);
                var likedVideos = _context.Likes.ToList().Where(like => like.User == email);
                var userList = _context.Users.ToList();
                List<VideoWithOwnerDetails> LikedVideosList = new List<VideoWithOwnerDetails>();
                foreach (Video video in result)
                {
                    User ownerDetails = userList.Where(user => user.Email == video.Owner).FirstOrDefault();
                    if (likedVideos.Where(liked => liked.Video == video.Url).Count() > 0)
                    {
                        LikedVideosList.Add(new VideoWithOwnerDetails(new VideosWithLikeState(video, "True"), ownerDetails));
                    }
                    else
                    {
                        LikedVideosList.Add(new VideoWithOwnerDetails(new VideosWithLikeState(video, "False"), ownerDetails));
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
                //Video details
                var result = _context.Videos.ToList().Where(video=>video.IsPrivate==0).Join
                    (
                        _context.Follows.ToList().Where(follow => follow.Follower == email),
                        video => video.Owner,
                        userfollow => userfollow.Following,
                        (video, userfollow) => video
                    ).ToList();
                var likedVideos = _context.Likes.ToList().Where(like => like.User == email);
                var userList = _context.Users.ToList();
                List<VideoWithOwnerDetails> LikedVideosList = new List<VideoWithOwnerDetails>();
                foreach(Video video in result)
                {
                    User ownerDetails = userList.Where(user => user.Email == video.Owner).FirstOrDefault();
                    if(likedVideos.Where(liked=>liked.Video==video.Url).Count()>0)
                    {
                        LikedVideosList.Add(new VideoWithOwnerDetails(new VideosWithLikeState(video, "True"),ownerDetails));
                    } 
                    else
                    {
                        LikedVideosList.Add(new VideoWithOwnerDetails(new VideosWithLikeState(video, "False"),ownerDetails));
                    }    
                }    
                //Owner Details
                var response = JsonConvert.SerializeObject(LikedVideosList);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        /*==============================================================================
                            GET VIDEO'S COMMENTS
        ================================================================================*/
        [HttpPost]
        [Route("GetVideoComments")]
        public IActionResult GetVideoComments()
        {
            try
            {
                var request = HttpContext.Request;
                var videoUrl = Request.Form["videourl"];
                //Comment details
                var comments = _context.Comments.ToList().Where(comment => comment.Video == videoUrl).OrderByDescending(video=>video.CommentTime);
                var result = comments.Join
                    (
                        _context.Users,
                        comment => comment.User,
                        user => user.Email,
                        (comment, user) => new { comment, user}
                    );
                var response = JsonConvert.SerializeObject(result);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        /*==============================================================================
                            ADD VIDEO'S COMMENT
        ================================================================================*/
        [HttpPost]
        [Route("AddComment")]
        public async Task<IActionResult> AddComment()
        {
            try
            {
                var request = HttpContext.Request;
                var commenter = Request.Form["commenter"];
                var videourl = Request.Form["videourl"];
                var content = Request.Form["content"];
                var isOwner = Request.Form["isowner"];
                //Comment details
                DateTime thisDate = DateTime.UtcNow.AddHours(7);
                var addRequest = await _context.Comments.AddAsync(new Comment(commenter,videourl,content,thisDate,isOwner));
                var plusComment = await _context.Videos.FindAsync(videourl);
                plusComment.CommentCount += 1;
                _context.Update(plusComment);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        /*==============================================================================
                                     LIKE VIDEO
        ================================================================================*/
        [HttpPost]
        [Route("LikeVideo")]
        public async Task<IActionResult> LikeVideo()
        {
            try
            {
                var request = HttpContext.Request;
                var user = Request.Form["user"];
                var videourl = Request.Form["videourl"];
                //Comment details
                var addRequest = await _context.Likes.AddAsync(new Like(user,videourl));
                var plusLikeRequest = await _context.Videos.FindAsync(videourl);
                plusLikeRequest.LikeCount += 1;
                _context.Update(plusLikeRequest);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        /*==============================================================================
                                            UNLIKE VIDEO
        ================================================================================*/
        [HttpPost]
        [Route("UnLikeVideo")]
        public async Task<IActionResult> UnLikeVideo()
        {
            try
            {
                var request = HttpContext.Request;
                var user = Request.Form["user"];
                var videourl = Request.Form["videourl"];
                //Comment details
                var addRequest = _context.Likes.Remove(new Like(user, videourl));
                var plusLikeRequest = await _context.Videos.FindAsync(videourl);
                plusLikeRequest.LikeCount -= 1;
                _context.Update(plusLikeRequest);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        /*==============================================================================
                                            GET SEARCH RESULT
        ================================================================================*/
        [HttpPost]
        [Route("Search")]
        public async Task<IActionResult> Search()
        {
            try
            {
                var request = HttpContext.Request;
                string searchText = Request.Form["searchtext"];
                string currentUser = Request.Form["user"];
                searchText = searchText.ToLower();
                var userTable = await _context.Users.ToListAsync();
                var followTable = await _context.Follows.ToListAsync();
                //Get matched user result
                var userQuery = userTable.Where(user => user.NickName.ToLower().Contains(searchText) && user.Email!=currentUser).OrderByDescending(user => StringSimilarityScore(user.NickName,searchText));
                List<MatchedUserReasult> userResult = new List<MatchedUserReasult>();
                foreach (User matchedUser in userQuery)
                {
                    var countFollower = followTable.Where(follow => follow.Following == matchedUser.Email).Count();
                    userResult.Add(new MatchedUserReasult(matchedUser, countFollower));
                }    
                //Get matched vieo result
                var videoQuery = _context.Videos.ToList().Where(video => video.Title.ToLower().Contains(searchText) && video.Owner != currentUser).OrderByDescending(video=>StringSimilarityScore(video.Title,searchText));
                List<MatchedVideoResult> videoResult = new List<MatchedVideoResult>();
                foreach (Video matchedVideo in videoQuery)
                {
                    var ownerProfile = userTable.Where(user => user.Email == matchedVideo.Owner).FirstOrDefault();
                    videoResult.Add(new MatchedVideoResult(matchedVideo, ownerProfile));
                }    

                var response = JsonConvert.SerializeObject(new { UserResult=userResult, VideoResult=videoResult});
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
        double StringSimilarityScore(string name, string searchString)
        {
            if (name.Contains(searchString))
            {
                return (double)searchString.Length / (double)name.Length;
            }

            return 0;
        }

        /*==============================================================================
                                                 UPLOAD VIDEO
        ================================================================================*/
        [HttpPost]
        [Route("UploadVideo")]
        public async Task<IActionResult> Uploadvideo()
        {
            try
            {
                var request = HttpContext.Request;
                var email = Request.Form["owner"];
                var title = Request.Form["title"];
                var video = request.Form.Files[0].FileName;
                var thumbnail = request.Form.Files[1].FileName;
                //Get thumbnail from video

                using (MemoryStream memoryStream = new MemoryStream(), thumbnailStream= new MemoryStream())
                {
                    request.Form.Files[0].CopyTo(memoryStream);
                    request.Form.Files[1].CopyTo(thumbnailStream);
                    // Create a BlobServiceClient object which will be used to create a container client
                    BlobServiceClient blobServiceClient = new BlobServiceClient(AzureConnectionString);
                    // Create the container and return a container client object
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("toptop");
                    // Get a reference to a blob
                    BlobClient blobClient = containerClient.GetBlobClient(video);
                    BlobClient thumbnailBlob = containerClient.GetBlobClient(thumbnail);
                    // Upload data from the local file
                    memoryStream.Position = 0;
                    thumbnailStream.Position = 0;
                    await blobClient.UploadAsync(memoryStream);
                    await thumbnailBlob.UploadAsync(thumbnailStream);
                    //Update database
                    var newVideo = new Video(blobClient.Uri.ToString(), email, title, DateTime.UtcNow.AddHours(7),thumbnailBlob.Uri.ToString(),0);
                    _context.Videos.Add(newVideo);
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
                                      Hide Video
        ================================================================================*/
        [HttpPost]
        [Route("HideVideo")]
        public async Task<IActionResult> HideVideo()
        {
            try
            {
                var request = HttpContext.Request;
                var videoUrl = Request.Form["url"];
                var record = await _context.Videos.FindAsync(videoUrl);
                record.IsPrivate = 1;
                _context.Update(record);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        /*==============================================================================
                                      Public Video
        ================================================================================*/
        [HttpPost]
        [Route("PublicVideo")]
        public async Task<IActionResult> PublicVideo()
        {
            try
            {
                var request = HttpContext.Request;
                var videoUrl = Request.Form["url"];
                var record = await _context.Videos.FindAsync(videoUrl);
                record.IsPrivate = 0;
                _context.Update(record);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
