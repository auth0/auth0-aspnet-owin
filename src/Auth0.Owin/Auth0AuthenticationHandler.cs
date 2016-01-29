using System.Linq;
using Microsoft.Owin;
using Microsoft.Owin.Helpers;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace Auth0.Owin
{
    internal class Auth0AuthenticationHandler : AuthenticationHandler<Auth0AuthenticationOptions>
    {
        private const string AuthorizeEndpoint = "https://{0}/authorize";
        private const string TokenEndpoint = "https://{0}/oauth/token";
        private const string UserInfoEndpoint = "https://{0}/userinfo";

        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public Auth0AuthenticationHandler(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            AuthenticationProperties properties = null;
            string code = null;
            string state = null;

            try
            {
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

                properties = Options.StateDataFormat.Unprotect(state);

                if (code == null)
                {
                    // Null if the remote server returns an error.
                    return new AuthenticationTicket(null, properties);
                }

                var tokenRequestParameters = string.Format(
                    CultureInfo.InvariantCulture,
                    "client_id={0}&redirect_uri={1}&client_secret={2}&code={3}&grant_type=authorization_code",
                    Uri.EscapeDataString(Options.ClientId),
                    Uri.EscapeDataString(GenerateRedirectUri(properties)),
                    Uri.EscapeDataString(Options.ClientSecret),
                    code);

                var body = new Dictionary<string, string> {
                    { "client_id", Options.ClientId },
                    { "redirect_uri", GenerateRedirectUri(properties) },
                    { "client_secret", Options.ClientSecret },
                    { "code", Uri.EscapeDataString(code) },
                    { "grant_type", "authorization_code" }
                };

                HttpResponseMessage tokenResponse = await _httpClient.PostAsync(string.Format(TokenEndpoint, Options.Domain), new FormUrlEncodedContent(body), Request.CallCancelled);
                await EnsureTokenExchangeSuccessful(tokenResponse);

                string text = await tokenResponse.Content.ReadAsStringAsync();
                JObject tokens = JObject.Parse(text);

                string accessToken = tokens["access_token"].Value<string>();
                string idToken = tokens["id_token"] != null ? tokens["id_token"].Value<string>() : null;
                string refreshToken = tokens["refresh_token"] != null ? tokens["refresh_token"].Value<string>() : null;

                HttpResponseMessage graphResponse = await _httpClient.GetAsync(
                   string.Format(UserInfoEndpoint, Options.Domain) + "?access_token=" + Uri.EscapeDataString(accessToken), Request.CallCancelled);
                graphResponse.EnsureSuccessStatusCode();
                text = await graphResponse.Content.ReadAsStringAsync();
                JObject user = JObject.Parse(text);

                var context = new Auth0AuthenticatedContext(Context, user, accessToken, idToken, refreshToken);
                context.Identity = new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, context.Id, ClaimValueTypes.String, context.Connection),
                        new Claim("user_id", context.Id, ClaimValueTypes.String, Constants.Auth0Issuer),

                        new Claim(ClaimTypes.Name, context.Name, ClaimValueTypes.String, context.Connection),
                        new Claim("name", context.Name, ClaimValueTypes.String, context.Connection),
                    },
                    Options.AuthenticationType,
                    ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);

                if (!string.IsNullOrWhiteSpace(context.Email))
                {
                    context.Identity.AddClaim(new Claim(ClaimTypes.Email, context.Email, ClaimValueTypes.String, context.Connection));
                    context.Identity.AddClaim(new Claim("email", context.Email, ClaimValueTypes.String, context.Connection));
                }

                if (!string.IsNullOrWhiteSpace(context.Nickname))
                    context.Identity.AddClaim(new Claim("nickname", context.Nickname, ClaimValueTypes.String, context.Connection));
                if (!string.IsNullOrWhiteSpace(context.FirstName))
                    context.Identity.AddClaim(new Claim("given_name", context.FirstName, ClaimValueTypes.String, context.Connection));
                if (!string.IsNullOrWhiteSpace(context.LastName))
                    context.Identity.AddClaim(new Claim("family_name", context.LastName, ClaimValueTypes.String, context.Connection));
                if (!string.IsNullOrWhiteSpace(context.Connection))
                    context.Identity.AddClaim(new Claim("connection", context.Connection, ClaimValueTypes.String, context.Connection));
                if (!string.IsNullOrWhiteSpace(context.Picture))
                    context.Identity.AddClaim(new Claim("picture", context.Picture, ClaimValueTypes.String, context.Connection));
                if (!string.IsNullOrWhiteSpace(context.Provider))
                    context.Identity.AddClaim(new Claim("provider", context.Provider, ClaimValueTypes.String, context.Connection));
                if (!string.IsNullOrWhiteSpace(context.ProviderAccessToken))
                    context.Identity.AddClaim(new Claim("provider_access_token", context.ProviderAccessToken, ClaimValueTypes.String, context.Connection));

                if (Options.SaveIdToken && !string.IsNullOrWhiteSpace(context.IdToken))
                    context.Identity.AddClaim(new Claim("id_token", context.IdToken, ClaimValueTypes.String, Constants.Auth0Issuer));
                if (Options.SaveRefreshToken && !string.IsNullOrWhiteSpace(context.RefreshToken))
                    context.Identity.AddClaim(new Claim("refresh_token", context.RefreshToken, ClaimValueTypes.String, Constants.Auth0Issuer));

                context.Identity.AddClaim(new Claim("access_token", context.AccessToken, ClaimValueTypes.String, Constants.Auth0Issuer));

                context.Properties = properties ?? new AuthenticationProperties();

                await Options.Provider.Authenticated(context);

                return new AuthenticationTicket(context.Identity, context.Properties);
            }
            catch (Exception ex)
            {
                var tokenExchangeFailedContext = new Auth0TokenExchangeFailedContext(
                    Context, Options,
                    ex, code, state);
                Options.Provider.TokenExchangeFailed(tokenExchangeFailedContext);

                _logger.WriteError(ex.Message);
            }
            return new AuthenticationTicket(null, properties);
        }

        private async Task EnsureTokenExchangeSuccessful(HttpResponseMessage tokenResponse)
        {
            if (!tokenResponse.IsSuccessStatusCode)
            {
                string errorResponse = null;

                try
                {
                    errorResponse = await tokenResponse.Content.ReadAsStringAsync();
                    tokenResponse.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    throw new TokenExchangeFailedException(errorResponse, ex);
                }
            }
        }

        protected override Task ApplyResponseChallengeAsync()
        {
            if (Response.StatusCode != 401)
            {
                return Task.FromResult<object>(null);
            }

            AuthenticationResponseChallenge challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

            if (challenge != null)
            {
                string baseUri =
                    Request.Scheme +
                    Uri.SchemeDelimiter +
                    Request.Host +
                    Request.PathBase;

                string currentUri =
                    baseUri +
                    Request.Path +
                    Request.QueryString;

                string redirectUri =
                    baseUri +
                    Options.CallbackPath;

                AuthenticationProperties properties = challenge.Properties;
                if (string.IsNullOrEmpty(properties.RedirectUri))
                {
                    properties.RedirectUri = currentUri;
                }

                // comma separated
                string scope = string.Join(",", Options.Scope);

                string state = Options.StateDataFormat.Protect(properties);

                string authorizationEndpoint =
                    string.Format(AuthorizeEndpoint, Options.Domain) +
                        "?client_id=" + Uri.EscapeDataString(Options.ClientId) +
                        "&connection=" + Uri.EscapeDataString(Options.Connection ?? string.Empty) +
                        "&response_type=code" +
                        "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
                        "&state=" + Uri.EscapeDataString(state) +
                        (Options.Scope.Count > 0 ? "&scope=" + Uri.EscapeDataString(string.Join(" ", Options.Scope)) : string.Empty);

                var redirectContext = new Auth0ApplyRedirectContext(
                    Context, Options,
                    properties, authorizationEndpoint);
                Options.Provider.ApplyRedirect(redirectContext);
            }

            return Task.FromResult<object>(null);
        }

        public override async Task<bool> InvokeAsync()
        {
            return await InvokeReplyPathAsync();
        }

        private async Task<bool> InvokeReplyPathAsync()
        {
            if (Options.CallbackPath.HasValue && Options.CallbackPath == Request.Path)
            {
                if (Request.Query["error"] != null)
                {
                    _logger.WriteVerbose("Remote server returned an error: " + Request.QueryString);

                    var redirectUrl = Options.RedirectPath + Request.QueryString;
                    Response.Redirect(redirectUrl);
                    return true;
                }

                AuthenticationTicket ticket = await AuthenticateAsync();
                if (ticket == null)
                {
                    _logger.WriteWarning("Invalid return state, unable to redirect.");
                    Response.StatusCode = 500;
                    return true;
                }

                var context = new Auth0ReturnEndpointContext(Context, ticket);
                context.SignInAsAuthenticationType = Options.SignInAsAuthenticationType;
                context.RedirectUri = ticket.Properties != null ? ticket.Properties.RedirectUri : null;

                await Options.Provider.ReturnEndpoint(context);

                if (context.SignInAsAuthenticationType != null && context.Identity != null)
                {
                    ClaimsIdentity grantIdentity = context.Identity;
                    if (!string.Equals(grantIdentity.AuthenticationType, context.SignInAsAuthenticationType, StringComparison.Ordinal))
                    {
                        grantIdentity = new ClaimsIdentity(grantIdentity.Claims, context.SignInAsAuthenticationType, grantIdentity.NameClaimType, grantIdentity.RoleClaimType);
                    }
                    Context.Authentication.SignIn(context.Properties, grantIdentity);
                }

                if (!context.IsRequestCompleted)
                {
                    string redirectUri = context.RedirectUri ?? Options.RedirectPath.ToString();
                    if (context.Identity == null)
                    {
                        // add a redirect hint that sign-in failed in some way
                        redirectUri = WebUtilities.AddQueryString(redirectUri, "error", "access_denied");
                    }

                    if (context.Request.Query["state"] != null && context.Request.Query["state"].Contains("ru="))
                    {
                        // set returnUrl with state -> ru
                        var state = HttpUtility.ParseQueryString(context.Request.Query["state"]);
                        redirectUri = WebUtilities.AddQueryString(redirectUri, "returnUrl", state["ru"]);
                    }

                    Response.Redirect(redirectUri);
                    context.RequestCompleted();
                }

                return context.IsRequestCompleted;
            }
            return false;
        }

        private string GenerateRedirectUri(AuthenticationProperties properties)
        {
            string requestPrefix = Request.Scheme + "://" + Request.Host;

            string redirectUri = requestPrefix + RequestPathBase + Options.CallbackPath; // + "?state=" + Uri.EscapeDataString(Options.StateDataHandler.Protect(state));          

            var context = new Auth0CustomizeTokenExchangeRedirectUriContext(Request.Context, Options, properties, redirectUri);
            Options.Provider.CustomizeTokenExchangeRedirectUri(context);

            return context.RedirectUri;
        }
    }
}
