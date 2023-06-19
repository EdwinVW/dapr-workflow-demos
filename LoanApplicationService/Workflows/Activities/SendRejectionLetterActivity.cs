namespace LoanApplicationService.Workflows.Activities;

using Dapr.Workflow;
using LoanApplicationService.Models;
using Microsoft.Extensions.Logging;

public class SendRejectionLetterActivity : WorkflowActivity<LoanInfo, object?>
{
    private readonly ILogger _logger;

    public SendRejectionLetterActivity(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<RegisterCustomerActivity>();
    }

    public override Task<object?> RunAsync(WorkflowActivityContext context, LoanInfo loanInfo)
    {
        _logger.LogInformation($"[Workflow {context.InstanceId}] Send rejection letter.");

        // Send letter
        // TODO

        return Task.FromResult<object?>(null);
    }
}