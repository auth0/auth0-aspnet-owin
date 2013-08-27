using System.Collections.Generic;
using Microsoft.Owin.Security.Provider;
using Microsoft.Owin.Security;

namespace Auth0.Owin
{
    public class Auth0ReturnEndpointContext : ReturnEndpointContext
    {
        public Auth0ReturnEndpointContext(
            IDictionary<string, object> environment,
            AuthenticationTicket ticket,
            IDictionary<string, string> errorDetails)
            : base(environment, ticket, errorDetails)
        {
        }
    }
}
