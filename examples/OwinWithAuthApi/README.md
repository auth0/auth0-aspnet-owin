# Auth0 Authentication API + OWIN
This sample demonstrates how you can use the [Auth0 Authentication API](https://auth0.com/docs/auth-api) to authenticate a user using your custom login form, and not using the Auth0 Lock component at all.

## Running the example
In order to run the example you need to have Visual Studio 2015 installed.

You also need to set the ClientId and Domain of your Auth0 app in the web.config file. To do that just look for the following keys and enter the right content:
```CSharp
<add key="auth0:Domain" value="{DOMAIN}" />
<add key="auth0:ClientID" value="{CLIENT_ID}"/>
```

After that just press **F5** to run the application.

## Things of note

The Authentication API is in the [Auth0.AuthenticationApi NuGet package](https://www.nuget.org/packages/Auth0.AuthenticationApi), so be sure to install that:

```
Install-Package Auth0.AuthenticationApi
```

You also need to ensure that you have Cookie authentication enabled:

```
app.UseCookieAuthentication(new CookieAuthenticationOptions
{
    AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
    LoginPath = new PathString("/Account/Login"),
    Provider = new CookieAuthenticationProvider()
});           
```

The next important bit is the `Login` action in the `AccountController` class. To authenticate the user, you will first need to instantiate an instance of the `AuthenticationApiClient`:

```
var auth = new Auth0.AuthenticationApi.AuthenticationApiClient(new Uri("https://YOUR_AUTH0_DOMAIN/"));
```

Next, you can authenticate the user:

```
var authResponse = await auth.Authenticate(new AuthenticationRequest
{
    Connection = "Username-Password-Authentication",
    ClientId = ConfigurationManager.AppSettings["auth0:ClientId"],
    Scope = "openid name email",
    Username = model.Email,
    Password = model.Password
});
```

This will contain the JWT token, which you use next to obtain information about the user:

```
User user = await auth.GetTokenInfo(authResponse.IdToken);
```

Finally you need to construct a `ClaimsIdentity` and sign the user in:

```
ClaimsIdentity identity = new ClaimsIdentity(null, DefaultAuthenticationTypes.ApplicationCookie);
identity.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Auth0"));
identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UserId));
identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, user.FullName));
AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);
```

**It is important** to notice that you need to set all 3 the claims as demonstrated in the code above