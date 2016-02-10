using System.Web.Configuration;
using System.Web.Http;

using Api;

using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;

using Owin;

using AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode;

[assembly: OwinStartup(typeof(Startup))]
namespace Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var issuer = WebConfigurationManager.AppSettings["Auth0Domain"];
            var audience = WebConfigurationManager.AppSettings["Auth0ClientID"];
            var secret = TextEncodings.Base64Url.Decode(WebConfigurationManager.AppSettings["Auth0ClientSecret"]);
            
            // Owin configuration.
            app.UseCors(CorsOptions.AllowAll);
            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                AllowedAudiences = new[] { audience },
                IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[]
                {
                    new SymmetricKeyIssuerSecurityTokenProvider(issuer, secret)
                },
            });

            // Setup web api.
            var configuration = new HttpConfiguration();
            configuration.MapHttpAttributeRoutes();
            app.UseWebApi(configuration);
        }
    }
}