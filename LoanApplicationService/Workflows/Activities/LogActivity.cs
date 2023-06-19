namespace LoanApplicationService.Workflows.Activities;

using Dapr.Workflow;
using Microsoft.Extensions.Logging;

public class LogActivity : WorkflowActivity<string, object?>
{
    private readonly ILogger _logger;

    public LogActivity(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<LogActivity>();
    }

    public override Task<object?> RunAsync(WorkflowActivityContext context, string message)
    {
        _logger.LogInformation($"[Workflow {context.InstanceId}] - {message}");
        return Task.FromResult<object?>(null);
    }
}