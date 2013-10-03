using System.Collections.Generic;
using Microsoft.Owin.Security.Provider;
using Microsoft.Owin.Security;
using Microsoft.Owin;

namespace Auth0.Owin
{
    public class Auth0ReturnEndpointContext : ReturnEndpointContext
    {
        public Auth0ReturnEndpointContext(
            IOwinContext context,
            AuthenticationTicket ticket,
            IDictionary<string, string> errorDetails)
            : base(context, ticket, errorDetails)
        {
        }
    }
}
