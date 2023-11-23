namespace LoanApplicationService.Workflows.Activities;

using System.Text.Json;
using Dapr.Client;
using Dapr.Workflow;
using LoanApplicationService.Models;
using Microsoft.Extensions.Logging;

public class DetermineExistingCustomerActivity : WorkflowActivity<string, CustomerInfo?>
{
    private readonly ILogger _logger;
    private readonly DaprClient _daprClient;

    public DetermineExistingCustomerActivity(ILoggerFactory loggerFactory, DaprClient daprClient)
    {
        _logger = loggerFactory.CreateLogger<DetermineExistingCustomerActivity>();
        this._daprClient = daprClient;
    }

    public override async Task<CustomerInfo?> RunAsync(WorkflowActivityContext context, string applicantName)
    {
        _logger.LogInformation(
                $"[Workflow {context.InstanceId}] - Determine existing client using the CustomerService.");

        var request = _daprClient.CreateInvokeMethodRequest(
            HttpMethod.Get, 
            "CustomerService", 
            $"customer/{applicantName}");
            
        var response = await _daprClient.InvokeMethodWithResponseAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogInformation(
                $"[Workflow {context.InstanceId}] - Called CustomerService. Customer was NOT FOUND.");
            return null;
        }

        var customerInfo = await JsonSerializer.DeserializeAsync<CustomerInfo>(
            await response.Content.ReadAsStreamAsync());
        
        _logger.LogInformation($"[Workflow {context.InstanceId}] - Called CustomerService. Customer was FOUND.");
        
        return customerInfo;
    }
}