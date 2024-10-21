using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System;
using System.Threading;
using System.Text.Json.Serialization;

namespace TrendingTopicsMonitor.Services
{
    public class TrendingTopicsService
    {
        private readonly HttpClient _httpClient;
        private readonly string newsApiKey;
        private readonly string serpApiKey;
        private readonly string xApiKey;
        private readonly string xApiKeySecret;
        private readonly string xApiBearerToken;

        public TrendingTopicsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            newsApiKey = Environment.GetEnvironmentVariable("NewsAPIKey");
            serpApiKey = Environment.GetEnvironmentVariable("SerpAPI");
            xApiKey = Environment.GetEnvironmentVariable("XAPIKey");
            xApiKeySecret = Environment.GetEnvironmentVariable("XAPIKeySecret");
            xApiBearerToken = Environment.GetEnvironmentVariable("BearerToken");

            Console.WriteLine($"NewsAPI Key: {newsApiKey}");
            Console.WriteLine($"SerpAPI Key: {serpApiKey}");
            Console.WriteLine($"XAPI BearerToken: {xApiBearerToken}");
        }

        // Method to fetch trending news from NewsAPI
        public async Task<List<NewsArticle>> GetTrendingNews()
        {
            var requestUrl = $"https://newsapi.org/v2/top-headlines?country=us&apiKey={newsApiKey}";

            Console.WriteLine($"Request URL: {requestUrl}");
            Console.WriteLine($"User-Agent: NewsAggregatorApp/1.0");

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("User-Agent", "TrendingTopicsMonitor/1.0");

            var response = await _httpClient.SendAsync(request);

            Console.WriteLine($"Response Status Code: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                return new List<NewsArticle> { new NewsArticle { Title = "Error fetching news." } };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var newsData = JsonSerializer.Deserialize<NewsApiResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return newsData?.Articles ?? new List<NewsArticle>();
        }

        // Method to fetch trending topics from SerpAPI (Google Trends) for the USA
        public async Task<List<TrendingTopic>> GetTrendingTopicsFromSerpAPI()
        {
            var query = "trending";
            var requestUrl = $"https://serpapi.com/search.json?engine=google_trends&q={query}&data_type=RELATED_TOPICS&api_key={serpApiKey}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("User-Agent", "TrendingTopicsMonitor/1.0");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return new List<TrendingTopic> { new TrendingTopic { Title = "Error fetching trending topics.", Type = "Error", Value = "", Link = "" } };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            SerpApiResponse serpData;
            try
            {
                serpData = JsonSerializer.Deserialize<SerpApiResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deserialization error: {ex.Message}");
                return new List<TrendingTopic> { new TrendingTopic { Title = "Error processing response.", Type = "Error", Value = "", Link = "" } };
            }

            var trendingTopics = new List<TrendingTopic>();

            if (serpData.RelatedTopics?.Rising != null)
            {
                trendingTopics.AddRange(serpData.RelatedTopics.Rising
                    .Select(topic => new TrendingTopic
                    {
                        Title = topic.Topic.Title,
                        Type = topic.Topic.Type,
                        Value = topic.Value,
                        Link = topic.Link
                    }));
            }

            if (serpData.RelatedTopics?.Top != null)
            {
                trendingTopics.AddRange(serpData.RelatedTopics.Top
                    .Select(topic => new TrendingTopic
                    {
                        Title = topic.Topic.Title,
                        Type = topic.Topic.Type,
                        Value = topic.Value,
                        Link = topic.Link
                    }));
            }

            return trendingTopics;
        }

        // Model to represent the NewsAPI response
        private class NewsApiResponse
        {
            public List<NewsArticle> Articles { get; set; }
        }

        // Model to represent individual articles
        public class NewsArticle
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
            public DateTime? PublishedAt { get; set; }
        }

        //Model to represent the SerpAPI response
        private class SerpApiResponse
        {
            [JsonPropertyName("related_topics")]  // Add this attribute to ensure proper mapping
            public RelatedTopics RelatedTopics { get; set; }
        }

        private class RelatedTopics
        {
            [JsonPropertyName("rising")]  // Explicitly specify the JSON property names
            public List<TopicInfo> Rising { get; set; }

            [JsonPropertyName("top")]  // Explicitly specify the JSON property names
            public List<TopicInfo> Top { get; set; }
        }

        private class TopicInfo
        {
            [JsonPropertyName("topic")]  // Explicitly specify the JSON property names
            public TopicDetails Topic { get; set; }

            [JsonPropertyName("value")]  // Explicitly specify the JSON property names
            public string Value { get; set; }

            [JsonPropertyName("link")]  // Explicitly specify the JSON property names
            public string Link { get; set; }
        }

        private class TopicDetails
        {
            [JsonPropertyName("value")]  // Explicitly specify the JSON property names
            public string Value { get; set; }  // This is the internal value or ID of the topic

            [JsonPropertyName("title")]  // Explicitly specify the JSON property names
            public string Title { get; set; }  // The title of the topic

            [JsonPropertyName("type")]  // Explicitly specify the JSON property names
            public string Type { get; set; }   // The type of the topic (e.g., 'Food', 'Topic', etc.)
        }

        public class TrendingTopic
        {
            public string Title { get; set; }
            public string Type { get; set; }
            public string Value { get; set; }
            public string Link { get; set; }
        }
    }
}