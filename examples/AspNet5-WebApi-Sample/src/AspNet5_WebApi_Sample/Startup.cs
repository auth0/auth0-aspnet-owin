using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Security.OAuthBearer;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNet5_WebApi_Sample
{
	public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
			Configuration = new Configuration()
				.AddJsonFile("config.json");
		}

		public IConfiguration Configuration { get; set; }

		// This method gets called by a runtime.
		// Use this method to add services to the container
		public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
			// Configure the app to use OAuth Bearer Authentication
			app.UseOAuthBearerAuthentication(options =>
			{
				options.Audience = Configuration.Get("Auth0:ClientId");
				options.Authority = "https://" + Configuration.Get("Auth0:Domain");
				options.Notifications = new OAuthBearerAuthenticationNotifications
				{
					// OPTIONAL: you can read/modify the claims that are populated based on the JWT
					// Check issue status: https://github.com/aspnet/Security/issues/140
					/*SecurityTokenValidated = context =>
					{
						var claimsIdentity = context.AuthenticationTicket.Principal.Identity as ClaimsIdentity;

						// ensure name claim
						claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, claimsIdentity.FindFirst("name").Value));

						return Task.FromResult(0);
					}*/
				};
			});

			app.UseStaticFiles();

            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
