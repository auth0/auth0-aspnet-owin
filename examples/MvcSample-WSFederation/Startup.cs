using System;
using System.Configuration;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.WsFederation;
using MvcSample;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace MvcSample
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(WsFederationAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = WsFederationAuthenticationDefaults.AuthenticationType,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider()
            });

            app.UseWsFederationAuthentication(new WsFederationAuthenticationOptions
            {
                MetadataAddress = String.Format("https://{0}/wsfed/{1}/FederationMetadata/2007-06/FederationMetadata.xml", 
                    ConfigurationManager.AppSettings["auth0:Domain"], 
                    ConfigurationManager.AppSettings["auth0:ClientId"]),
                Wtrealm = "urn:" + ConfigurationManager.AppSettings["auth0:ApplicationName"],
                Notifications = new WsFederationAuthenticationNotifications
                {
                    SecurityTokenValidated = notification =>
                    {
                        notification.AuthenticationTicket.Identity.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Auth0"));
                        return Task.FromResult(true);
                    }
                }
            });
        }
    }
}