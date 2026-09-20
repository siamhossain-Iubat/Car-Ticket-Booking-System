using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CarTicketBookingSystem.Services
{
    public interface ISmsSender
    {
        Task<bool> SendAsync(string phone, string message);
    }

    public sealed class SmsNetBdSender : ISmsSender
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsNetBdSender> _logger;

        public SmsNetBdSender(HttpClient http, IConfiguration configuration, ILogger<SmsNetBdSender> logger)
        {
            _http = http;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendAsync(string phone, string message)
        {
            var apiKey = _configuration["SmsNetBd:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(_configuration["SmsNetBd:Enabled"]) ||
                !bool.TryParse(_configuration["SmsNetBd:Enabled"], out var enabled) || !enabled)
            {
                _logger.LogInformation("SMS sending is disabled or API key is not configured.");
                return false;
            }

            var values = new Dictionary<string, string>
            {
                ["api_key"] = apiKey,
                ["msg"] = message,
                ["to"] = phone
            };

            using var content = new FormUrlEncodedContent(values);
            using var response = await _http.PostAsync("https://api.sms.net.bd/sendsms", content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("SMS provider returned HTTP status {StatusCode}.", (int)response.StatusCode);
                return false;
            }

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(responseText);
                var root = doc.RootElement;
                if (root.TryGetProperty("error", out var error) && error.GetInt32() == 0)
                    return true;
                _logger.LogWarning("SMS provider rejected request. Provider error code: {ErrorCode}.", error.ToString());
            }
            catch (System.Text.Json.JsonException)
            {
                _logger.LogWarning("SMS provider returned an unreadable response.");
            }

            return false;
        }
    }
}
