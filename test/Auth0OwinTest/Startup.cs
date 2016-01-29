using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Auth0OwinTest.Startup))]
namespace Auth0OwinTest
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
