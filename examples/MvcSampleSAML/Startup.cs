using System;
using System.IdentityModel.Metadata;
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
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    //OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                    //    validateInterval: TimeSpan.FromMinutes(30),
                    //    regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            app.UseKentorAuthServicesAuthentication(CreateAuthServicesOptions());
        }

        private static KentorAuthServicesAuthenticationOptions CreateAuthServicesOptions()
        {
            var authServicesOptions = new KentorAuthServicesAuthenticationOptions(false)
            {
                SPOptions = new SPOptions { EntityId = new EntityId("urn:MyApp"), ReturnUrl = new Uri("http://localhost:3500/") }
            };

            authServicesOptions.IdentityProviders.Add(new IdentityProvider(new EntityId("urn:YOUR-TENANT.auth0.com"), authServicesOptions.SPOptions)
                {
                    AllowUnsolicitedAuthnResponse = true,
                    MetadataUrl = new Uri("https://YOUR-TENANT.auth0.com/samlp/metadata/YOUR-CLIENT-ID"),
                    Binding = Saml2BindingType.HttpPost
                });
            return authServicesOptions;
        }
    }
}