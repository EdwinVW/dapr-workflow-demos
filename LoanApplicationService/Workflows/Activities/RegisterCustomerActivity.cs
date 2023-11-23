namespace LoanApplicationService.Workflows.Activities;

using Dapr.Workflow;
using LoanApplicationService.Models;
using Microsoft.Extensions.Logging;

public class RegisterCustomerActivity : WorkflowActivity<CustomerInfo, object?>
{
    private readonly ILogger _logger;

    public RegisterCustomerActivity(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<RegisterCustomerActivity>();
    }

    public override Task<object?> RunAsync(WorkflowActivityContext context, CustomerInfo customerInfo)
    {
        // Store customer
        // TODO: Store customer in database

        _logger.LogInformation($"[Workflow {context.InstanceId}] - New customer was registered.");

        return Task.FromResult<object?>(null);
    }
}