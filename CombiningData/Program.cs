using CombiningData.Constants;
using CombiningData.Models;

var builder = WebApplication.CreateBuilder(args);
var serviceUrls=builder.Configuration.GetSection("ServiceUrls").Get<ServiceUrls>();

if (serviceUrls!=null)
{
    builder.Services.AddSingleton(_=>serviceUrls);
    builder.Services.AddHttpClient(ShopfloorConstants.ShopfloorManagementService, (service, client) =>
    {
        if (!string.IsNullOrEmpty(serviceUrls.ShopfloorServiceUrl))
        {
            client.BaseAddress = new Uri(serviceUrls.ShopfloorServiceUrl);
        }
    });
}

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
