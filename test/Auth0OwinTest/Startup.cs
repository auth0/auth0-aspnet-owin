using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Auth0.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;

[assembly: OwinStartup(typeof(Auth0OwinTest.Startup))]
namespace Auth0OwinTest
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Configure Auth0 parameters
            string auth0Domain = ConfigurationManager.AppSettings["auth0:Domain"];
            string auth0ClientId = ConfigurationManager.AppSettings["auth0:ClientId"];
            string auth0ClientSecret = ConfigurationManager.AppSettings["auth0:ClientSecret"];

            // Set Cookies as default authentication type
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                LoginPath = new PathString("/Account/Login")
            });

            // Configure Auth0 authentication
            var options = new Auth0AuthenticationOptions()
            {
                Domain = auth0Domain,
                ClientId = auth0ClientId,
                ClientSecret = auth0ClientSecret,

                //Provider = new Auth0AuthenticationProvider()
                //{
                //    OnApplyRedirect = context =>
                //    {
                //        context.RedirectUri += "&audience=" + Uri.EscapeDataString("https://rs256.test.api");

                //        context.Response.Redirect(context.RedirectUri);
                //    }
                //}
            };
            app.UseAuth0Authentication(options);

        }
    }
}
