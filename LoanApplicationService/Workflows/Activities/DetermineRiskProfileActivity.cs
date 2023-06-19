namespace LoanApplicationService.Workflows.Activities;

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
        int riskClass = 0;
        var riskClauses = new List<RiskClause>();

        if (applicationInfo.LoanAmount <= 10000)
        {
            riskClass = 1;
        }
        else if (applicationInfo.LoanAmount > 10000 && applicationInfo.LoanAmount <= 50000)
        {
            riskClass = 2;
            riskClauses.Add(RiskClause.A);
        }
        else if (applicationInfo.LoanAmount > 50000 && applicationInfo.LoanAmount <= 100000)
        {
            riskClass = 3;
            riskClauses.Add(RiskClause.A);
        }
        else
        {
            riskClass = 4;
            riskClauses.Add(RiskClause.A);
        }
        
        if (applicationInfo.OutstandingAmount > 100000)
        {
            riskClauses.Add(RiskClause.B);
        }

        if (applicationInfo.HasDefaulted == true)
        {
            riskClass += 1;
            riskClauses.Add(RiskClause.C);
        }

        var riskProfile = new RiskProfile(riskClass, riskClauses.ToArray());

        _logger.LogInformation(
            $"[Workflow {context.InstanceId}] - Risk profile was determined: {riskProfile.Print()}.");

        return Task.FromResult(riskProfile);
    }
}