using System.Linq;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Auth0.Owin
{
    public class OpenIdConnectSigningKeyResolver
    {
        private readonly OpenIdConnectConfiguration openIdConfig;

        public OpenIdConnectSigningKeyResolver(string authority)
        {
            var cm = new ConfigurationManager<OpenIdConnectConfiguration>($"{authority.TrimEnd('/')}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
            openIdConfig = AsyncHelper.RunSync(async () => await cm.GetConfigurationAsync());
        }

        public SecurityKey[] GetSigningKey(string kid)
        {
            // Find the security token which matches the identifier
            return new[] { openIdConfig.JsonWebKeySet.GetSigningKeys().FirstOrDefault(t => t.KeyId == kid) };
        }
    }
}
