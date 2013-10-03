using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Auth0.Owin
{
    public class Auth0AuthenticationOptions : AuthenticationOptions
    {
        public Auth0AuthenticationOptions() : base("Auth0")
        {
            ReturnEndpointPath = "/signin-auth0";
            AuthenticationMode = AuthenticationMode.Passive;
            Connection = "";
            Domain = "";
            BackchannelTimeout = TimeSpan.FromSeconds(60 * 1000); // 60 seconds
            SaveIdToken = true;
        }

        public string Caption
        {
            get { return Description.Caption; }
            set { Description.Caption = value; }
        }

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Connection { get; set; }
        public string Domain { get; set; }
        public bool SaveIdToken { get; set; }
        public string[] Scopes { get; set; }

        /// <summary>
        /// Gets or sets the a pinned certificate validator to use to validate the endpoints used
        /// in back channel communications belong to Facebook.
        /// </summary>
        /// <value>
        /// The pinned certificate validator.
        /// </value>
        /// <remarks>If this property is null then the default certificate checks are performed,
        /// validating the subject name and if the signing chain is a trusted party.</remarks>
        public ICertificateValidator BackchannelCertificateValidator { get; set; }

        /// <summary>
        /// Gets or sets timeout value in milliseconds for back channel communications with Facebook.
        /// </summary>
        /// <value>
        /// The back channel timeout in milliseconds.
        /// </value>
        public TimeSpan BackchannelTimeout { get; set; }

        /// <summary>
        /// The HttpMessageHandler used to communicate with Facebook.
        /// This cannot be set at the same time as BackchannelCertificateValidator unless the value 
        /// can be downcast to a WebRequestHandler.
        /// </summary>
        public HttpMessageHandler BackchannelHttpHandler { get; set; }

        public string ReturnEndpointPath { get; set; }
        public string SignInAsAuthenticationType { get; set; }

        public IAuth0AuthenticationProvider Provider { get; set; }
        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }
    }
}
