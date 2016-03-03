# Auth0 + ASP.NET MVC OWIN - Server Side Login Sample

Auth0 samples typically show how to initiate a login transaction using client-side JavaScript (eg: social login, username/password, ...). This sample shows how you can achieve all of that with server-side code only:

 - Username/Password authentication with SSO support (by redirecting to Auth0)
 - Social/Enterprise login

## Running the example
In order to run the example you need to have Visual Studio 2013 installed.

You also need to set the ClientSecret, ClientId and Domain of your Auth0 app in the web.config file. To do that just look for the following keys and enter the right content:
```CSharp
<add key="auth0:Domain" value="{DOMAIN}" />
<add key="auth0:ClientID" value="{CLIENT_ID}"/>
<add key="auth0:ClientSecret" value="{CLIENT_SECRET}"/>
```

After that just press **F5** to run the application.

## How it works

This application has been configured as a standard ASP.NET MVC application with Auth0, but all of the UI is implemented using ASP.NET MVC Controllers and Views.

### Social / Enterprise Login

In order to support Social / Enterprise connections we can create buttons on the login page:

```csharp
@using Microsoft.Owin.Security
@model ServerSideLogin.Models.LoginViewModel

<h4>Use another service to log in.</h4>
<hr />
@{
    using (Html.BeginForm("ExternalLogin", "Account", new { ReturnUrl = Model.ReturnUrl }))
    {
        @Html.AntiForgeryToken()
        <div id="socialLoginList">
            <p>
                <button type="submit" class="btn btn-default" id="facebook" name="connection" value="facebook" title="Log in using your Facebook account">Facebook</button>
                <button type="submit" class="btn btn-default" id="google-oauth2" name="connection" value="google-oauth2" title="Log in using your Google account">Google</button>
                <button type="submit" class="btn btn-default" id="twitter" name="connection" value="twitter" title="Log in using your Twitter account">Twitter</button>
            </p>
        </div>
    }
}
```

After clicking a button the user is then redirect to Auth0 where the login transaction will be started:

```csharp
// POST: /Account/ExternalLogin
[HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
public ActionResult ExternalLogin(string connection, string returnUrl)
{
    var redirectUri = Request.Url.Scheme + "://" + Request.Url.Authority + "/signin-auth0";

    return Redirect(auth0.BuildAuthorizationUrl()
        .WithClient(System.Configuration.ConfigurationManager.AppSettings["auth0:ClientId"])
        .WithConnection(connection)
        .WithRedirectUrl(redirectUri.ToString())
        .WithState("ru=" + returnUrl)
        .WithResponseType(Auth0.AuthenticationApi.Models.AuthorizationResponseType.Code)
        .WithScope("openid name email")
        .Build()
        .ToString());
}
```

### Username Password Authentication

In order to support Database Connections we start by creating a View where the credentials are captured:

```csharp
@using (Html.BeginForm("Login", "Account", new { ReturnUrl = ViewBag.ReturnUrl }, FormMethod.Post, new { @class = "form-horizontal", role = "form" }))
{
    @Html.AntiForgeryToken()
    <h4>Use a local account to log in.</h4>
    <hr />
    @Html.ValidationSummary(true)
    <div class="form-group">
        @Html.LabelFor(m => m.UserName, new { @class = "col-md-2 control-label" })
        <div class="col-md-10">
            @Html.TextBoxFor(m => m.UserName, new { @class = "form-control" })
            @Html.ValidationMessageFor(m => m.UserName)
        </div>
    </div>
    <div class="form-group">
        @Html.LabelFor(m => m.Password, new { @class = "col-md-2 control-label" })
        <div class="col-md-10">
            @Html.PasswordFor(m => m.Password, new { @class = "form-control" })
            @Html.ValidationMessageFor(m => m.Password)
        </div>
    </div>
    <div class="form-group">
        <div class="col-md-offset-2 col-md-10">
            <input type="submit" value="Log in" class="btn btn-default" />
        </div>
    </div>
}
```

These credentials are then passed along to Auth0 through the AccountController:

```csharp
public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
{
    if (ModelState.IsValid)
    {
        try
        {
            var redirectUri = Request.Url.Scheme + "://" + Request.Url.Authority + "/signin-auth0";

            var response = await auth0.UsernamePasswordLoginAsync(new Auth0.AuthenticationApi.Models.UsernamePasswordLoginRequest()
            {
                Username = model.UserName,
                Tenant = System.Configuration.ConfigurationManager.AppSettings["auth0:Domain"].Split('.')[0],
                ClientId = System.Configuration.ConfigurationManager.AppSettings["auth0:ClientId"],
                Connection = "Username-Password-Authentication", // You can change this to the name of your database.
                ResponseType = "code",
                RedirectUri = redirectUri.ToString(),
                Password = model.Password,
                Scope = "openid email"
            });

            return View("LoginSuccess", new LoginSuccessViewModel()
            {
                HtmlForm = response.HtmlForm
            });
        }
        catch (ApiException apiException)
        {
            ModelState.AddModelError("", apiException.ApiError.Message);
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "Internal server error.");
        }
    }

    return View("Login", model);
}
``` 

The call to `UsernamePasswordLoginAsync` returns an Html form containing a temporary token. Posting this to Auth0 will complete the authentication transaction, set the SSO cookie in Auth0 and redirect back to the application. The redirect is handled in the `LoginSuccess` view:

```csharp
@model ServerSideLogin.Models.LoginSuccessViewModel

@{
    ViewBag.Title = "Log in";
}

<h2>Signing in...</h2>
<div class="row">
    <div class="col-md-8">
        You are being signed in...
    </div>

    @Html.Raw(Model.HtmlForm)
</div>
@section Scripts {
    @Scripts.Render("~/bundles/jquery")
    <script type="text/javascript">
        $(function() {
            $('form').submit();
        });
    </script>
}
```