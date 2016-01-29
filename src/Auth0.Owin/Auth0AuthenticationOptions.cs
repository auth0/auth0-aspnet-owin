using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Auth0.Owin
{
    /// <summary>
    /// Configuration options for <see cref="Auth0AuthenticationMiddleware"/>
    /// </summary>
    public class Auth0AuthenticationOptions : AuthenticationOptions
    {
        /// <summary>
        /// Initializes a new <see cref="Auth0AuthenticationOptions"/>
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "Auth0.Owin.Auth0AuthenticationOptions.set_Caption(System.String)", Justification = "Not localizable.")]
        public Auth0AuthenticationOptions()
            : base(Constants.DefaultAuthenticationType)
        {
            Caption = Constants.DefaultAuthenticationType;
            CallbackPath = new PathString("/signin-auth0");
            AuthenticationMode = AuthenticationMode.Passive;
            Scope = new List<string>();
            BackchannelTimeout = TimeSpan.FromSeconds(60);

            Connection = string.Empty;
            Domain = string.Empty;
            SaveIdToken = true;
            EnableDiagnostics = true;
        }

        /// <summary>
        /// Gets or sets the Auth0-assigned client Id
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the Auth0-assigned client secret
        /// </summary>
        public string ClientSecret { get; set; }

        public string Connection { get; set; }

        public string Domain { get; set; }

        public bool SaveIdToken { get; set; }

        public bool SaveRefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the a pinned certificate validator to use to validate the endpoints used
        /// in back channel communications belong to Auth0.
        /// </summary>
        /// <value>
        /// The pinned certificate validator.
        /// </value>
        /// <remarks>If this property is null then the default certificate checks are performed,
        /// validating the subject name and if the signing chain is a trusted party.</remarks>
        public ICertificateValidator BackchannelCertificateValidator { get; set; }

        /// <summary>
        /// Gets or sets timeout value in milliseconds for back channel communications with Auth0.
        /// </summary>
        /// <value>
        /// The back channel timeout in milliseconds.
        /// </value>
        public TimeSpan BackchannelTimeout { get; set; }

        /// <summary>
        /// The HttpMessageHandler used to communicate with Auth0.
        /// This cannot be set at the same time as BackchannelCertificateValidator unless the value 
        /// can be downcast to a WebRequestHandler.
        /// </summary>
        public HttpMessageHandler BackchannelHttpHandler { get; set; }

        /// <summary>
        /// Get or sets the text that the user can display on a sign in user interface.
        /// </summary>
        public string Caption
        {
            get { return Description.Caption; }
            set { Description.Caption = value; }
        }

        /// <summary>
        /// The request path within the application's base path where the user-agent will be returned.
        /// The middleware will process this request when it arrives.
        /// Default value is "/signin-auth0".
        /// </summary>
        public PathString CallbackPath { get; set; }

        /// <summary>
        /// The request path within the application's base path where the user-agent will be returned when the middleware is completed.
        /// </summary>
        public PathString RedirectPath { get; set; }

        /// <summary>
        /// Gets or sets the name of another authentication middleware which will be responsible for actually issuing a user <see cref="System.Security.Claims.ClaimsIdentity"/>.
        /// </summary>
        public string SignInAsAuthenticationType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IAuth0AuthenticationProvider"/> used to handle authentication events.
        /// </summary>
        public IAuth0AuthenticationProvider Provider { get; set; }

        /// <summary>
        /// Gets or sets the type used to secure data handled by the middleware.
        /// </summary>
        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }

        /// <summary>
        /// A list of permissions to request.
        /// </summary>
        public IList<string> Scope { get; set; }

        /// <summary>
        ///  Allow diagnostic information to be sent to Auth0.
        /// </summary>
        public bool EnableDiagnostics { get; set; }
    }
}