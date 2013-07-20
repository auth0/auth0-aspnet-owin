using System.Collections.Generic;

namespace Microsoft.Owin.Security.Auth0
{
    public class Auth0AuthenticationOptions : AuthenticationOptions
    {
        public Auth0AuthenticationOptions() : base("Auth0")
        {
            ReturnEndpointPath = "/signin-auth0";
            AuthenticationMode = AuthenticationMode.Passive;
            Connection = "";
            Domain = "";
            BackChannelRequestTimeOut = 60 * 1000; // 60 seconds
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

        public int BackChannelRequestTimeOut { get; set; }

        public string ReturnEndpointPath { get; set; }
        public string SignInAsAuthenticationType { get; set; }

        public IAuth0AuthenticationProvider Provider { get; set; }
        public ISecureDataHandler<AuthenticationExtra> StateDataHandler { get; set; }
    }
}
