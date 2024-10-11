using CombiningData.Constants;

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
                var machines=await _httpClient.GetStringAsync("http://localhost/api/shopfloor/api/shop-floor-management/v1/machines");
                
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
