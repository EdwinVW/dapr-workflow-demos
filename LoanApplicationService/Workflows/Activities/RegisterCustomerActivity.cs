namespace LoanApplicationService.Workflows.Activities;

using Dapr.Workflow;
using LoanApplicationService.Models;
using Microsoft.Extensions.Logging;

public class RegisterCustomerActivity : WorkflowActivity<string, CustomerInfo>
{
    private readonly ILogger _logger;

    public RegisterCustomerActivity(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<RegisterCustomerActivity>();
    }

    public override Task<CustomerInfo> RunAsync(WorkflowActivityContext context, string applicantName)
    {
        _logger.LogInformation($"[Workflow {context.InstanceId}] Register new customer.");
        string prospectId = Guid.NewGuid().ToString("D");
        var customerInfo = new CustomerInfo(
            Id: Guid.NewGuid().ToString("D"),
            Name: applicantName,
            OutstandingAmount: 0,
            HasDefaulted: false);
        return Task.FromResult(customerInfo);
    }
}