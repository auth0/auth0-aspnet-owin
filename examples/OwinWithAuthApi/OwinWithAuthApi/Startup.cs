using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OwinWithAuthApi.Startup))]
namespace OwinWithAuthApi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
