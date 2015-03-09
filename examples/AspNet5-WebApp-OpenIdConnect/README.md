# Auth0 + ASP.NET 5 + MVC 6 (OpenID Connect) Seed
This is the seed project you need to use if you're going to create an ASP.NET 5 Web App with OpenID Connect.

# Running the example
In order to run the example you need to have Visual Studio 2015 CTP 6 installed.

You also need to set the `Domain` and `ClientId` of your Auth0 app in the `config.json` file. To do that just look for the following settings and enter the right content:

```js
{
	"Auth0": {
		"ClientId":  "{CLIENT_ID}",
		"Domain": "{DOMAIN}",
		"PostLogoutRedirectUri":  "http://localhost:49848/"
	}
}
```

After that just press **F5** to run the application.