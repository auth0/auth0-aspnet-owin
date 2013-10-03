using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

using Microsoft.Owin.Security.Provider;

using Newtonsoft.Json.Linq;
using Microsoft.Owin.Security;
using Microsoft.Owin;

namespace Auth0.Owin
{
    public class Auth0AuthenticatedContext : BaseContext
    {
        public Auth0AuthenticatedContext(IOwinContext environment, JObject user, string accessToken, string idToken)
            : base(environment)
        {
            IDictionary<string, JToken> userAsDictionary = user;

            User = user;
            AccessToken = accessToken;

            Id = User["user_id"].ToString();
            Name = PropertyValueIfExists("name", userAsDictionary);
            FirstName = PropertyValueIfExists("given_name", userAsDictionary);
            LastName = PropertyValueIfExists("family_name", userAsDictionary);
            Email = PropertyValueIfExists("email", userAsDictionary);
            Connection = user["identities"][0]["connection"].ToString();
            Picture = PropertyValueIfExists("picture", userAsDictionary);
            Provider = user["identities"][0]["provider"].ToString();
            ProviderAccessToken = user["identities"][0]["access_token"] != null ? user["identities"][0]["access_token"].ToString() : null;
            IdToken = idToken;
        }

        public JObject User { get; private set; }
        public string AccessToken { get; private set; }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string Connection { get; private set; }
        public string Picture { get; private set; }
        public string Provider { get; private set; }
        public string ProviderAccessToken { get; private set; }
        public string IdToken { get; private set; }

        public ClaimsIdentity Identity { get; set; }
        public AuthenticationProperties Properties { get; set; }

        private static string PropertyValueIfExists(string property, IDictionary<string, JToken> dictionary)
        {
            return dictionary.ContainsKey(property) ? dictionary[property].ToString() : null;
        }
    }
}
