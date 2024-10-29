using CombiningData.Constants;
using CombiningData.Models;
using Newtonsoft.Json;
using ShopfloorService.Sdk.Models;
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
                var machinesData = JsonConvert.DeserializeObject<List<MachineResponseModel>>(machines)!;
                var shifts = await GetShifts();
                List<ShopfloorDenormalizedModel> shopfloorDenormalizedModels = [];
                foreach (var shift in shifts)
                {
                    foreach (var machine in machinesData)
                    {
                        var machineName = machine.Name;
                        var machineTimeLineBlocks = await GetTimelineBlocks(machineName);

                        foreach (var machineTimeLine in machineTimeLineBlocks.Where(_ => _.ShiftName == shift.ShiftName))
                        {
                            var timeLineBlocks = machineTimeLine.TimeLineBlocks;
                            foreach (var timeLineBlock in timeLineBlocks)
                            {
                                var oeeData = await GetOeeStatistics(machineName,
                                                            timeLineBlock.StartTime.Date,
                                                            timeLineBlock.EndTime.Date);
                                var subWorkOrderBurnDownSummaries = await GetSubWorkOrderBurnDownRecords(
                                                timeLineBlock.StartTime.Date, timeLineBlock.EndTime.Date,
                                                machineName, timeLineBlock.ShiftName);

                                var shopfloorDenormalized = new ShopfloorDenormalizedModel();
                                shopfloorDenormalized.TimeLineBlock = timeLineBlock;
                                shopfloorDenormalized.MachineResponse = machine;
                                shopfloorDenormalized.DateSpanOeeReportModel = oeeData;
                                shopfloorDenormalized.SubWorkOrderBurnDownSummaries = subWorkOrderBurnDownSummaries;

                                shopfloorDenormalizedModels.Add(shopfloorDenormalized);
                            }
                        }
                    }
                }
                foreach (var shopfloorData in shopfloorDenormalizedModels)
                {
                    Console.WriteLine(shopfloorData.ToString());
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<DateSpanOeeReportModel> GetOeeStatistics(string machineName,
            DateTime fromDate, DateTime toDate)
        {
            var oeeReports = await _httpClient.GetStringAsync(
                $"http://localhost/api/shopfloor/api/shop-floor-management/v1/production-statistics/" +
                $"{machineName}/oee-statistics?" +
                $"fromDate={DateOnly.FromDateTime(fromDate)}&toDate={DateOnly.FromDateTime(toDate)}");
            var oeeReportsData = JsonConvert.DeserializeObject<DateSpanOeeReportModel>(oeeReports)!;
            return oeeReportsData;
        }

        public async Task<List<ShiftModel>> GetShifts()
        {
            var shifts = await _httpClient.GetStringAsync("http://localhost/api/shopfloor/api/shop-floor-management/v1/shifts");
            var shiftsData = JsonConvert.DeserializeObject<List<ShiftModel>>(shifts)!;
            return shiftsData;
        }
        public async Task<List<MachineTimeLine>> GetTimelineBlocks(string machineName)
        {
            var timeline = await _httpClient.GetStringAsync($"http://localhost/api/shopfloor/api/shop-floor-management/v1/machines/{machineName}/time-line-data");
            var timelineData = JsonConvert.DeserializeObject<List<MachineTimeLine>>(timeline)!;

            return timelineData;
        }
        public async Task<List<ToolBurnDownSummary>> GetToolBurnDown(string machineName, string partNumber, string operationNumber)
        {

            var toolBurnDown = await _httpClient.GetStringAsync(
                $"http://localhost/api/shopfloor/api/shop-floor-management/v1/production-statistics/" +
                $"{machineName}/tool-usage-statistics?" +
                $"partNumber={partNumber}" +
                $"&operationNumber={operationNumber}");
            var toolBurnDownData = JsonConvert.DeserializeObject<List<ToolBurnDownSummary>>(toolBurnDown)!;
            return toolBurnDownData;
        }

        public async Task<List<SubWorkOrderBurnDownSummary>> GetSubWorkOrderBurnDownRecords(
            DateTime fromDate, DateTime toDate, string machineName, string shiftName)
        {
            var subWorkOrderBurnDownRecords = await _httpClient.GetStringAsync(
                "http://localhost/api/shopfloor/api/shop-floor-management/v1/production-statistics/sub-work-order-burndown-records?" +
                $"fromDate={DateOnly.FromDateTime(fromDate)}&toDate={DateOnly.FromDateTime(toDate)}" +
                $"&shifts={shiftName}&machines={machineName}");
            var subWorkOrderBurnDownRecordsData = JsonConvert.DeserializeObject<List<SubWorkOrderBurnDownSummary>>(subWorkOrderBurnDownRecords)!;
            return subWorkOrderBurnDownRecordsData;
        }
    }
}
