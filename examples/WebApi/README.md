# Auth0 + ASP.NET WebAPI OWIN Seed
This is the seed project you need to use if you're going to create an ASP.NET WebAPI application using OWIN.

This example is deployed in an Azure Web Site at https://auth0-aspnet-webapi-owin.azurewebsites.net.

#Running the example
In order to run the example you need to have Visual Studio 2013 installed.

You also need to set the ClientSecret, ClientId and Domain of your Auth0 app in the web.config file. To do that just look for the following keys and enter the right content:
```CSharp
<add key="Auth0Domain" value="{DOMAIN}" />
<add key="Auth0ClientID" value="{CLIENT_ID}"/>
<add key="Auth0ClientSecret" value="{CLIENT_SECRET}"/>
```

After that just press **F5** to run the application. It will start running in port **3001**. If you browse to [http://localhost:3001/ping](http://localhost:3001/ping) you should receive a response message.
