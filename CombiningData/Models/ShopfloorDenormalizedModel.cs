using ShopfloorService.Sdk.Models;
using ShopfloorService.Sdk.Models.Response;

namespace CombiningData.Models
{
    public class ShopfloorDenormalizedModel
    {
        public TimeLineBlockModel TimeLineBlock { get; set; }
        public MachineResponseModel MachineResponse { get; set; }
        public DateSpanOeeReportModel DateSpanOeeReportModel { get; set; }
        public ToolBurnDownModel ToolBurnDownModel { get; set; }
        public List<SubWorkOrderBurnDownSummary> SubWorkOrderBurnDownSummaries { get; set; }

        public override string ToString()
        {
            var subWorkOrderBurnDownData = "\nSub Work Order Burn Down :\n";
            foreach (var subWorkOrderBurnDown in SubWorkOrderBurnDownSummaries)
            {
                subWorkOrderBurnDownData += $"Part Number: {subWorkOrderBurnDown.PartNumber} \n" +
                    $"Operation Number: {subWorkOrderBurnDown.OperationNumber} \n" +
                    $"Sub wOrk Order Id: {subWorkOrderBurnDown.SubWorkOrderId} \n" +
                    $"Part Produced Count:{subWorkOrderBurnDown.PartProducedCount} \n" +
                    $"Part Rejection Count: {subWorkOrderBurnDown.PartRejectedCount}\n";
            }

            var oeeData = string.Empty;
            if (DateSpanOeeReportModel != null)
            {
                oeeData = $"\nDate Span Oee Report: \n" +
                $"Running Time: {DateSpanOeeReportModel.RunningTime} " +
                $"Performance: {DateSpanOeeReportModel.OeeStatistics.Performance} " +
                $"Quality: {DateSpanOeeReportModel.OeeStatistics.Quality}";
            }

            return $"TimeLineBlock:\n" +
                $"Start Time: {TimeLineBlock.StartTime}\n" +
                $"End Time: {TimeLineBlock.EndTime}\n" +
                $"Reason: {TimeLineBlock.Reason} \n" +
                $"Shift Name: {TimeLineBlock.ShiftName}\n" +
                $"Machine: \n" +
                $"Name : {MachineResponse.Name}\n" +
                $"Machine Name : {MachineResponse.DisplayName}" +
                $"{oeeData}" +
                $"{subWorkOrderBurnDownData}";
        }
    }
}
