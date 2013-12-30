using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace $rootnamespace$.Controllers
{
    public class Auth0AccountController : Controller
    {
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return this.HttpContext.GetOwinContext().Authentication;
            }
        }

        //
        // GET: /Auth0Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            this.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            var externalIdentity = await this.AuthenticationManager.GetExternalIdentityAsync(DefaultAuthenticationTypes.ExternalCookie);
            this.AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, CreateIdentity(externalIdentity));
            
            return this.RedirectToLocal(returnUrl);
        }

        private static ClaimsIdentity CreateIdentity(ClaimsIdentity externalIdentity)
        {
            var identity = new ClaimsIdentity(
                externalIdentity.Claims,
                DefaultAuthenticationTypes.ApplicationCookie);

            // Add claim for anti-forgery (see AntiForgeryConfig.UniqueClaimTypeIdentifier)
            identity.AddClaim(
                new Claim(
                    "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "ASP.NET Identity", "http://www.w3.org/2001/XMLSchema#string"));

            return identity;
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }
            else
            {
                return this.Redirect("/");
            }
        }
	}
}