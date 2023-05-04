namespace LoanApplicationService.Activities;

using Dapr.Client;
using Dapr.Workflow;
using LoanApplicationService.Models;
using Microsoft.Extensions.Logging;

public class DetermineExistingCustomerActivity : WorkflowActivity<ProspectInfo, bool>
{
    private readonly ILogger _logger;

    public DetermineExistingCustomerActivity(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<DetermineExistingCustomerActivity>();
    }

    public override async Task<bool> RunAsync(WorkflowActivityContext context, ProspectInfo prospectInfo)
    {
        using var client = new DaprClientBuilder().Build();
        var request = client.CreateInvokeMethodRequest(HttpMethod.Get, "CustomerService", $"customer/{prospectInfo.Name}");
        var response = await client.InvokeMethodWithResponseAsync(request);

        _logger.LogInformation($"Called CustomerService. Result statuscode: {response.StatusCode}.");

        return response.IsSuccessStatusCode;
    }
}