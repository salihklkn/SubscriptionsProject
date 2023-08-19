using System.Web.Http;
using WebActivatorEx;
using SubscriptionsProject;
using Swashbuckle.Application;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace SubscriptionsProject
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;
            GlobalConfiguration.Configuration
              .EnableSwagger(c => c.SingleApiVersion("v1", "SubscriptionsProject"))
              .EnableSwaggerUi();
        }
    }
}
