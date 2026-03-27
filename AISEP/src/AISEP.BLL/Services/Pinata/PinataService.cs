using AISEP.BLL.DTOs.Requests;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AISEP.BLL.Services.Pinata
{
    public class PinataService : IPinataService
    {
        private const string PinataPinJsonEndpoint = "https://api.pinata.cloud/pinning/pinJSONToIPFS";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PinataService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<string> UploadJsonToIpfsAsync(NftMetadataDto metadata)
        {
            if (metadata is null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            var pinataApiKey = _configuration["Pinata:PinataApiKey"];
            var pinataSecretApiKey = _configuration["Pinata:PinataSecretApiKey"];

            if (string.IsNullOrWhiteSpace(pinataApiKey) || string.IsNullOrWhiteSpace(pinataSecretApiKey))
            {
                throw new InvalidOperationException("Pinata API key or secret key is missing in configuration.");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("pinata_api_key", pinataApiKey);
                client.DefaultRequestHeaders.Add("pinata_secret_api_key", pinataSecretApiKey);

                var metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null
                });

                using var content = new StringContent(metadataJson, Encoding.UTF8, "application/json");
                using var request = new HttpRequestMessage(HttpMethod.Post, PinataPinJsonEndpoint)
                {
                    Content = content
                };

                var response = await client.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        $"Pinata upload failed with status {(int)response.StatusCode}: {responseContent}");
                }

                using var jsonDocument = JsonDocument.Parse(responseContent);
                if (!jsonDocument.RootElement.TryGetProperty("IpfsHash", out var ipfsHashElement))
                {
                    throw new InvalidOperationException("Pinata response does not contain IpfsHash.");
                }

                var ipfsHash = ipfsHashElement.GetString();
                if (string.IsNullOrWhiteSpace(ipfsHash))
                {
                    throw new InvalidOperationException("Pinata returned an empty IpfsHash.");
                }

                return ipfsHash;
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to connect to Pinata API: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new InvalidOperationException($"Pinata request timed out: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to parse Pinata response: {ex.Message}", ex);
            }
        }
    }
}
