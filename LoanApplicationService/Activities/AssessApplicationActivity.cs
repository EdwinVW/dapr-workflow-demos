namespace LoanApplicationService.Activities;

using Dapr.Workflow;
using LoanApplicationService.Models;
using Microsoft.Extensions.Logging;

public class AssessApplicationActivity : WorkflowActivity<ApplicationInfo, bool>
{
    private readonly ILogger _logger;

    public AssessApplicationActivity(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<AssessApplicationActivity>();
    }

    public override Task<bool> RunAsync(WorkflowActivityContext context, ApplicationInfo applicationInfo)
    {
        _logger.LogInformation($"[Workflow {context.InstanceId}] Assess Application.");
        Task.Delay(TimeSpan.FromSeconds(25));
        return Task.FromResult(true);
    }
}