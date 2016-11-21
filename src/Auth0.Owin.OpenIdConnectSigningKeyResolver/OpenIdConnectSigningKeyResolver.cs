using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.IdentityModel.Protocols;

namespace Auth0.Owin
{
    public class OpenIdConnectSigningKeyResolver
    {
        private readonly OpenIdConnectConfiguration openIdConfig;

        public OpenIdConnectSigningKeyResolver(string authority)
        {
            var cm = new ConfigurationManager<OpenIdConnectConfiguration>($"{authority.TrimEnd('/')}/.well-known/openid-configuration");
            openIdConfig = AsyncHelper.RunSync(async () => await cm.GetConfigurationAsync());
        }

        public SecurityKey GetSigningKey(SecurityKeyIdentifier identifier)
        {
            // Find the security token which matches the identifier
            var securityToken = openIdConfig.SigningTokens.FirstOrDefault(t =>
            {
                // Each identifier has multiple clauses. Try and match for each
                foreach (var securityKeyIdentifierClause in identifier)
                {
                    if (t.MatchesKeyIdentifierClause(securityKeyIdentifierClause))
                        return true;
                }

                return false;
            });

            // Return the first key of the security token (if found)
            return securityToken?.SecurityKeys.FirstOrDefault();
        }
    }
}
