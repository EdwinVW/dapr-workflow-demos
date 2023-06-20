namespace LoanApplicationService.Workflows.Activities;

using Dapr.Workflow;
using LoanApplicationService.Models;
using Microsoft.Extensions.Logging;

public class SendRejectionLetterActivity : WorkflowActivity<LoanInfo, object?>
{
    private readonly ILogger _logger;

    public SendRejectionLetterActivity(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<SendRejectionLetterActivity>();
    }

    public override Task<object?> RunAsync(WorkflowActivityContext context, LoanInfo loanInfo)
    {
        // Send letter
        // TODO

        _logger.LogInformation($"[Workflow {context.InstanceId}] - Rejection letter was sent.");

        return Task.FromResult<object?>(null);
    }
}