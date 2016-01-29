using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Provider;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace Auth0.Owin
{
    /// <summary>
    /// Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.
    /// </summary>
    public class Auth0AuthenticatedContext : BaseContext
    {
        /// <summary>
        /// Initializes a <see cref="Auth0AuthenticatedContext"/>
        /// </summary>
        /// <param name="context">The OWIN environment</param>
        /// <param name="user">The JSON-serialized user</param>
        /// <param name="accessToken">Auth0 Access token</param>
        /// <param name="expires">Seconds until expiration</param>
        public Auth0AuthenticatedContext(IOwinContext context, JObject user, string accessToken, string idToken, string refreshToken)
            : base(context)
        {
            User = user;
            AccessToken = accessToken;
            IdToken = idToken;
            RefreshToken = refreshToken;

            Id = TryGetValue(user, "user_id");
            Name = TryGetValue(user, "name");
            FirstName = TryGetValue(user, "given_name");
            LastName = TryGetValue(user, "family_name");
            Email = TryGetValue(user, "email");
            Picture = TryGetValue(user, "picture");
            Nickname = TryGetValue(user, "nickname");

            Connection = Connection = user["identities"][0]["connection"].ToString();
            Provider = user["identities"][0]["provider"].ToString();
            ProviderAccessToken = user["identities"][0]["access_token"] != null ? user["identities"][0]["access_token"].ToString() : null;
        }

        /// <summary>
        /// Gets the JSON-serialized user
        /// </summary>
        public JObject User { get; private set; }

        /// <summary>
        /// Gets the Auth0 access token
        /// </summary>
        public string AccessToken { get; private set; }

        public string Id { get; private set; }

        public string Name { get; private set; }
        
        public string FirstName { get; private set; }
        
        public string LastName { get; private set; }
        
        public string Email { get; private set; }
        
        public string Connection { get; private set; }

        public string Nickname { get; private set; }

        public string Picture { get; private set; }
        
        public string Provider { get; private set; }
        
        public string ProviderAccessToken { get; private set; }

        public string IdToken { get; private set; }

        public string RefreshToken { get; private set; }

        /// <summary>
        /// Gets the <see cref="ClaimsIdentity"/> representing the user
        /// </summary>
        public ClaimsIdentity Identity { get; set; }

        /// <summary>
        /// Gets or sets a property bag for common authentication properties
        /// </summary>
        public AuthenticationProperties Properties { get; set; }

        private static string TryGetValue(JObject user, string propertyName)
        {
            JToken value;
            return user.TryGetValue(propertyName, out value) ? value.ToString() : null;
        }
    }
}