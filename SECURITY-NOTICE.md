# Security vulnerability details for all versions of auth0-aspnet-owin

All versions of the `Auth-ASPNET-Owin` package have a security vulnerability that leave client applications vulnerable to a Cross-Site Request Forgery (CSRF) attack during authorization and authentication operations.

Root cause of this vulnerability is lack of use and verification of the `state` parameter in OAuth 2.0 and OpenID Connect protocols that allows an attacker to inject their authorization code into victim's session.

# Migration

Further development of the `Auth-ASPNET-Owin` package has been discontinued. We strongly recommend moving to OWIN 4 and the official `Microsoft.Owin.Security.OpenIdConnect` package that is not vulnerable.

See the [migration guide](https://auth0.com/docs/quickstart/webapp/aspnet-owin/04-migrating) for more details.
