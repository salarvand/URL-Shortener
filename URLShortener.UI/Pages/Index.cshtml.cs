using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using URLShortener.Application.Models;

namespace URLShortener.UI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IndexModel> _logger;

        [BindProperty]
        public CreateShortUrlDto CreateModel { get; set; }

        public ShortUrlDto? ShortUrl { get; set; }
        public IEnumerable<ShortUrlDto> RecentUrls { get; set; } = new List<ShortUrlDto>();
        public string BaseUrl { get; set; } = string.Empty;

        public IndexModel(
            IHttpClientFactory clientFactory,
            IConfiguration configuration,
            ILogger<IndexModel> logger)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
            _logger = logger;
            CreateModel = new CreateShortUrlDto();
        }

        public async Task OnGetAsync()
        {
            await LoadRecentUrlsAsync();
            BaseUrl = GetBaseUrl();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Even if there are errors, we should still load recent URLs for consistent UX
            await LoadRecentUrlsAsync();
            BaseUrl = GetBaseUrl();
            
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var client = _clientFactory.CreateClient("API");
                var response = await client.PostAsJsonAsync("api/shorturl", CreateModel);

                if (response.IsSuccessStatusCode)
                {
                    ShortUrl = await response.Content.ReadFromJsonAsync<ShortUrlDto>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    
                    // Try to extract the error message if it's a JSON response
                    try 
                    {
                        // Some APIs return error details in JSON format
                        using var doc = JsonDocument.Parse(errorContent);
                        if (doc.RootElement.TryGetProperty("message", out var messageElement))
                        {
                            errorContent = messageElement.GetString() ?? errorContent;
                        }
                    }
                    catch
                    {
                        // If parsing fails, use the original error content
                    }
                    
                    // Add appropriate error based on status code
                    switch (response.StatusCode)
                    {
                        case System.Net.HttpStatusCode.BadRequest:
                            // If error is about custom short code being in use
                            if (errorContent.Contains("already in use"))
                            {
                                ModelState.AddModelError("CreateModel.CustomShortCode", errorContent);
                            }
                            // If error is about invalid URL format
                            else if (errorContent.Contains("URL format") || errorContent.Contains("URL cannot be empty"))
                            {
                                ModelState.AddModelError("CreateModel.OriginalUrl", errorContent);
                            }
                            else
                            {
                                ModelState.AddModelError("", errorContent);
                            }
                            break;
                        default:
                            ModelState.AddModelError("", $"Error: {response.StatusCode}. {errorContent}");
                            break;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error connecting to API");
                ModelState.AddModelError("", "Cannot connect to the server. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating short URL");
                ModelState.AddModelError("", "An error occurred while creating the short URL. Please try again.");
            }

            return Page();
        }

        private async Task LoadRecentUrlsAsync()
        {
            try
            {
                var client = _clientFactory.CreateClient("API");
                var urls = await client.GetFromJsonAsync<IEnumerable<ShortUrlDto>>("api/shorturl");
                if (urls != null)
                {
                    RecentUrls = urls;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recent URLs");
                RecentUrls = new List<ShortUrlDto>();
            }
        }

        private string GetBaseUrl()
        {
            var apiBaseUrl = _configuration["ApiBaseUrl"];
            if (!string.IsNullOrEmpty(apiBaseUrl))
            {
                return apiBaseUrl;
            }

            return $"{Request.Scheme}://{Request.Host}";
        }
    }
}
