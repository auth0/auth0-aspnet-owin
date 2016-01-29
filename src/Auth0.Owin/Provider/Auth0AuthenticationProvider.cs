using System;
using System.Threading.Tasks;

namespace Auth0.Owin
{
    /// <summary>
    /// Default <see cref="IAuth0AuthenticationProvider"/> implementation.
    /// </summary>
    public class Auth0AuthenticationProvider : IAuth0AuthenticationProvider
    {
        /// <summary>
        /// Initializes a <see cref="Auth0AuthenticationProvider"/>
        /// </summary>
        public Auth0AuthenticationProvider()
        {
            OnAuthenticated = context => Task.FromResult<object>(null);
            OnReturnEndpoint = context => Task.FromResult<object>(null);
            OnApplyRedirect = context =>
                context.Response.Redirect(context.RedirectUri);
            OnTokenExchangeFailed = context => { };
            OnCustomizeTokenExchangeRedirectUri = context => { };
        }

        /// <summary>
        /// Gets or sets the function that is invoked when the Authenticated method is invoked.
        /// </summary>
        public Func<Auth0AuthenticatedContext, Task> OnAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets the function that is invoked when the ReturnEndpoint method is invoked.
        /// </summary>
        public Func<Auth0ReturnEndpointContext, Task> OnReturnEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the delegate that is invoked when the ApplyRedirect method is invoked.
        /// </summary>
        public Action<Auth0ApplyRedirectContext> OnApplyRedirect { get; set; }

        /// <summary>
        /// Gets or sets the delegate that is invoked when the CustomizeTokenExchangeRedirectUri method is invoked.
        /// </summary>
        public Action<Auth0CustomizeTokenExchangeRedirectUriContext> OnCustomizeTokenExchangeRedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the delegate that is invoked when the TokenExchangeFailed method is invoked.
        /// </summary>
        public Action<Auth0TokenExchangeFailedContext> OnTokenExchangeFailed { get; set; }

        /// <summary>
        /// Invoked whenever Auth0 succesfully authenticates a user
        /// </summary>
        /// <param name="context">Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        public virtual Task Authenticated(Auth0AuthenticatedContext context)
        {
            return OnAuthenticated(context);
        }

        /// <summary>
        /// Invoked prior to the <see cref="System.Security.Claims.ClaimsIdentity"/> being saved in a local cookie and the browser being redirected to the originally requested URL.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        public virtual Task ReturnEndpoint(Auth0ReturnEndpointContext context)
        {
            return OnReturnEndpoint(context);
        }

        /// <summary>
        /// Called when a Challenge causes a redirect to authorize endpoint in the Auth0 middleware
        /// </summary>
        /// <param name="context">Contains redirect URI and <see cref="AuthenticationProperties"/> of the challenge </param>
        public virtual void ApplyRedirect(Auth0ApplyRedirectContext context)
        {
            OnApplyRedirect(context);
        }


        /// <summary>
        /// Called the redirect_uri is generated during the token exchange. 
        /// You may need to change this value if you handle SSL offloading on the load balancer.
        /// </summary>
        /// <param name="context">Contains redirect URI and <see cref="AuthenticationProperties"/> of the challenge </param>
        public void CustomizeTokenExchangeRedirectUri(Auth0CustomizeTokenExchangeRedirectUriContext context)
        {
            OnCustomizeTokenExchangeRedirectUri(context);
        }

        /// <summary>
        /// Called when a token exchange fails in the Auth0 middleware.
        /// </summary>
        public void TokenExchangeFailed(Auth0TokenExchangeFailedContext context)
        {
            OnTokenExchangeFailed(context);
        }
    }
}
