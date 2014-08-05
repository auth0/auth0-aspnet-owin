using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MvcSample.Startup))]
namespace MvcSample
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
