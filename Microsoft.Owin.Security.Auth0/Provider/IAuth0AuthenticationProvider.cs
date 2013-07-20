using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Auth0
{
    public interface IAuth0AuthenticationProvider
    {
        Task Authenticated(Auth0AuthenticatedContext context);
        Task ReturnEndpoint(Auth0ReturnEndpointContext context);
    }
}
