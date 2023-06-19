namespace LoanApplicationService.Workflows.Activities;

using System.Text.Json;
using Dapr.Client;
using Dapr.Workflow;
using LoanApplicationService.Models;
using Microsoft.Extensions.Logging;

public class DetermineExistingCustomerActivity : WorkflowActivity<string, CustomerInfo?>
{
    private readonly ILogger _logger;

    public DetermineExistingCustomerActivity(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<DetermineExistingCustomerActivity>();
    }

    public override async Task<CustomerInfo?> RunAsync(WorkflowActivityContext context, string applicantName)
    {
        using var client = new DaprClientBuilder().Build();
        var request = client.CreateInvokeMethodRequest(
            HttpMethod.Get, 
            "CustomerService", 
            $"customer/{applicantName}");
        var response = await client.InvokeMethodWithResponseAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogInformation($"[Workflow {context.InstanceId}] Called CustomerService. Customer NOT FOUND.");
            return null;
        }

        var customerInfo = await JsonSerializer.DeserializeAsync<CustomerInfo>(await response.Content.ReadAsStreamAsync());
        _logger.LogInformation($"Called CustomerService. Customer FOUND.");
        return customerInfo;
    }
}