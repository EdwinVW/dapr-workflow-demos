namespace LoanApplicationService.Activities;

using Dapr.Workflow;
using LoanApplicationService.Models;
using Microsoft.Extensions.Logging;

public class RegisterProspectActivity : WorkflowActivity<string, ProspectInfo>
{
    private readonly ILogger _logger;

    public RegisterProspectActivity(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<RegisterProspectActivity>();
    }

    public override Task<ProspectInfo> RunAsync(WorkflowActivityContext context, string applicantName)
    {
        _logger.LogInformation($"[Workflow {context.InstanceId}] Register new prospect.");
        string prospectId = Guid.NewGuid().ToString("D");
        return Task.FromResult(new ProspectInfo(prospectId, applicantName));
    }
}