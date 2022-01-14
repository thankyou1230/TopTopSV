using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Firebase.Auth;
using Google.Cloud.Firestore;
using System.Threading.Tasks;
using TopTopServer.Models;

namespace TopTopServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private FirebaseAuthProvider authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyAeobHFw2yHBP0bgrRCTQMRyv6F3BKjnx8"));

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login()
        {
            var email = Request.Form["email"];
            var password = Request.Form["password"];
            try
            {
                //var authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyAeobHFw2yHBP0bgrRCTQMRyv6F3BKjnx8"));
                var authenticator = await authProvider.SignInWithEmailAndPasswordAsync(email, password);
                if (authenticator.User.IsEmailVerified)
                {
                    return Ok();
                }
                else if (!authenticator.User.IsEmailVerified)
                {
                    authProvider.SendEmailVerificationAsync(authenticator);
                    return Unauthorized();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                // Info
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword()
        {
            var email = Request.Form["email"];
            try
            {
                //var authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyAeobHFw2yHBP0bgrRCTQMRyv6F3BKjnx8"));
                await authProvider.SendPasswordResetEmailAsync(email);
                return Ok();
            }
            catch (Exception ex)
            {
                // Info
                return BadRequest(ex.Message);
            }
        }


        

    }
}
