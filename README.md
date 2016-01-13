Owin/Katana Authentication Handler for Auth0. Plugs into the ASP.NET 4.5 Owin infrastructure (middleware) and extends default providers with more social providers such as Amazon, Facebook, GitHub, LinkedIn, LiveId, Google, Twitter, PayPal and vKontakte. Also integrates with Enterprise providers like any IdP that speaks SAML Protocol, ADFS, Google Apps, ADFS, Windows Azure AD, etc.

> Note: ASP.NET 5 support is available in the following repository https://github.com/auth0/auth0-aspnet5

## Installation

    Install-Package Auth0-ASPNET-Owin

## Usage

[Please see this NuGet's README.](nuget/README.txt)

## Examples

In this repository we've also included different Owin samples, using the Auth0 Authentication Handler or standards based (Bearer token, OpenID Connect, ...) handlers.

 - [ASP.NET 4 - MVC sample with Auth0-ASPNET-OWIN](https://github.com/auth0/auth0-aspnet-owin/tree/master/examples/MvcSample)
 - [ASP.NET 4 - MVC seed project with Auth0-ASPNET-OWIN](https://github.com/auth0/auth0-aspnet-owin/tree/master/examples/basic-mvc-sample)
 - [ASP.NET 4 - Web API sample with Bearer Tokens](https://github.com/auth0/auth0-aspnet-owin/tree/master/examples/WebApi)

 ## Issue Reporting

 If you have found a bug or if you have a feature request, please report them at this repository issues section. Please do not report security vulnerabilities on the public GitHub issue tracker. The [Responsible Disclosure Program](https://auth0.com/whitehat) details the procedure for disclosing security issues.

 ## Author

 [Auth0](auth0.com)

 ## License

 This project is licensed under the MIT license. See the [LICENSE](LICENSE) file for more info.
