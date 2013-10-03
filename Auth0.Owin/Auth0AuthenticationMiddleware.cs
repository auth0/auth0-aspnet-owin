using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Auth0.Owin
{
    public class Auth0AuthenticationMiddleware : AuthenticationMiddleware<Auth0AuthenticationOptions>
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        
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
            if (Options.StateDataFormat == null)
            {
                var dataProtector = app.CreateDataProtector(
                   typeof(Auth0AuthenticationMiddleware).FullName,
                   Options.AuthenticationType, "v1");

                Options.StateDataFormat = new PropertiesDataFormat(dataProtector);
            }

            _httpClient = new HttpClient(ResolveHttpMessageHandler(Options));
            _httpClient.Timeout = Options.BackchannelTimeout;
            _httpClient.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
        }

        protected override AuthenticationHandler<Auth0AuthenticationOptions> CreateHandler()
        {
            return new Auth0AuthenticationHandler(_httpClient, _logger);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Managed by caller")]
        private static HttpMessageHandler ResolveHttpMessageHandler(Auth0AuthenticationOptions options)
        {
            HttpMessageHandler handler = options.BackchannelHttpHandler ?? new WebRequestHandler();

            // If they provided a validator, apply it or fail.
            if (options.BackchannelCertificateValidator != null)
            {
                // Set the cert validate callback
                WebRequestHandler webRequestHandler = handler as WebRequestHandler;
                if (webRequestHandler == null)
                {
                    throw new InvalidOperationException("BackhannkelHttpHandler shoud be a WebRequestHandler");
                }

                webRequestHandler.ServerCertificateValidationCallback = options.BackchannelCertificateValidator.Validate;
            }

            return handler;
        }
    }
}
