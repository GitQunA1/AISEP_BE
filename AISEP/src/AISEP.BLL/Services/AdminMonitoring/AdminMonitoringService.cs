using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Settings;
using Microsoft.Extensions.Options;
using Nethereum.Web3;

namespace AISEP.BLL.Services.AdminMonitoring
{
    public class AdminMonitoringService : IAdminMonitoringService
    {
        private readonly GeminiSettings _geminiSettings;
        private readonly BlockchainSettings _blockchainSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminMonitoringService(
            IOptions<GeminiSettings> geminiSettings,
            IOptions<BlockchainSettings> blockchainSettings,
            IHttpClientFactory httpClientFactory)
        {
            _geminiSettings = geminiSettings.Value;
            _blockchainSettings = blockchainSettings.Value;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<AdminStatusResponse> GetStatusAsync()
        {
            var aiTask = CheckAiAsync();
            var blockchainTask = CheckBlockchainAsync();
            await Task.WhenAll(aiTask, blockchainTask);

            return new AdminStatusResponse
            {
                Ai = aiTask.Result,
                Blockchain = blockchainTask.Result
            };
        }

        private async Task<ServiceStatusResponse> CheckAiAsync()
        {
            try
            {
                var model = Uri.EscapeDataString(_geminiSettings.Model);
                var requestUri = $"{_geminiSettings.BaseUrl}/models/{model}?key={_geminiSettings.ApiKey}";

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(requestUri, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Kiem tra AI that bai: HTTP {(int)response.StatusCode}");
                }

                return new ServiceStatusResponse { Status = "HOAT_DONG" };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Dich vu AI khong kha dung: {ex.Message}", ex);
            }
        }

        private async Task<ServiceStatusResponse> CheckBlockchainAsync()
        {
            try
            {
                var web3 = new Web3(_blockchainSettings.RpcUrl);

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var blockNumberTask = web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                var completedTask = await Task.WhenAny(blockNumberTask, Task.Delay(Timeout.InfiniteTimeSpan, cts.Token));
                if (completedTask != blockNumberTask)
                {
                    throw new TimeoutException("Qua thoi gian cho RPC Blockchain.");
                }

                var blockNumber = await blockNumberTask;
                if (blockNumber is null)
                {
                    throw new InvalidOperationException("Kiem tra Blockchain that bai: khong lay duoc block number.");
                }

                return new ServiceStatusResponse { Status = "HOAT_DONG" };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Dich vu Blockchain khong kha dung: {ex.Message}", ex);
            }
        }
    }
}
