namespace LoanApplicationService.Workflows.Activities;

using Dapr.Workflow;
using LoanApplicationService.Models;
using Microsoft.Extensions.Logging;

public class RegisterContractActivity : WorkflowActivity<LoanInfo, object?>
{
    private readonly ILogger _logger;

    public RegisterContractActivity(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<RegisterContractActivity>();
    }

    public override Task<object?> RunAsync(WorkflowActivityContext context, LoanInfo loanInfo)
    {
        // Register Contract
        // TODO

        _logger.LogInformation($"[Workflow {context.InstanceId}] - Contract was registered.");

        return Task.FromResult<object?>(null);
    }
}