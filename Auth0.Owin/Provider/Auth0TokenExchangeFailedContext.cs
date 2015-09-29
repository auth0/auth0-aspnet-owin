using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Provider;

using System;

namespace Auth0.Owin
{
    /// <summary>
    /// Context passed when the token exchange fails in the Auth0 middleware
    /// </summary>
    public class Auth0TokenExchangeFailedContext : BaseContext<Auth0AuthenticationOptions>
    {
        /// <summary>
        /// Creates a new context object.
        /// </summary>
        /// <param name="context">The OWIN request context</param>
        /// <param name="options">The Auth0 middleware options</param>
        /// <param name="exception">The exception</param>
        /// <param name="code">The authorization code</param>
        /// <param name="state">The state</param>
        public Auth0TokenExchangeFailedContext(IOwinContext context, Auth0AuthenticationOptions options, Exception exception, string code, string state)
            : base(context, options)
        {
            Code = code;
            State = state;
            Exception = exception;
        }

        /// <summary>
        /// Gets the authorization code.
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        public string State { get; private set; }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        public Exception Exception { get; private set; }
    }
}