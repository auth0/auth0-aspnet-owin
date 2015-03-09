# Auth0 + ASP.NET 5 + Web API Seed
This is the seed project you need to use if you're going to create an ASP.NET 5 Web API backend.

# Running the example
In order to run the example you need to have Visual Studio 2015 CTP 6 installed.

You also need to set the `Domain` and `ClientId` of your Auth0 app in the `config.json` file. To do that just look for the following settings and enter the right content:

```js
{
	"Auth0": {
		"ClientId":  "{CLIENT_ID}",
		"Domain": "{DOMAIN}"
	}
}
```

After that just press **F5** to run the application.

### IMPORTANT

Currently, the OAuth Bearer middleware is not supporting JWT tokens signed with symmetric keys, so we need to make sure to configure RSA algorithm from Auth0 dashboard:

1. Go to https://manage.auth0.com/#/applications/{YOUR_AUTH0_CLIENT_ID}/settings
2. Click on `Show Advanced Settings` button.
3. Set `RS256` as `JsonWebToken Token Signature Algorithm` and click on `Save`.
