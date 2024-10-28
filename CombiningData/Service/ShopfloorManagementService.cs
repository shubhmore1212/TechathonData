using CombiningData.Constants;
using Newtonsoft.Json;
using ShopfloorService.Sdk.Models.Response;
using System.Text.Json.Serialization;

namespace CombiningData.Service
{
    public class ShopfloorManagementService
    {
        private readonly HttpClient _httpClient;

        public ShopfloorManagementService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient(ShopfloorConstants.ShopfloorManagementService);
        }

        public async Task GetMachines()
        {
            try
            {
                var machines = await _httpClient.GetStringAsync("http://localhost/api/shopfloor/api/shop-floor-management/v1/machines");
                var machineData = JsonConvert.DeserializeObject<List<MachineResponseModel>>(machines);

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task GetOeeStatistics(string machineName)
        {
            try
            {
                var machines = await _httpClient.GetStringAsync($"http://localhost/api/shopfloor/api/shop-floor-management/v1/production-statistics/{machineName}/oee-statistics");
                var machineData = JsonConvert.DeserializeObject<List<MachineResponseModel>>(machines);

            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task GetShifts()
        {
            try
            {
                var machines = await _httpClient.GetStringAsync("http://localhost/api/shopfloor/api/shop-floor-management/v1/shifts");
                var machineData = JsonConvert.DeserializeObject<List<MachineResponseModel>>(machines);

            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task GetTimelineBlocks(string machineName)
        {
            try
            {
                var machines = await _httpClient.GetStringAsync($"http://localhost/api/shopfloor/api/shop-floor-management/v1/machines/{machineName}/time-line-data");
                var machineData = JsonConvert.DeserializeObject<List<MachineResponseModel>>(machines);

            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task GetToolBurnDown(string machineName, string partNumber, string operationNumber)
        {
            try
            {
                var machines = await _httpClient.GetStringAsync($"http://localhost/api/shopfloor/api/shop-floor-management/v1/production-statistics/{machineName}/tool-usage-statistics?partNumber={partNumber}&operationNumber={operationNumber}");
                var machineData = JsonConvert.DeserializeObject<List<MachineResponseModel>>(machines);
            }
            catch(Exception)
            {
                throw;
            }
        }
    }
}
