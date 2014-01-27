using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MvcSample.Controllers
{
    public class Auth0AccountController : Controller
    {
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        //
        // GET: /Auth0Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            AuthenticationManager.SignOut();

            var externalIdentity = await AuthenticationManager.GetExternalIdentityAsync(DefaultAuthenticationTypes.ExternalCookie);
            if (externalIdentity == null)
            {
                throw new Exception("Could not get the external identity. Please check your Auth0 configuration settings and ensure that " +
                                    "you configured UseCookieAuthentication and UseExternalSignInCookie in the OWIN Startup class. " +
                                    "Also make sure you are not calling setting the callbackOnLocationHash option on the JavaScript widget.");
            }

            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, CreateIdentity(externalIdentity));
            return RedirectToLocal(returnUrl);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> Logout()
        {
            AuthenticationManager.SignOut();
            //You can comment out the next line if you're not saving the ID_Token in a client-side cookie.
            Response.Cookies.Clear();
            return RedirectToLocal("/");
        }

        private static ClaimsIdentity CreateIdentity(ClaimsIdentity externalIdentity)
        {
            var identity = new ClaimsIdentity(externalIdentity.Claims, DefaultAuthenticationTypes.ApplicationCookie);

            // This claim is required for the ASP.NET Anti-Forgery Token to function.
            // See http://msdn.microsoft.com/en-us/library/system.web.helpers.antiforgeryconfig.uniqueclaimtypeidentifier(v=vs.111).aspx
            identity.AddClaim(
                new Claim(
                    "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", 
                    "ASP.NET Identity", 
                    "http://www.w3.org/2001/XMLSchema#string"));

            return identity;
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : Redirect("/");
        }
    }
}