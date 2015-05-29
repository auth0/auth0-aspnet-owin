# SAML ASP.NET MVC Sample

This sample shows how to integrate your ASP.NET MVC application with Auth0 using SAML (without using the Auth0 SDKs). 

## Configuring the application in Auth0

1. Create a new application in Auth0
2. Enable the SAML addon on the Addons tab
3. In the settings of the SAML addon, specify the callback url `http://localhost:3500/AuthServices/Acs` and these settings:
```
{
  "audience":  "urn:MyApp",
}
```

## Configuring the ASP.NET MVC application. 

This sample uses the **Kentor.AuthServices.Owin** NuGet package for SAML support. The library needs to be configured at application startup:

```
public partial class Startup
{
    public void Configuration(IAppBuilder app)
    {
        app.UseCookieAuthentication(new CookieAuthenticationOptions
        {
            AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
            LoginPath = new PathString("/Account/Login")
        });
        app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
        app.UseKentorAuthServicesAuthentication(CreateAuthServicesOptions());
    }

    private static KentorAuthServicesAuthenticationOptions CreateAuthServicesOptions()
    {
        var authServicesOptions = new KentorAuthServicesAuthenticationOptions(false)
        {
            SPOptions = new SPOptions 
            { 
              EntityId = new EntityId("urn:MyApp"), 
              ReturnUrl = new Uri("http://localhost:3500/") 
            }
        };
        authServicesOptions.IdentityProviders.Add(new IdentityProvider(new EntityId("urn:YOUR-TENANT.auth0.com"), 
                authServicesOptions.SPOptions)
            {
                AllowUnsolicitedAuthnResponse = true,
                MetadataUrl = new Uri("https://YOUR-TENANT.auth0.com/samlp/metadata/YOUR-CLIENT-ID"),
                Binding = Saml2BindingType.HttpPost
            });
        return authServicesOptions;
    }
}
```

Note that the EntityId needs to match the audience you configured in Auth0.

Finally the Account controller will start the Login flow to Auth0 and will also process the response coming from Auth0.

```
public ActionResult Login(string returnUrl)
{
    return new ChallengeResult("KentorAuthServices", Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
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
