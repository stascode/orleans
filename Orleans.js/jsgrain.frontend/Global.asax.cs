using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace frontend
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            #region OrleansClientInit
            // Todo: Make client initialization cleaner. 
            // This wait a temporary hack to wait for silo to come up before starting the client.
            
            System.Threading.Thread.Sleep(15 * 1000);
            Orleans.OrleansClient.Initialize(Server.MapPath("~/DevTestClientConfiguration.xml"));
            
            #endregion

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
