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
            var provider = new Auth0AuthenticationProvider()
            {
                //// When logging out
                //OnApplyLogout = context =>
                //{
                //    if (System.Configuration.ConfigurationManager.AppSettings["ForceHttps"] == "true" &&
                //        context.LogoutUri.Contains("&returnTo=http%3A%2F%2F"))
                //    {
                //        context.LogoutUri = context.LogoutUri.Replace("&returnTo=http%3A%2F%2F",
                //            "&returnTo=https%3A%2F%2F");
                //    }

                //    context.Response.Redirect(context.LogoutUri);
                //},
                //// When redirecting to /authorize
                //OnApplyRedirect = context =>
                //{
                //    if (System.Configuration.ConfigurationManager.AppSettings["ForceHttps"] == "true" &&
                //        context.RedirectUri.Contains("&redirect_uri=http%3A%2F%2F"))
                //    {
                //        context.RedirectUri = context.RedirectUri.Replace("&redirect_uri=http%3A%2F%2F",
                //            "&redirect_uri=https%3A%2F%2F");
                //    }

                //    context.Response.Redirect(context.RedirectUri);
                //},
                //// When doing the code exchange
                //OnCustomizeTokenExchangeRedirectUri = (context) =>
                //{
                //    var redirectUri = context.RedirectUri;

                //    if (System.Configuration.ConfigurationManager.AppSettings["ForceHttps"] == "true"
                //        && redirectUri.StartsWith("http://"))
                //    {
                //        context.RedirectUri = redirectUri.Replace("http://", "https://");
                //    }
                //}
            };
            var options = new Auth0AuthenticationOptions()
            {
                Domain = auth0Domain,
                ClientId = auth0ClientId,
                ClientSecret = auth0ClientSecret,
                ErrorRedirectPath = new PathString("/Account/LoginError"),

                Provider = provider
            };
            app.UseAuth0Authentication(options);

        }
    }
}
