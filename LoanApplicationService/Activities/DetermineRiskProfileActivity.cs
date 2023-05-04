// namespace LoanApplicationService.Activities;

// using Dapr.Client;
// using Dapr.Workflow;
// using LoanApplicationService.Models;
// using Microsoft.Extensions.Logging;

// public class DetermineRiskProfileActivity : WorkflowActivity<string, ProspectInfo>
// {
//     private readonly ILogger _logger;

//     public DetermineRiskProfileActivity(ILoggerFactory loggerFactory)
//     {
//         _logger = loggerFactory.CreateLogger<DetermineRiskProfileActivity>();
//     }

//     public override Task<ProspectInfo> RunAsync(WorkflowActivityContext context, string name)
//     {
//         string prospectId = Guid.NewGuid().ToString("D");
//         return Task.FromResult(new ProspectInfo(prospectId, name));
//     }
// }