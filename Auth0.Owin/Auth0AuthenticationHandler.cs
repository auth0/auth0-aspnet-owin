using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Infrastructure;

using Newtonsoft.Json.Linq;
using Microsoft.Owin.Security;

namespace Auth0.Owin
{
    internal class Auth0AuthenticationHandler : AuthenticationHandler<Auth0AuthenticationOptions>
    {
        private const string AuthorizeEndpoint = "https://{0}/authorize";
        private const string TokenEndpoint = "https://{0}/oauth/token";
        private const string GraphApiEndpoint = "https://{0}/userinfo";

        private readonly ILogger _logger;

        public Auth0AuthenticationHandler(ILogger logger)
        {
            _logger = logger;
        }

        public override async Task<bool> Invoke()
        {
            if (Options.ReturnEndpointPath != null &&
                String.Equals(Options.ReturnEndpointPath, Request.Path, StringComparison.OrdinalIgnoreCase))
            {
                return await InvokeReturnPath();
            }
            return false;
        }

        protected override async Task<AuthenticationTicket> AuthenticateCore()
        {
            _logger.WriteVerbose("AuthenticateCore");

            AuthenticationExtra extra = null;
            try
            {
                string code = null;
                IDictionary<string, string[]> query = Request.GetQuery();
                string[] values;
                if (query.TryGetValue("code", out values) && values != null && values.Length == 1)
                {
                    code = values[0];
                }

                extra = UnprotectExtraData();
                if (extra == null)
                {
                    return null;
                }

                // OAuth2 10.12 CSRF
                var authType = Options.AuthenticationType;
                Options.AuthenticationType = "auth0";
                if (!ValidateCorrelationId(extra, _logger))
                {
                    return new AuthenticationTicket(null, extra);
                }
                Options.AuthenticationType = authType;

                var tokenRequestParameters = string.Format(
                    CultureInfo.InvariantCulture,
                    "client_id={0}&redirect_uri={1}&client_secret={2}&code={3}&grant_type=authorization_code",
                    Uri.EscapeDataString(Options.ClientId),
                    Uri.EscapeDataString(GenerateRedirectUri()),
                    Uri.EscapeDataString(Options.ClientSecret),
                    code);

                WebRequest tokenRequest = WebRequest.Create(string.Format(TokenEndpoint, Options.Domain));
                tokenRequest.Method = "POST";
                tokenRequest.ContentType = "application/x-www-form-urlencoded";
                tokenRequest.ContentLength = tokenRequestParameters.Length;
                tokenRequest.Timeout = Options.BackChannelRequestTimeOut;
                using (var bodyStream = new StreamWriter(tokenRequest.GetRequestStream()))
                {
                    bodyStream.Write(tokenRequestParameters);
                }

                WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
                string accessToken = null;
                string idToken = null;

                using (var reader = new StreamReader(tokenResponse.GetResponseStream()))
                {
                    string oauthTokenResponse = await reader.ReadToEndAsync();
                    JObject oauth2Token = JObject.Parse(oauthTokenResponse);
                    accessToken = oauth2Token["access_token"].Value<string>();
                    idToken = oauth2Token["id_token"] != null ? oauth2Token["id_token"].Value<string>() : null;
                }

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    _logger.WriteWarning("Access token was not found");
                    return new AuthenticationTicket(null, extra);
                }

                JObject accountInformation;
                var accountInformationRequest = WebRequest.Create(string.Format(GraphApiEndpoint, Options.Domain) + "?access_token=" + Uri.EscapeDataString(accessToken));
                accountInformationRequest.Timeout = Options.BackChannelRequestTimeOut;
                var accountInformationResponse = await accountInformationRequest.GetResponseAsync();
                using (var reader = new StreamReader(accountInformationResponse.GetResponseStream()))
                {
                    accountInformation = JObject.Parse(await reader.ReadToEndAsync());
                }

                var context = new Auth0AuthenticatedContext(Request.Environment, accountInformation, accessToken, idToken);
                context.Identity = new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, context.Id, "http://www.w3.org/2001/XMLSchema#string", context.Connection),
                        new Claim(ClaimTypes.Name, context.Name, "http://www.w3.org/2001/XMLSchema#string", context.Connection),
                        new Claim("user_id", context.Id, "http://www.w3.org/2001/XMLSchema#string", context.Connection),
                    },
                    context.Connection,
                    ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);

                if (!string.IsNullOrWhiteSpace(context.Email))
                {
                    context.Identity.AddClaim(new Claim("email", context.Email, "http://www.w3.org/2001/XMLSchema#string", context.Connection));
                    context.Identity.AddClaim(new Claim(ClaimTypes.Email, context.Email, "http://www.w3.org/2001/XMLSchema#string", context.Connection));
                }

                if (!string.IsNullOrWhiteSpace(context.FirstName)) context.Identity.AddClaim(new Claim("given_name", context.FirstName, "http://www.w3.org/2001/XMLSchema#string", context.Connection));
                if (!string.IsNullOrWhiteSpace(context.LastName)) context.Identity.AddClaim(new Claim("family_name", context.LastName, "http://www.w3.org/2001/XMLSchema#string", context.Connection));
                if (!string.IsNullOrWhiteSpace(context.Connection)) context.Identity.AddClaim(new Claim("connection", context.Connection, "http://www.w3.org/2001/XMLSchema#string", context.Connection));
                if (!string.IsNullOrWhiteSpace(context.Picture)) context.Identity.AddClaim(new Claim("picture", context.Picture, "http://www.w3.org/2001/XMLSchema#string", context.Connection));
                if (!string.IsNullOrWhiteSpace(context.Provider)) context.Identity.AddClaim(new Claim("provider", context.Provider, "http://www.w3.org/2001/XMLSchema#string", context.Connection));
                if (!string.IsNullOrWhiteSpace(context.ProviderAccessToken)) context.Identity.AddClaim(new Claim("provider_access_token", context.ProviderAccessToken, "http://www.w3.org/2001/XMLSchema#string", context.Connection));
                if (Options.SaveIdToken && !string.IsNullOrWhiteSpace(context.IdToken)) context.Identity.AddClaim(new Claim("id_token", context.IdToken, "http://www.w3.org/2001/XMLSchema#string", context.Connection));

                await Options.Provider.Authenticated(context);

                context.Extra = extra;

                return new AuthenticationTicket(context.Identity, context.Extra);
            }
            catch (Exception ex)
            {
                _logger.WriteWarning("Authentication failed", ex);
                return new AuthenticationTicket(null, extra);
            }
        }

        protected override async Task ApplyResponseChallenge()
        {
            _logger.WriteVerbose("ApplyResponseChallenge");

            if (Response.StatusCode != 401)
            {
                return;
            }

            var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

            if (challenge != null)
            {
                string requestPrefix = Request.Scheme + "://" + Request.Host;
                string currentQueryString = Request.QueryString;
                string currentUri = string.IsNullOrEmpty(currentQueryString)
                    ? requestPrefix + Request.PathBase + Request.Path
                    : requestPrefix + Request.PathBase + Request.Path + "?" + currentQueryString;

                string redirectUri = requestPrefix + Request.PathBase + Options.ReturnEndpointPath;

                var extra = challenge.Extra;
                if (string.IsNullOrEmpty(extra.RedirectUrl))
                {
                    extra.RedirectUrl = currentUri;
                }

                // OAuth2 10.12 CSRF
                var authType = Options.AuthenticationType;
                Options.AuthenticationType = "auth0";
                GenerateCorrelationId(extra);
                Options.AuthenticationType = authType;

                string state = Options.StateDataHandler.Protect(extra);

                string authorizationEndpoint =
                    string.Format(AuthorizeEndpoint, Options.Domain) +
                        "?client_id=" + Uri.EscapeDataString(Options.ClientId) +
                        "&connection=" + Uri.EscapeDataString(Options.Connection) +
                        "&response_type=code" +
                        "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
                        "&state=" + Uri.EscapeDataString(state) +
                        (Options.Scopes.Length > 0 ? "&scope=" + Uri.EscapeDataString(string.Join(" ", Options.Scopes)) : "");

                Response.StatusCode = 302;
                Response.SetHeader("Location", authorizationEndpoint);
            }
        }

        public async Task<bool> InvokeReturnPath()
        {
            _logger.WriteVerbose("InvokeReturnPath");

            var model = await Authenticate();

            var context = new Auth0ReturnEndpointContext(Request.Environment, model, ErrorDetails);
            context.SignInAsAuthenticationType = Options.SignInAsAuthenticationType;
            if (model.Extra != null)
            {
                context.RedirectUri = model.Extra.RedirectUrl;
                model.Extra.RedirectUrl = null;
            }
            
            await Options.Provider.ReturnEndpoint(context);

            if (context.SignInAsAuthenticationType != null && context.Identity != null)
            {
                ClaimsIdentity signInIdentity = context.Identity;
                if (!string.Equals(signInIdentity.AuthenticationType, context.SignInAsAuthenticationType, StringComparison.Ordinal))
                {
                    signInIdentity = new ClaimsIdentity(signInIdentity.Claims, context.SignInAsAuthenticationType, signInIdentity.NameClaimType, signInIdentity.RoleClaimType);
                }
                if (context.Extra != null)
                {
                    Response.Grant(signInIdentity, context.Extra);
                }
                else
                {
                    Response.Grant(signInIdentity);
                }
            }

            if (!context.IsRequestCompleted)
            {
                if (context.RedirectUri != null)
                {
                    Response.Redirect(context.RedirectUri);
                }
                else
                {
                    Response.Redirect("/Account/ExternalLoginCallback?loginProvider=" + model.Identity.FindFirst("connection").Value);
                }
                
                context.RequestCompleted();
            } 

            return context.IsRequestCompleted;
        }

        private AuthenticationExtra UnprotectExtraData()
        {
            IDictionary<string, string[]> query = Request.GetQuery();
            string state = null;
            string[] values;
            if (query.TryGetValue("state", out values) && values != null && values.Length == 1)
            {
                state = values[0];
            }

            return Options.StateDataHandler.Unprotect(state);
        }

        private string GenerateRedirectUri()
        {
            string requestPrefix = Request.Scheme + "://" + Request.Host;

            string redirectUri = requestPrefix + RequestPathBase + Options.ReturnEndpointPath; // + "?state=" + Uri.EscapeDataString(Options.StateDataHandler.Protect(state));            
            return redirectUri;
        }
    }
}
