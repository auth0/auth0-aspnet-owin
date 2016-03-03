using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ServerSideLogin.Startup))]
namespace ServerSideLogin
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}