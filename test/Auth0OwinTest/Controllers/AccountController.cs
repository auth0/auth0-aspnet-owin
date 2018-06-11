using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;

namespace Auth0OwinTest.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login(string returnUrl)
        {
            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties
                {
                    RedirectUri = returnUrl ?? Url.Action("Index", "Home")
                },
                "Auth0");
            return new HttpUnauthorizedResult();
        }

        [Authorize]
        public void Logout()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            HttpContext.GetOwinContext().Authentication.SignOut("Auth0");
        }

        public ActionResult LoginError(string error, string error_description)
        {
            ViewBag.Error = error;
            ViewBag.ErrorDescription = error_description;

            return View();
        }

        [Authorize]
        public ActionResult Claims()
        {
            return View();
        }

        [Authorize]
        public ActionResult UserProfile()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            return View(new UserProfileViewModel()
            {
                Name = claimsIdentity?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                EmailAddress = claimsIdentity?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                ProfileImage = claimsIdentity?.Claims.FirstOrDefault(c => c.Type == "picture")?.Value
            });
        }

        [Authorize]
        public ActionResult Tokens()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            // Extract tokens
            string accessToken = claimsIdentity?.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
            string idToken = claimsIdentity?.Claims.FirstOrDefault(c => c.Type == "id_token")?.Value;
            string refreshToken = claimsIdentity?.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;

            // Save tokens in ViewBag
            ViewBag.AccessToken = accessToken;
            ViewBag.IdToken = idToken;
            ViewBag.RefreshToken = refreshToken;

            return View();
        }
    }

    internal class ChallengeResult : HttpUnauthorizedResult
    {
        private const string XsrfKey = "XsrfId";

        public ChallengeResult(string provider, string redirectUri)
            : this(provider, redirectUri, null)
        {
        }

        public ChallengeResult(string provider, string redirectUri, string userId)
        {
            LoginProvider = provider;
            RedirectUri = redirectUri;
            UserId = userId;
        }

        public string LoginProvider { get; set; }
        public string RedirectUri { get; set; }
        public string UserId { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            var properties = new AuthenticationProperties {RedirectUri = RedirectUri};
            if (UserId != null)
            {
                properties.Dictionary[XsrfKey] = UserId;
            }
            context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
        }
    }
}