using System.Collections.Generic;
using Microsoft.Owin.Security;
using Auth0.Owin;

namespace Owin
{
    public static class Auth0AuthenticationExtensions
    {
        public static IAppBuilder AddAuth0Authentication(
             this IAppBuilder app,
             string clientId,
             string clientSecret,
             string domain,
             string connection,
             string displayName,
             bool saveIdToken = true,
             string scopes = "openid",
             IAuth0AuthenticationProvider provider = null)
        {

            app.Use(typeof(Auth0AuthenticationMiddleware), app, new Auth0AuthenticationOptions
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                Domain = domain,
                Caption = displayName,
                Connection = connection,
                AuthenticationType = connection,
                SaveIdToken = saveIdToken,
                Scopes = scopes.Split(' '),
                SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType(),
                Provider = provider
            });

            return app;
        }
    }
}
