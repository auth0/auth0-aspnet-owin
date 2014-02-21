using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MvcSample
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login")
                // LoginPath property informs the middleware that it should change an outgoing 401 Unauthorized status code into a 302 redirection onto the given login path
                // More info: http://msdn.microsoft.com/en-us/library/microsoft.owin.security.cookies.cookieauthenticationoptions.loginpath(v=vs.111).aspx
            });

            // Use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Use Auth0
            var provider = new Auth0.Owin.Auth0AuthenticationProvider
            {
                OnAuthenticated = (context) =>
                {
                    // context.User is a JObject with the original user object from Auth0
                    if (context.User["admin"] != null)
                    {
                        context.Identity.AddClaim(new Claim("admin", context.User.Value<string>("admin")));
                    }
                    
                    context.Identity.AddClaim(
                        new Claim(
                            "friendly_name",
                            string.Format("{0}, {1}", context.User["family_name"], context.User["given_name"])));

                    // NOTE: uncomment this if you send an array of roles (i.e.: ['sales','marketing','hr'])
                    //context.User["roles"].ToList().ForEach(r =>
                    //{
                    //    context.Identity.AddClaim(new Claim(ClaimTypes.Role, r.ToString()));
                    //});

                    return Task.FromResult(0);
                }
            };

            app.UseAuth0Authentication(
                clientId: ConfigurationManager.AppSettings["auth0:ClientId"],
                clientSecret: ConfigurationManager.AppSettings["auth0:ClientSecret"],
                domain: ConfigurationManager.AppSettings["auth0:Domain"],
                //redirectPath: "/Account/ExternalLoginCallback", // use AccountController instead of Auth0AccountController
                provider: provider);
        }
    }
}