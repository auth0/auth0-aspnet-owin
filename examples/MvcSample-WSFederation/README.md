# SAML ASP.NET MVC Sample

This sample shows how to integrate your ASP.NET MVC application with Auth0 using SAML (without using the Auth0 SDKs). 

## Configuring the application in Auth0

1. Create a new application in Auth0
2. Enable the WS-FED addon on the Addons tab
3. In the settings of the SAML addon, specify the callback url `http://localhost:3500/` (or whatever the actual URL for your applications is, 
and set the Realm to `urn:MyApp`.

## Configuring the ASP.NET MVC application. 

This sample uses the **Microsoft.Owin.Security.WsFederation** NuGet package for WS-FED support, so install the package:

```
Install-Package Microsoft.Owin.Security.WsFederation
```

The library needs to be configured at application startup:

```
public partial class Startup
{
    public void Configuration(IAppBuilder app)
    {
        app.SetDefaultSignInAsAuthenticationType(WsFederationAuthenticationDefaults.AuthenticationType);

        app.UseCookieAuthentication(new CookieAuthenticationOptions
        {
            AuthenticationType = WsFederationAuthenticationDefaults.AuthenticationType,
            LoginPath = new PathString("/Account/Login"),
            Provider = new CookieAuthenticationProvider()
        });
        app.UseWsFederationAuthentication(new WsFederationAuthenticationOptions
        {
            MetadataAddress = "https://YOUR DOMAIN/wsfed/YOUR CLIENT ID/FederationMetadata/2007-06/FederationMetadata.xml",
            Wtrealm = "urn:MyApp",
            Notifications = new WsFederationAuthenticationNotifications
            {
                SecurityTokenValidated = notification =>
                {
                    notification.AuthenticationTicket.Identity.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Auth0"));
                    return Task.FromResult(true);
                }
            }
        });
    }
}
```


Finally the Account controller will start the Login flow to Auth0 and will also process the response coming from Auth0.

```
public ActionResult Login(string returnUrl)
{
    return new ChallengeResult("Federation", Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
}

[AllowAnonymous]
public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
{
    var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
    if (loginInfo == null)
    {
        return RedirectToAction("Login");
    }

    var externalIdentity = await AuthenticationManager.GetExternalIdentityAsync(DefaultAuthenticationTypes.ExternalCookie);
    if (externalIdentity == null)
    {
        throw new Exception("Could not get the external identity.");
    }

    AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, CreateIdentity(externalIdentity));
    return RedirectToLocal(returnUrl);
}
```
