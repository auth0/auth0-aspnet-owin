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
using Microsoft.Owin;
using System.Net.Http;
using Microsoft.Owin.Helpers;
using Microsoft.Owin.Infrastructure;
using System.Collections.Specialized;
using System.Web;

namespace Auth0.Owin
{
    internal class Auth0AuthenticationHandler : AuthenticationHandler<Auth0AuthenticationOptions>
    {
        private const string AuthorizeEndpoint = "https://{0}/authorize";
        private const string TokenEndpoint = "https://{0}/oauth/token";
        private const string UserInfoEndpoint = "https://{0}/userinfo";

        private readonly HttpClient _httpClient;

        private readonly ILogger _logger;

        public Auth0AuthenticationHandler(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public override async Task<bool> InvokeAsync()
        {
            return await InvokeReturnPath();
        }

        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            _logger.WriteVerbose("AuthenticateCore");

            try
            {
                string code = null;
                string state = null;
                IReadableStringCollection query = Request.Query;
                IList<string> values = query.GetValues("code");
                if (values != null && values.Count == 1)
                {
                    code = values[0];
                }
                values = query.GetValues("state");
                if (values != null && values.Count == 1)
                {
                    state = values[0];
                }

                // OAuth2 10.12 CSRF
                var authType = Options.AuthenticationType;
                Options.AuthenticationType = "auth0";
                Options.AuthenticationType = authType;

                var tokenRequestParameters = string.Format(
                    CultureInfo.InvariantCulture,
                    "client_id={0}&redirect_uri={1}&client_secret={2}&code={3}&grant_type=authorization_code",
                    Uri.EscapeDataString(Options.ClientId),
                    Uri.EscapeDataString(GenerateRedirectUri()),
                    Uri.EscapeDataString(Options.ClientSecret),
                    code);

                var body = new Dictionary<string,string> {
                    { "client_id", Options.ClientId },
                    { "redirect_uri", GenerateRedirectUri() },
                    { "client_secret", Options.ClientSecret },
                    { "code", code },
                    { "grant_type", "authorization_code" }
                };

                HttpResponseMessage tokenResponse = await _httpClient.PostAsync(string.Format(TokenEndpoint, Options.Domain), new FormUrlEncodedContent(body), Request.CallCancelled);
                tokenResponse.EnsureSuccessStatusCode();
                string text = await tokenResponse.Content.ReadAsStringAsync();
                JObject tokens = JObject.Parse(text);

                string accessToken = tokens["access_token"].Value<string>();
                string idToken = tokens["id_token"] != null ? tokens["id_token"].Value<string>() : null;

                HttpResponseMessage graphResponse = await _httpClient.GetAsync(
                   string.Format(UserInfoEndpoint, Options.Domain) + "?access_token=" + Uri.EscapeDataString(accessToken), Request.CallCancelled);
                graphResponse.EnsureSuccessStatusCode();
                text = await graphResponse.Content.ReadAsStringAsync();
                JObject user = JObject.Parse(text);

                var context = new Auth0AuthenticatedContext(Context, user, accessToken, idToken);
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

                return new AuthenticationTicket(context.Identity, context.Properties);
            }
            catch (Exception ex)
            {
                _logger.WriteWarning("Authentication failed", ex);
                return new AuthenticationTicket(null, null);
            }
        }

        protected override Task ApplyResponseChallengeAsync()
        {
            _logger.WriteVerbose("ApplyResponseChallenge");

            if (Response.StatusCode != 401)
            {
                return Task.FromResult<object>(null);
            }

            var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

            if (challenge != null)
            {
                string requestPrefix = Request.Scheme + "://" + Request.Host;
                string currentQueryString = Request.QueryString.ToString();
                string currentUri = string.IsNullOrEmpty(currentQueryString)
                    ? requestPrefix + Request.PathBase + Request.Path
                    : requestPrefix + Request.PathBase + Request.Path + "?" + currentQueryString;

                string redirectUri = requestPrefix + Request.PathBase + Options.ReturnEndpointPath;

                var extra = challenge.Properties;
                if (string.IsNullOrEmpty(extra.RedirectUri))
                {
                    extra.RedirectUri = currentUri;
                }

                // OAuth2 10.12 CSRF
                var authType = Options.AuthenticationType;
                Options.AuthenticationType = "auth0";
                GenerateCorrelationId(extra);
                Options.AuthenticationType = authType;

                string state = Options.StateDataFormat.Protect(extra);

                string authorizationEndpoint =
                    string.Format(AuthorizeEndpoint, Options.Domain) +
                        "?client_id=" + Uri.EscapeDataString(Options.ClientId) +
                        "&connection=" + Uri.EscapeDataString(Options.Connection) +
                        "&response_type=code" +
                        "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
                        "&state=" + Uri.EscapeDataString(state) +
                        (Options.Scopes.Length > 0 ? "&scope=" + Uri.EscapeDataString(string.Join(" ", Options.Scopes)) : "");

                Response.Redirect(authorizationEndpoint);
            }

            return Task.FromResult<object>(null);
        }

        public async Task<bool> InvokeReturnPath()
        {
            _logger.WriteVerbose("InvokeReturnPath");

            if (Options.ReturnEndpointPath != null &&
                String.Equals(Options.ReturnEndpointPath, Request.Path.ToString(), StringComparison.OrdinalIgnoreCase))
            {

                AuthenticationTicket ticket = await AuthenticateAsync();
                if (ticket == null)
                {
                    _logger.WriteWarning("Invalid return state, unable to redirect.");
                    Response.StatusCode = 500;
                    return true;
                }

                var context = new Auth0ReturnEndpointContext(Context, ticket);
                context.SignInAsAuthenticationType = Options.SignInAsAuthenticationType;

                await Options.Provider.ReturnEndpoint(context);

                if (context.SignInAsAuthenticationType != null && context.Identity != null)
                {
                    ClaimsIdentity signInIdentity = context.Identity;
                    if (!string.Equals(signInIdentity.AuthenticationType, context.SignInAsAuthenticationType, StringComparison.Ordinal))
                    {
                        signInIdentity = new ClaimsIdentity(signInIdentity.Claims, context.SignInAsAuthenticationType, signInIdentity.NameClaimType, signInIdentity.RoleClaimType);
                    }

                    Context.Authentication.SignIn(context.Properties ?? new AuthenticationProperties(), signInIdentity);
                }

                if (!context.IsRequestCompleted)
                {
                    string redirectUri = context.RedirectUri;

                    if (string.IsNullOrEmpty(redirectUri))
                    {
                        if (Request.Query["state"] != null && Request.Query["state"].StartsWith("ru="))
                        {
                            var state = HttpUtility.ParseQueryString(Request.Query["state"]);
                            redirectUri = state["ru"];
                        }
                        else
                        {
                            redirectUri = "/";
                        }
                    }

                    if (context.Identity == null)
                    {
                        // add a redirect hint that sign-in failed in some way
                        redirectUri = WebUtilities.AddQueryString(redirectUri, "error", "access_denied");
                    }

                    Response.Redirect(redirectUri);
                    context.RequestCompleted();
                }

                return context.IsRequestCompleted;
            }

            return false;
        }

        private string GenerateRedirectUri()
        {
            string requestPrefix = Request.Scheme + "://" + Request.Host;

            string redirectUri = requestPrefix + RequestPathBase + Options.ReturnEndpointPath; // + "?state=" + Uri.EscapeDataString(Options.StateDataHandler.Protect(state));            
            return redirectUri;
        }
    }
}
