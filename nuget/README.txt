1- Go to Web.config and set "auth0:ClientId", "auth0:ClientSecret" and "auth0:Domain" from appSettings.

    Note: These settings can be found in the Auth0 dashboard (https://app.auth0.com/).

2- Edit App_Start\Startup.Auth.cs in order to call the UseAuth0Authentication extension method:

public void ConfigureAuth(IAppBuilder app)
{
    // Enable the application to use a cookie to store information for the signed in user
    app.UseCookieAuthentication(new CookieAuthenticationOptions
    {
        AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
		LoginPath = new PathString("/Account/Login")
		// LoginPath property informs the middleware that it should change an outgoing 401 Unauthorized status code into a 302 redirection onto the given login path
		// More info: http://msdn.microsoft.com/en-us/library/microsoft.owin.security.cookies.cookieauthenticationoptions.loginpath(v=vs.111).aspx
    });

    // Use a cookie to temporarily store information about a user logging in with a third party login provider
    app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

    // ...

    app.UseAuth0Authentication(
    	clientId:       System.Configuration.ConfigurationManager.AppSettings["auth0:ClientId"],
    	clientSecret:   System.Configuration.ConfigurationManager.AppSettings["auth0:ClientSecret"],
    	domain:         System.Configuration.ConfigurationManager.AppSettings["auth0:Domain"]);
}

> Note: This nuget provides a simple controller (Auth0AccountController) to process the authentication response from Auth0. If you want to use your own controller, you need to set the "redirectPath" parameter. For example, in order to use the implementation provided by Visual Studio templates, use the following: redirectPath: "/Account/ExternalLoginCallback".

3- Include the Auth0 Lock:

<a href="javascript:lock.signin();">Login</a>

<script src="http://cdn.auth0.com/js/lock-6.6.js"></script>
<script type="text/javascript">
	var lock = new Auth0Lock({
        '@System.Configuration.ConfigurationManager.AppSettings["auth0:ClientId"]'
        '@System.Configuration.ConfigurationManager.AppSettings["auth0:Domain"]'
	});
</script>

> Note: Once user is authenticated, you can access his profile by doing:

@if (Request.IsAuthenticated)
{
	var id_token = "@ClaimsPrincipal.Current.FindFirst("id_token").Value";
	var email = "@ClaimsPrincipal.Current.FindFirst("email").Value";

	// ...
}

## Customizing the Claims Identity

You can change/add new claims by attaching to "OnAuthenticated":

var provider = new Auth0.Owin.Auth0AuthenticationProvider
{
	OnAuthenticated = (context) =>
	{
		// These are examples of adding additional claims. Comment them out if you're not going to use them.
		// context.User is a JObject with the original user object from Auth0
		if (context.User["admin"] != null)
		{
			context.Identity.AddClaim(new Claim("admin", context.User.Value<string>("admin")));
		}

		context.Identity.AddClaim(
			new Claim(
				"friendly_name",
				string.Format("{0}, {1}", context.User["family_name"], context.User["given_name"])));

		// NOTE: uncomment this if you send an array of roles (i.e.: ['sales','marketing','hr'])
		//context.User["roles"].ToList().ForEach(r =>
		//{
		//    context.Identity.AddClaim(new Claim(ClaimTypes.Role, r.ToString()));
		//});

		return System.Threading.Tasks.Task.FromResult(0);
	}
};

// specify the provider
app.UseAuth0Authentication(provider: provider, clientId: ... );

## Cross Site Request Forgery

You can validate the xsrf value by attaching to "OnReturnEndpoint":

var provider = new Auth0.Owin.Auth0AuthenticationProvider
{
	OnReturnEndpoint = (context) =>
	{
		// xsrf validation
		if (context.Request.Query["state"] != null && context.Request.Query["state"].Contains("xsrf="))
		{
			var state = HttpUtility.ParseQueryString(context.Request.Query["state"]);
			if (state["xsrf"] != "your_xsrf_random_string")
			{
				throw new HttpException(400, "invalid xsrf");
			}
		}

		return System.Threading.Tasks.Task.FromResult(0);
	}
};

// specify the provider
app.UseAuth0Authentication(provider: provider, clientId: ... );

And set same value when the widget is shown:

<a href="javascript:showWidget();">Login</a>

<script type="text/javascript">
    var lock = new Auth0Lock(
        '@System.Configuration.ConfigurationManager.AppSettings["auth0:ClientId"]'
        '@System.Configuration.ConfigurationManager.AppSettings["auth0:Domain"]'
    );

    function showWidget() {
        var xsrf = 'your_xsrf_random_string';
        var returnUrl = window.location.pathname;

        lock.showSignin({
            state: 'xsrf=' + xsrf + '&ru=' + returnUrl
        });
    }
</script>

## Token Exchange

In the token exchange phase the Owin middleware will exchange the authorization code for an access_token, id_token (and optionally also a refresh token). 

If your application is not configured correctly this exchange can fail. The provider allows you to catch all errors that occur during the token exchange to log them or take additional actions:

var provider = new Auth0.Owin.Auth0AuthenticationProvider
{
    OnTokenExchangeFailed = (context) =>
    {
        logger.Error(context.Exception.Message);
    }
};

Depending on your architecture it could be possible that SSL is handled by an other tier in your network (like a load balancer), which can cause issues with the redirect_uri during the token exchange. The provider allows you to override the generated redirect_uri:

var provider = new Auth0.Owin.Auth0AuthenticationProvider
{
    OnCustomizeTokenExchangeRedirectUri = (context) =>
    {
        context.RedirectUri = "https://some/other/url"
    }
};