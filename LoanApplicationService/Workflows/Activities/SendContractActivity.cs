namespace LoanApplicationService.Workflows.Activities;

using Dapr.Workflow;
using LoanApplicationService.Models;
using Microsoft.Extensions.Logging;

public class SendContractActivity : WorkflowActivity<LoanInfo, object?>
{
    private readonly ILogger _logger;

    public SendContractActivity(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<RegisterContractActivity>();
    }

    public override Task<object?> RunAsync(WorkflowActivityContext context, LoanInfo loanInfo)
    {
        // Register Contract
        // TODO

        _logger.LogInformation($"[Workflow {context.InstanceId}] - Contract was sent.");

        return Task.FromResult<object?>(null);
    }
}