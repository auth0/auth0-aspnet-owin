using System;
using Microsoft.AspNet.Http.Security;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Security.Cookies;
using Microsoft.AspNet.Security.OpenIdConnect;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Core.Collections;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;

namespace AspNet5.WebApp.OpenIdConnect.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (Context.User == null || !Context.User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(OpenIdConnectAuthenticationDefaults.AuthenticationType, new AuthenticationProperties { RedirectUri = "/" });
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// When authenticating using the OpenID Connect middleware the nonce is required by default (but this can be turned off) 
        /// and the state is always required. These values are always generated server side and the following action exposes this
        /// logic to your javascript code.
        /// 
        /// This is useful when the authentication flow needs to start from the Lock.
        /// </summary>
        /// <returns></returns>
	    [HttpPost]
        public IActionResult Prepare()
        {
            var middlewareOptions = Context.ApplicationServices.GetService<IOptions<OpenIdConnectAuthenticationOptions>>();

            // Generate the nonce.
            var nonce = middlewareOptions.Options.ProtocolValidator.GenerateNonce();

            // Store it in the cache or in a cookie.
            if (middlewareOptions.Options.NonceCache != null)
                middlewareOptions.Options.NonceCache.AddNonce(nonce);
            else
                Response.Cookies.Append(
                    ".AspNet.OpenIdConnect.Nonce." + middlewareOptions.Options.StringDataFormat.Protect(nonce), "N",
                        new CookieOptions { HttpOnly = true, Secure = Request.IsSecure });

            // Generate the state.
            var state = "OpenIdConnect.AuthenticationProperties=" +
                        Uri.EscapeDataString(middlewareOptions.Options.StateDataFormat.Protect(new AuthenticationProperties(
                            Request.Form.ToDictionary(i => i.Key, i => i.Value?.FirstOrDefault()))));

            // Return nonce to the Lock.
            return Json(new { nonce, state });
        }

        // GET: /Account/LogOff
        [HttpGet]
        public IActionResult LogOff()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                Context.Response.SignOut(new List<string>()
                {
                    OpenIdConnectAuthenticationDefaults.AuthenticationType,
                    CookieAuthenticationDefaults.AuthenticationType
                });
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Manage
        [HttpGet]
        public IActionResult Manage()
        {
            return View();
        }
    }
}