using System.Configuration;
using System.Web.Http;

namespace MvcSample
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MessageHandlers.Add(new MvcSample.App_Start.JsonWebTokenValidationHandler()
            {
                Audience = ConfigurationManager.AppSettings["auth0:ClientId"],
                SymmetricKey = ConfigurationManager.AppSettings["auth0:ClientSecret"]
            });

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}