# Auth0 + ASP.NET OWIN Seed
This is the seed project you need to use if you're going to create an ASP.NET application using OWIN.

#Running the example
In order to run the example you need to have Visual Studio 2013/2015 installed.

You also need to set the ClientSecret, ClientId and Domain of your Auth0 app in the web.config file. To do that just look for the following keys and enter the right content:
```CSharp
<add key="auth0:Domain" value="{DOMAIN}" />
<add key="auth0:ClientID" value="{CLIENT_ID}"/>
<add key="auth0:ClientSecret" value="{CLIENT_SECRET}"/>
```

After that just press **F5** to run the application.
