using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Collections;
using System.Net;
using NewsAggregatorAPI.Controllers;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;

namespace NewsAggregatorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public ValuesController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet("recommend-news/{userID}")]
        public async Task<IActionResult> RecommendNews(string userID)
        {
            // Placeholder logic to mock news recommendations based on userId
            var recommendedArticles = await GetNewsFromNewsAPI(userID);

            if (recommendedArticles == null || !recommendedArticles.Any())
            {
                return NotFound($"No news recommendations found for user {userID}.");
            }

            return Ok(recommendedArticles);
        }

        // Method to get news articles from NewsAPI
        private async Task<List<NewsArticle>> GetNewsFromNewsAPI(string query)
        {
            // NewsAPI key
            var apiKey = "aa61e24e281e46d08e2efec86d16a5f3";

            // Build the NewsAPI request URL
            var requestUrl = $"https://newsapi.org/v2/everything?q={query}&from={DateTime.Now:yyyy-MM-dd}&sortBy=popularity&language=en&apiKey={apiKey}";

            // Create the request message
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            // Add the User-Agent header (as required by NewsAPI)
            request.Headers.Add("User-Agent", "NewsAggregatorApp/1.0");

            // Send the HTTP request
            var response = await _httpClient.SendAsync(request);

            // Check if the response was successful
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                // Return a List<NewsArticle> with an error message
                return new List<NewsArticle>
                {
                    new NewsArticle
                    {
                        Title = "Error fetching news",
                        Description = errorContent,
                        Url = "",
                        PublishedAt = null
                    }
                };
            }

            // Read the response content
            var jsonResponse = await response.Content.ReadAsStringAsync();

            // Deserialize the JSON response into a NewsApiResponse object
            var newsData = JsonSerializer.Deserialize<NewsApiResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var articles = new List<NewsArticle>();
            if (newsData?.Articles != null)
            {
                foreach (var article in newsData.Articles)
                {
                    // Create a NewsArticle object and add it to the list
                    articles.Add(new NewsArticle
                    {
                        Title = article.Title,
                        Description = article.Description,
                        Url = article.Url,
                        PublishedAt = article.PublishedAt
                    });
                }
            }

            return articles;
        }

        // Model to represent the NewsAPI response
        private class NewsApiResponse
        {
            public List<Article> Articles { get; set; }
        }

        private class Article
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
            public DateTime? PublishedAt { get; set; }
        }

        // NewsArticle class to hold the article information
        public class NewsArticle
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
            public DateTime? PublishedAt { get; set; }
        }
    }
}