using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Provider;

namespace Auth0.Owin
{
    /// <summary>
    /// Context passed when a Challenge causes a logout in the Auth0 middleware
    /// </summary>
    public class Auth0ApplyLogoutContext : BaseContext<Auth0AuthenticationOptions>
    {
        /// <summary>
        /// Creates a new context object.
        /// </summary>
        /// <param name="context">The OWIN request context</param>
        /// <param name="options">The Auth0 middleware options</param>
        /// <param name="properties">The authentication properties of the challenge</param>
        /// <param name="logoutUri">The logout URI</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "3#",
            Justification = "Represents header value")]
        public Auth0ApplyLogoutContext(IOwinContext context, Auth0AuthenticationOptions options,
            AuthenticationProperties properties, string logoutUri)
            : base(context, options)
        {
            LogoutUri = logoutUri;
            Properties = properties;
        }

        /// <summary>
        /// Gets the URI used for the logout operation.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Represents header value")]
        public string LogoutUri { get; set; }

        /// <summary>
        /// Gets the authenticaiton properties of the challenge
        /// </summary>
        public AuthenticationProperties Properties { get; private set; }
    }
}