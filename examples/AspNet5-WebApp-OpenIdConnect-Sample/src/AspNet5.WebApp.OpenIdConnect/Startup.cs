using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Diagnostics.Entity;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Security;
using Microsoft.AspNet.Security.Cookies;
using Microsoft.AspNet.Security.OpenIdConnect;
using Microsoft.Data.Entity;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Logging.Console;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Security.Principal;

namespace AspNet5.WebApp.OpenIdConnect
{
  public class Startup
  {
    public Startup(IHostingEnvironment env)
    {
      // Setup configuration sources.
      Configuration = new Configuration()
          .AddJsonFile("config.json")
          .AddEnvironmentVariables();
    }

    public IConfiguration Configuration { get; set; }

    // This method gets called by the runtime.
    public void ConfigureServices(IServiceCollection services)
    {
    	// Add MVC services to the services container.
    	services.AddMvc();
    	
    	// OpenID Connect Authentication Requires Cookie Auth
			services.Configure<ExternalAuthenticationOptions>(options =>
			{
				options.SignInAsAuthenticationType = CookieAuthenticationDefaults.AuthenticationType;
			});
    }

    // Configure is called after ConfigureServices is called.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory)
    {
      // Configure the HTTP request pipeline.
      // Add the console logger.
      loggerfactory.AddConsole();

      // Add the following to the request pipeline only in development environment.
      if (string.Equals(env.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
      {
        app.UseBrowserLink();
        app.UseErrorPage(ErrorPageOptions.ShowAll);
        app.UseDatabaseErrorPage(DatabaseErrorPageOptions.ShowAll);
      }
      else
      {
        // Add Error handling middleware which catches all application specific errors and
        // send the request to the following path or controller action.
        app.UseErrorHandler("/Home/Error");
      }

      // Add static files to the request pipeline.
      app.UseStaticFiles();

      // Configure the OWIN Pipeline to use OpenID Connect Authentication
			app.UseCookieAuthentication(options => { });

			app.UseOpenIdConnectAuthentication(options =>
			{
				options.ClientId = Configuration.Get("Auth0:ClientId");
				options.Authority = Configuration.Get("Auth0:Domain");
				options.PostLogoutRedirectUri = Configuration.Get("Auth0:PostLogoutRedirectUri");
				options.ResponseType = "token";
				options.Notifications = new OpenIdConnectAuthenticationNotifications
				{
					// OPTIONAL: you can read/modify the claims that are populated based on the JWT
					SecurityTokenValidated = context =>
					{
						var claimsIdentity = context.AuthenticationTicket.Principal.Identity as ClaimsIdentity;

						// add Auth0 access_token as claim
						claimsIdentity.AddClaim(new Claim("access_token", context.ProtocolMessage.AccessToken));

						// ensure name claim
						claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, claimsIdentity.FindFirstValue("name")));

						return Task.FromResult(0);
					}
				};
      });

			// Add MVC to the request pipeline.
			app.UseMvc(routes =>
      {
        routes.MapRoute(
          name: "default",
          template: "{controller}/{action}/{id?}",
          defaults: new { controller = "Home", action = "Index" });

        // Uncomment the following line to add a route for porting Web API 2 controllers.
        // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
      });
    }
  }
}
