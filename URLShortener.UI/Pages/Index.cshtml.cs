using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
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

        public ShortUrlDto ShortUrl { get; set; }
        public IEnumerable<ShortUrlDto> RecentUrls { get; set; }
        public string BaseUrl { get; set; }

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
            if (!ModelState.IsValid)
            {
                await LoadRecentUrlsAsync();
                BaseUrl = GetBaseUrl();
                return Page();
            }

            try
            {
                var client = _clientFactory.CreateClient("API");
                var response = await client.PostAsJsonAsync("api/shorturl", CreateModel);

                if (response.IsSuccessStatusCode)
                {
                    ShortUrl = await response.Content.ReadFromJsonAsync<ShortUrlDto>();
                    await LoadRecentUrlsAsync();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating short URL");
                ModelState.AddModelError("", "An error occurred while creating the short URL. Please try again.");
            }

            BaseUrl = GetBaseUrl();
            return Page();
        }

        private async Task LoadRecentUrlsAsync()
        {
            try
            {
                var client = _clientFactory.CreateClient("API");
                RecentUrls = await client.GetFromJsonAsync<IEnumerable<ShortUrlDto>>("api/shorturl");
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
