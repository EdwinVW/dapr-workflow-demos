namespace LoanApplicationService.Workflows.Activities;

using Dapr.Workflow;
using LoanApplicationService.Models;
using Microsoft.Extensions.Logging;

public class SendProposalActivity : WorkflowActivity<LoanInfo, object?>
{
    private readonly ILogger _logger;

    public SendProposalActivity(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<RegisterCustomerActivity>();
    }

    public override Task<object?> RunAsync(WorkflowActivityContext context, LoanInfo loanInfo)
    {
        // Send proposal
        // TODO

        _logger.LogInformation($"[Workflow {context.InstanceId}] - Loan proposal was sent.");
        
        return Task.FromResult<object?>(null);
    }
}