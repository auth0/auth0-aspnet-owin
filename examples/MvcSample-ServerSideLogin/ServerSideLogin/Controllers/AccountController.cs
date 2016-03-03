using System;
using System.Web.Mvc;
using System.Threading.Tasks;
using ServerSideLogin.Models;

using Auth0.Core.Exceptions;
using Auth0.AuthenticationApi;

namespace ServerSideLogin.Controllers
{
    public class AccountController : Controller
    {
        private AuthenticationApiClient auth0;

        public AccountController()
        {
            auth0 = new AuthenticationApiClient(new System.Uri("https://" + System.Configuration.ConfigurationManager.AppSettings["auth0:Domain"]));
        }
        
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            return View("Login", new LoginViewModel()
            {
                ReturnUrl = returnUrl
            });
        }
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
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
    }
}