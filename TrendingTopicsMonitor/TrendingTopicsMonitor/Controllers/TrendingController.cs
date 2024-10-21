using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TrendingTopicsMonitor.Services;

namespace TrendingTopicsMonitor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrendingController : ControllerBase
    {
        private readonly TrendingTopicsService _trendingTopicsService;

        public TrendingController(TrendingTopicsService trendingTopicsService)
        {
            _trendingTopicsService = trendingTopicsService;
        }

        // Endpoint to get trending news from NewsAPI
        [HttpGet("trending-news")]
        public async Task<IActionResult> GetTrendingNews()
        {
            var trendingNews = await _trendingTopicsService.GetTrendingNews();

            if (trendingNews == null || trendingNews.Count == 0)
            {
                return NotFound("No trending news available.");
            }

            return Ok(trendingNews);
        }

        // Endpoint to get trending topics from SerpAPI
        [HttpGet("Serp-Topics")]
        public async Task<IActionResult> GetTrendingTopicsSerp()
        {
            var trendingTopics = await _trendingTopicsService.GetTrendingTopicsFromSerpAPI();
            Console.WriteLine($"Number of trending topics fetched: {trendingTopics.Count}");
            if (trendingTopics == null || trendingTopics.Count == 0)
            {
                return NotFound("No trending topics available.");
            }

            return Ok(trendingTopics);
        }

        // Endpoint to get data from XAPI
        [HttpGet("xapi-data")]
        public async Task<IActionResult> GetDataFromXAPI()
        {
            var xApiData = await _trendingTopicsService.GetTrendingTopicsFromXAPI();

            if (xApiData == null || xApiData.Count == 0)
            {
                return NotFound("No data from XAPI available.");
            }

            return Ok(xApiData);
        }
    }
}