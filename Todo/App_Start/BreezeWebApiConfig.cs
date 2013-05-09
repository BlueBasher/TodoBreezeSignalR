using System.Web.Http;
using Breeze.WebApi;
using System.Web.Routing;

[assembly: WebActivator.PreApplicationStartMethod(
    typeof(Todo.App_Start.BreezeWebApiConfig), "RegisterBreezePreStart")]
namespace Todo.App_Start
{
    ///<summary>
    /// Inserts the Breeze Web API controller route at the front of all Web API routes
    ///</summary>
    ///<remarks>
    /// This class is discovered and run during startup; see
    /// http://blogs.msdn.com/b/davidebb/archive/2010/10/11/light-up-your-nupacks-with-startup-code-and-webactivator.aspx
    ///</remarks>
    public static class BreezeWebApiConfig
    {

        public static void RegisterBreezePreStart()
        {

            // CORS enabled on this server
            GlobalConfiguration.Configuration.MessageHandlers.Add(new BreezeSimpleCorsHandler());

            // Map the routes for SignalR-hubs
            RouteTable.Routes.MapHubs();

            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                name: "BreezeApi",
                routeTemplate: "api/{controller}/{action}"
            );
        }
    }
}