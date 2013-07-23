Owin/Katana Authentication Handler for Auth0. Plug into the ASP.NET 4.5 Owin infrastructure (middleware) and extends default providers with more social providers like Amazon, Facebook, GitHub, LinkedIn, LiveId Google, Twitter, Paypal, vKontakte and Enterprise provides like any IdP that speaks SAML Protocol, ADFS, Google Apps, ADFS, Windows Azure AD, etc. 

## Installation

	Install-Package Auth0-ASPNET-45 -Pre

> Note: Katana is still in pre release, that's why this package won't be listed and can be installed only with `-Pre`

## Usage

1. Create a new ASP.NET 4.5 Project
2. Edit `Startup.Auth.cs`

~~~c#
public void ConfigureAuth(IAppBuilder app)
{
    // add google
    app.AddAuth0Authentication(clientId: "...your_auth0_clientid...", clientSecret: "...your_auth0_clientsecret...", domain: "youraccount.auth0.com", connection: "google-oauth2", displayName: "Google");
    
    // add linkedin
    app.AddAuth0Authentication(clientId: "...your_auth0_clientid...", clientSecret: "...your_auth0_clientsecret...", domain: "youraccount.auth0.com", connection: "linkedin", displayName: "LinkedIn");


    // add an enterprise connection like ADFS, SAML, Windows Azure AD, etc.
    app.AddAuth0Authentication(clientId: "...your_auth0_clientid...", clientSecret: "...your_auth0_clientsecret...", domain: "youraccount.auth0.com", connection: "bigenterprise.com", displayName: "Big Enterprise");
}
~~~

The clientid, secrets and connection names can be found on Auth0 dashboard (http://app.auth0.com).

## Customizing the Claims Identity

You can change/add new claims by attaching to OnAuthenticated

~~~c#
    var provider = new Microsoft.Owin.Security.Auth0.Auth0AuthenticationProvider
    {
        OnAuthenticated = async (context) =>
        {
            context.Identity.AddClaim(new System.Security.Claims.Claim("foo", "bar"));
            context.Identity.AddClaim("something", context.User["another_prop"].ToString());
            // context.User is a JObject with the original user object from Auth0
        }
    };

    // specify the provider
    app.AddAuth0Authentication(provider: provider, clientId: ... );
~~~
