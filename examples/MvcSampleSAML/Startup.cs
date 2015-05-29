using System;
using System.Configuration;
using System.IdentityModel.Metadata;
using System.Net;
using Kentor.AuthServices;
using Kentor.AuthServices.Configuration;
using Kentor.AuthServices.Owin;
using Kentor.AuthServices.WebSso;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using MvcSampleSAML;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace MvcSampleSAML
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider()
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            app.UseKentorAuthServicesAuthentication(CreateAuthServicesOptions());
        }

        private static KentorAuthServicesAuthenticationOptions CreateAuthServicesOptions()
        {
            var authServicesOptions = new KentorAuthServicesAuthenticationOptions(false)
            {
                SPOptions = new SPOptions { EntityId = new EntityId("urn:" + ConfigurationManager.AppSettings["auth0:ApplicationName"]), ReturnUrl = new Uri(ConfigurationManager.AppSettings["auth0:ReturnUrl"]) }
            };

            authServicesOptions.IdentityProviders.Add(new IdentityProvider(new EntityId("urn:" + ConfigurationManager.AppSettings["auth0:Domain"]), authServicesOptions.SPOptions)
                {
                    AllowUnsolicitedAuthnResponse = true,
                    MetadataUrl = new Uri(String.Format("https://{0}/samlp/metadata/{1}", ConfigurationManager.AppSettings["auth0:Domain"], ConfigurationManager.AppSettings["auth0:ClientId"])),
                    Binding = Saml2BindingType.HttpPost
                });
            return authServicesOptions;
        }
    }
}