## Overview

A helper class which will resolve the Issues Signin Key for JSON Web Tokens when using the standard OWIN JWT middleware. This is meant to be used when using RS256 signed JSON Web Tokens issued by Auth0.

The public key will automatically be downloaded from the OIDC well-known endpoint.

## Usage

```
var keyResolver = new OpenIdConnectSigningKeyResolver("https://your_tenant.auth0.com");
app.UseJwtBearerAuthentication(
    new JwtBearerAuthenticationOptions
    {
        AuthenticationMode = AuthenticationMode.Active,
        TokenValidationParameters = new TokenValidationParameters()
        {
            ValidAudience = apiIdentifier,
            ValidIssuer = domain,
            IssuerSigningKeyResolver = (token, securityToken, identifier, parameters) => keyResolver.GetSigningKey(identifier)
        }
    });
```