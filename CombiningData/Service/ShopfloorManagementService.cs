using ClosedXML.Excel;
using CombiningData.Constants;
using CombiningData.Models;
using DocumentFormat.OpenXml.Office2013.Word;
using DocumentFormat.OpenXml.Spreadsheet;
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
                var machines = await _httpClient.GetStringAsync("api/shop-floor-management/v1/machines");
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
                using (var workBook = new XLWorkbook())
                {
                    var worksheet = workBook.Worksheets.Add("Shopfloor");

                    for (int i = 0; i < shopfloorDenormalizedModels.Count; i++)
                    {
                        #region TimeLineBlock
                        worksheet.Cell(i + 2, 1).Value = shopfloorDenormalizedModels[i].TimeLineBlock.StartTime.ToString();
                        worksheet.Cell(i + 2, 2).Value = shopfloorDenormalizedModels[i].TimeLineBlock.EndTime.ToString();
                        worksheet.Cell(i + 2, 3).Value = shopfloorDenormalizedModels[i].TimeLineBlock.Reason.ToString();
                        worksheet.Cell(i + 2, 4).Value = shopfloorDenormalizedModels[i].TimeLineBlock.Reason.ToString();
                        worksheet.Cell(i + 2, 5).Value = shopfloorDenormalizedModels[i].TimeLineBlock.ShiftName.ToString();
                        #endregion

                        #region Machine
                        worksheet.Cell(i + 2, 6).Value = shopfloorDenormalizedModels[i].MachineResponse.Name.ToString();
                        worksheet.Cell(i + 2, 7).Value = shopfloorDenormalizedModels[i].MachineResponse.DisplayName.ToString();
                        worksheet.Cell(i + 2, 8).Value = shopfloorDenormalizedModels[i].MachineResponse.LoadUnLoadTimePerPart.ToString();
                        worksheet.Cell(i + 2, 9).Value = shopfloorDenormalizedModels[i].MachineResponse.ToolChangeTime.ToString();
                        #endregion

                        #region Oee Report
                        if (shopfloorDenormalizedModels[i].DateSpanOeeReportModel != null)
                        {
                            worksheet.Cell(i + 2, 11).Value = shopfloorDenormalizedModels[i].DateSpanOeeReportModel.RunningTime.ToString();
                            worksheet.Cell(i + 2, 12).Value = shopfloorDenormalizedModels[i].DateSpanOeeReportModel.OeeStatistics.Performance.ToString();
                            worksheet.Cell(i + 2, 13).Value = shopfloorDenormalizedModels[i].DateSpanOeeReportModel.OeeStatistics.Quality.ToString();
                        }
                        else
                        {
                            worksheet.Cell(i + 2, 11).Value = "-";
                            worksheet.Cell(i + 2, 12).Value = "-";
                            worksheet.Cell(i + 2, 13).Value = "-";
                        }
                        #endregion

                        foreach (var subWorkOrderBurnDown in shopfloorDenormalizedModels[i].SubWorkOrderBurnDownSummaries)
                        {
                            worksheet.Cell(i + 2, 14).Value = subWorkOrderBurnDown.PartNumber;
                            worksheet.Cell(i + 2, 15).Value = subWorkOrderBurnDown.OperationNumber;
                            worksheet.Cell(i + 2, 16).Value = subWorkOrderBurnDown.SubWorkOrderId;
                            worksheet.Cell(i + 2, 17).Value = subWorkOrderBurnDown.PartProducedCount;
                            worksheet.Cell(i + 2, 18).Value = subWorkOrderBurnDown.PartRejectedCount;
                        }
                    }

                    workBook.SaveAs("Shopfloor.xlsx");
                }

                await Console.Out.WriteLineAsync("Done Bro!");

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
                "api/shopfloor/api/shop-floor-management/v1/production-statistics/" +
                $"{machineName}/oee-statistics?" +
                $"fromDate={DateOnly.FromDateTime(fromDate)}&toDate={DateOnly.FromDateTime(toDate)}");
            var oeeReportsData = JsonConvert.DeserializeObject<DateSpanOeeReportModel>(oeeReports)!;
            return oeeReportsData;
        }

        public async Task<List<ShiftModel>> GetShifts()
        {
            var shifts = await _httpClient.GetStringAsync("api/shopfloor/api/shop-floor-management/v1/shifts");
            var shiftsData = JsonConvert.DeserializeObject<List<ShiftModel>>(shifts)!;
            return shiftsData;
        }
        public async Task<List<MachineTimeLine>> GetTimelineBlocks(string machineName)
        {
            var timeline = await _httpClient.GetStringAsync($"api/shopfloor/api/shop-floor-management/v1/machines/{machineName}/time-line-data");
            var timelineData = JsonConvert.DeserializeObject<List<MachineTimeLine>>(timeline)!;

            return timelineData;
        }
        public async Task<List<ToolBurnDownSummary>> GetToolBurnDown(string machineName, string partNumber, string operationNumber)
        {

            var toolBurnDown = await _httpClient.GetStringAsync(
                $"api/shopfloor/api/shop-floor-management/v1/production-statistics/" +
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
                "api/shopfloor/api/shop-floor-management/v1/production-statistics/sub-work-order-burndown-records?" +
                $"fromDate={DateOnly.FromDateTime(fromDate)}&toDate={DateOnly.FromDateTime(toDate)}" +
                $"&shifts={shiftName}&machines={machineName}");
            var subWorkOrderBurnDownRecordsData = JsonConvert.DeserializeObject<List<SubWorkOrderBurnDownSummary>>(subWorkOrderBurnDownRecords)!;
            return subWorkOrderBurnDownRecordsData;
        }
    }
}
