namespace LoanApplicationService.Activities;

using Dapr.Workflow;
using LoanApplicationService.Models;
using Microsoft.Extensions.Logging;

public class DetermineRiskProfileActivity : WorkflowActivity<ApplicationInfo, RiskProfile>
{
    private readonly ILogger _logger;

    public DetermineRiskProfileActivity(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<DetermineRiskProfileActivity>();
    }

    public override Task<RiskProfile> RunAsync(WorkflowActivityContext context, ApplicationInfo applicationInfo)
    {
        _logger.LogInformation($"[Workflow {context.InstanceId}] Determine Risk Profile (2 + A,C).");
        var riskProfile = new RiskProfile(2, new string[] { "A", "C" });
        return Task.FromResult(riskProfile);
    }
}