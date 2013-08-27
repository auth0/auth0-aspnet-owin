using System.Threading.Tasks;

namespace Auth0.Owin
{
    public interface IAuth0AuthenticationProvider
    {
        Task Authenticated(Auth0AuthenticatedContext context);
        Task ReturnEndpoint(Auth0ReturnEndpointContext context);
    }
}
