Owin/Katana Authentication Handler for Auth0. Plug into the ASP.NET 4.5 Owin infrastructure (middleware) and extends default providers with more social providers like Amazon, Facebook, GitHub, LinkedIn, LiveId Google, Twitter, Paypal, vKontakte and Enterprise provides like any IdP that speaks SAML Protocol, ADFS, Google Apps, ADFS, Windows Azure AD, etc. 

## Installation

	Install-Package Auth0-ASPNET-Owin

## Usage

1- Create a new ASP.NET 4.5 Project.

2- Go to Web.config and set `auth0:ClientId`, `auth0:ClientSecret` and `auth0:Domain` from appSettings.

> Note: These settings can be found on <a href="http://app.auth0.com" target="_new">Auth0 dashboard</a>.

3- Edit `App_Start\Startup.Auth.cs`:

~~~c#
public void ConfigureAuth(IAppBuilder app)
{
    // ...
    
    app.UseAuth0Authentication(
    	clientId:       System.Configuration.ConfigurationManager.AppSettings["auth0:ClientId"],
    	clientSecret:   System.Configuration.ConfigurationManager.AppSettings["auth0:ClientSecret"],
    	domain:         System.Configuration.ConfigurationManager.AppSettings["auth0:Domain"]);
}
~~~

> Note: The nuget provides a simple controller (_Auth0AccountController_) to process the authentication response from Auth0. If you want to use your own controller, make sure you set the `redirectPath` parameter. For example, in order to use the implementation provided by Visual Studio templates, use the following: `redirectPath: "/Account/ExternalLoginCallback"`.

4- Include the <a href="https://docs.auth0.com/login-widget2" target="_new">Auth0 Widget</a>:

~~~html
<a href="javascript:widget.signin();">Login</a>

<script src="https://d19p4zemcycm7a.cloudfront.net/w2/auth0-widget-2.3.min.js"></script>
<script type="text/javascript">
	var widget = new Auth0Widget({
	    domain:       '@System.Configuration.ConfigurationManager.AppSettings["auth0:Domain"]',
	    clientID:     '@System.Configuration.ConfigurationManager.AppSettings["auth0:ClientId"]',
	    callbackURL:  'http://localhost:PORT/signin-auth0'
	});
</script>
~~~

## Customizing the Claims Identity

You can change/add new claims by attaching to `OnAuthenticated`:

~~~c#
var provider = new Auth0.Owin.Auth0AuthenticationProvider
{
	OnAuthenticated = (context) =>
	{
		// context.User is a JObject with the original user object from Auth0
		context.Identity.AddClaim(new System.Security.Claims.Claim("foo", "bar"));
		context.Identity.AddClaim(
			new System.Security.Claims.Claim(
				"friendly_name",
				string.Format("{0}, {1}", context.User["family_name"], context.User["given_name"])));
		
		return System.Threading.Tasks.Task.FromResult(0);
	}
};

// specify the provider
app.UseAuth0Authentication(provider: provider, clientId: ... );
~~~

## Cross Site Request Forgery

You can validate the xsrf value by attaching to `OnReturnEndpoint`:

~~~c#
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
~~~

And set same value when the widget is shown:

~~~html
<a href="javascript:showWidget();">Login</a>

<script type="text/javascript">
	var widget = new Auth0Widget({
        domain:       '@System.Configuration.ConfigurationManager.AppSettings["auth0:Domain"]',
        clientID:     '@System.Configuration.ConfigurationManager.AppSettings["auth0:ClientId"]',
        callbackURL:  'http://localhost:PORT/signin-auth0'
    });

    function showWidget() {
        var xsrf = 'your_xsrf_random_string';
        var returnUrl = window.location.pathname;

        widget.signin({
            state: 'xsrf=' + xsrf + '&ru=' + returnUrl
        });
    }
</script>
~~~
