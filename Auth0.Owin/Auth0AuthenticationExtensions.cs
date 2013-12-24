using Auth0.Owin;
using System;

namespace Owin
{
    /// <summary>
    /// Extension methods for using <see cref="Auth0AuthenticationMiddleware"/>
    /// </summary>
    public static class Auth0AuthenticationExtensions
    {
        /// <summary>
        /// Authenticate users using Auth0
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="options">Middleware configuration options</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        public static IAppBuilder AddAuth0Authentication(this IAppBuilder app, Auth0AuthenticationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            app.Use(typeof(Auth0AuthenticationMiddleware), app, options);
            return app;
        }

        /// <summary>
        /// Authenticate users using Auth0
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="clientId">The client ID assigned by Auth0</param>
        /// <param name="clientSecret">The client secret assigned by Auth0</param>
        /// <param name="domain"></param>
        /// <param name="connection"></param>
        /// <param name="displayName"></param>
        /// <param name="saveIdToken"></param>
        /// <param name="scopes"></param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        public static IAppBuilder AddAuth0Authentication(
            this IAppBuilder app,
            string clientId,
            string clientSecret,
            string domain,
            string displayName,
            string connection = null,
            bool saveIdToken = true,
            string scopes = "openid",
            IAuth0AuthenticationProvider provider = null)
        {
            return AddAuth0Authentication(
                app,
                new Auth0AuthenticationOptions
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    Domain = domain,
                    Caption = displayName,
                    Connection = connection,
                    AuthenticationType = string.IsNullOrEmpty(connection) ? Constants.DefaultAuthenticationType : connection,
                    SaveIdToken = saveIdToken,
                    Scope = scopes.Split(' '),
                    Provider = provider
                });
        }
    }
}
