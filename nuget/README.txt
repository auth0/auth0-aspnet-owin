PLEASE NOTE!! There is a bug in Microsoft's OWIN implementation for System.Web, which can cause cookies to disappear on some occasions. 
To work around this issue, you will also need to install the Kentor.OwinCookieSaver NuGet package:

```
Install-Package Kentor.OwinCookieSaver
```

Once that is done you will need to also register the Kentor Cookie Saver middleware before the cookie middleware:

```csharp
app.UseKentorOwinCookieSaver();

app.UseCookieAuthentication(new CookieAuthenticationOptions(...));
```

See the implementation section below for a full code sample.

For more information please see the Kentor.OwinCookieSaver GitHub page at https://github.com/Sustainsys/owin-cookie-saver. 
For more information on using Auth0 in your ASP.NET (OWIN) application, please see our Quickstart at https://auth0.com/docs/quickstart/webapp/aspnet-owin

Implementing Auth0 in your application
--------------------------------------

1. Go to Web.config and set "auth0:ClientId", "auth0:ClientSecret" and "auth0:Domain" from appSettings.

    Note: These settings can be found in the Auth0 dashboard (https://app.auth0.com/).

2. Edit Startup.cs in order to register the Cookie and Auth0 middleware

public void Configuration(IAppBuilder app)
{
    // Configure Auth0 parameters
    string auth0Domain = ConfigurationManager.AppSettings["auth0:Domain"];
    string auth0ClientId = ConfigurationManager.AppSettings["auth0:ClientId"];
    string auth0ClientSecret = ConfigurationManager.AppSettings["auth0:ClientSecret"];

	// Enable the Cookie saver middleware to work around a bug in the OWIN implementation
	// !! DO NOT FORGET TO ADD THE KENTOR PACKAGE !!
    app.UseKentorOwinCookieSaver();

    // Set Cookies as default authentication type
    app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
    app.UseCookieAuthentication(new CookieAuthenticationOptions
    {
        AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
        LoginPath = new PathString("/Account/Login")
    });

    // Configure Auth0 authentication
    var options = new Auth0AuthenticationOptions()
    {
        Domain = auth0Domain,
        ClientId = auth0ClientId,
        ClientSecret = auth0ClientSecret
    };
    app.UseAuth0Authentication(options);
}

3. Challenge the Auth0 middleware when the user logs in or out:

public class AccountController : Controller
{
    public ActionResult Login(string returnUrl)
    {
        return new ChallengeResult("Auth0", returnUrl ?? Url.Action("Index", "Home"));
    }

    [Authorize]
    public void Logout()
    {
        HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
        HttpContext.GetOwinContext().Authentication.SignOut(new AuthenticationProperties
        {
            RedirectUri = Url.Action("Index", "Home")
        }, "Auth0");
    }
}
