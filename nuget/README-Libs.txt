1. Go to Web.config and set "auth0:ClientId", "auth0:ClientSecret" and "auth0:Domain" from appSettings.

    Note: These settings can be found in the Auth0 dashboard (https://app.auth0.com/).

2. Edit Startup.cs in order to register the Cookie and Auth0 middleware

public void Configuration(IAppBuilder app)
{
    // Configure Auth0 parameters
    string auth0Domain = ConfigurationManager.AppSettings["auth0:Domain"];
    string auth0ClientId = ConfigurationManager.AppSettings["auth0:ClientId"];
    string auth0ClientSecret = ConfigurationManager.AppSettings["auth0:ClientSecret"];

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
