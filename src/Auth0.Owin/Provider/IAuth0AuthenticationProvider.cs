using System.Threading.Tasks;

namespace Auth0.Owin
{
    /// <summary>
    /// Specifies callback methods which the <see cref="Auth0AuthenticationMiddleware"></see> invokes to enable developer control over the authentication process. />
    /// </summary>
    public interface IAuth0AuthenticationProvider
    {
        /// <summary>
        /// Invoked whenever Auth0 succesfully authenticates a user
        /// </summary>
        /// <param name="context">Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        Task Authenticated(Auth0AuthenticatedContext context);

        /// <summary>
        /// Invoked prior to the <see cref="System.Security.Claims.ClaimsIdentity"/> being saved in a local cookie and the browser being redirected to the originally requested URL.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        Task ReturnEndpoint(Auth0ReturnEndpointContext context);

        /// <summary>
        /// Called when a Challenge causes a redirect to authorize endpoint in the Auth0 middleware
        /// </summary>
        /// <param name="context">Contains redirect URI and <see cref="AuthenticationProperties"/> of the challenge </param>
        void ApplyRedirect(Auth0ApplyRedirectContext context);

        /// <summary>
        /// Called the redirect_uri is generated during the token exchange. 
        /// You may need to change this value if you handle SSL offloading on the load balancer.
        /// </summary>
        /// <param name="context">Contains redirect URI and <see cref="AuthenticationProperties"/> of the challenge </param>
        void CustomizeTokenExchangeRedirectUri(Auth0CustomizeTokenExchangeRedirectUriContext context);

        /// <summary>
        /// Called when a token exchange fails in the Auth0 middleware.
        /// </summary>
        void TokenExchangeFailed(Auth0TokenExchangeFailedContext context);
    }
}