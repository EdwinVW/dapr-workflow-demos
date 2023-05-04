namespace LoanApplicationService.Workflows;

using System.Threading.Tasks;
using Dapr.Workflow;
using LoanApplicationService.Activities;
using LoanApplicationService.Models;

public class LoanApplicationWorkflow : Workflow<LoanApplication, ApplicationResult>
{
    public override async Task<ApplicationResult> RunAsync(WorkflowContext context, LoanApplication input)
    {
        await context.CallActivityAsync(
                nameof(DetermineExistingCustomerActivity),
                new ProspectInfo(input.ApplicantName));

        return new ApplicationResult(true);
    }
}