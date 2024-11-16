using System;
using System.Net.Http;
using NewsAggregatorAPI.Controllers;
using TrendingTopicsMonitor.Services;

namespace YourNamespace
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // Initialize a shared HttpClient
            var httpClient = new HttpClient();

            // Initialize NewsAggregatorAPI service
            var newsAggregatorService = new ValuesController(httpClient);
            Application["NewsAggregatorService"] = newsAggregatorService;

            // Initialize TrendingTopicsMonitor service
            var trendingTopicsService = new TrendingTopicsService(httpClient);
            Application["TrendingTopicsService"] = trendingTopicsService;

            // Log the initialization
            System.Diagnostics.Debug.WriteLine("Services initialized in Application_Start.");
        }
    }
}
