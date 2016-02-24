using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Auth0.AuthenticationApi.Models;
using Auth0.Core;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using OwinWithAuthApi.Models;

namespace OwinWithAuthApi.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public AccountController()
        {
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Try and validate the user using 
            var auth = new Auth0.AuthenticationApi.AuthenticationApiClient(new Uri(string.Format("https://{0}/", ConfigurationManager.AppSettings["auth0:Domain"])));
            try
            {
                // Autenticate the user
                var authResponse = await auth.Authenticate(new AuthenticationRequest
                {
                    Connection = "Username-Password-Authentication",
                    ClientId = ConfigurationManager.AppSettings["auth0:ClientId"],
                    Scope = "openid name email",
                    Username = model.Email,
                    Password = model.Password
                });

                // Get user information from the token
                User user = await auth.GetTokenInfo(authResponse.IdToken);

                // Create a valid claims identity with the relevant claims, and sign the identity in
                ClaimsIdentity identity = new ClaimsIdentity(null, DefaultAuthenticationTypes.ApplicationCookie);
                identity.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Auth0"));
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UserId));
                identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, user.FullName));
                AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);

                // Redirect to return URL (of specified)
                return RedirectToLocal(returnUrl);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.Message);
                return View(model);
            }
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }


        #region Helpers

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}