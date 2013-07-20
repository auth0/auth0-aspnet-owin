using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;

using Owin;

namespace Microsoft.Owin.Security.Auth0
{
    public class Auth0AuthenticationMiddleware : AuthenticationMiddleware<Auth0AuthenticationOptions>
    {
        private readonly ILogger _logger;

        public Auth0AuthenticationMiddleware(
            OwinMiddleware next,
            IAppBuilder app,
            Auth0AuthenticationOptions options)
            : base(next, options)
        {
            _logger = app.CreateLogger<Auth0AuthenticationMiddleware>();

            if (Options.Provider == null)
            {
                Options.Provider = new Auth0AuthenticationProvider();
            }
            if (Options.StateDataHandler == null)
            {
                var dataProtecter = app.CreateDataProtecter(typeof(Auth0AuthenticationMiddleware).FullName);
                Options.StateDataHandler = new ExtraDataHandler(dataProtecter);
            }
        }

        protected override AuthenticationHandler<Auth0AuthenticationOptions> CreateHandler()
        {
            return new Auth0AuthenticationHandler(_logger);
        }
    }
}
