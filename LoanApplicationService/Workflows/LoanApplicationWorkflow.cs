namespace LoanApplicationService.Workflows;

using System.Threading.Tasks;
using Dapr.Workflow;
using LoanApplicationService.Activities;
using LoanApplicationService.Models;
using Microsoft.Extensions.Logging;

public class LoanApplicationWorkflow : Workflow<LoanApplication, ApplicationResult>
{
     public override async Task<ApplicationResult> RunAsync(WorkflowContext context, LoanApplication application)
    {
        // Determine whether the applicant is already an existing customer with us
        var customerInfo = await context.CallActivityAsync<CustomerInfo>(
                nameof(DetermineExistingCustomerActivity),
                application.ApplicantName);
        bool existingCustomer = (customerInfo != null);

        // If the customer is new, register a new prospect
        ProspectInfo prospectInfo;
        if (!existingCustomer)
        {
            prospectInfo = await context.CallActivityAsync<ProspectInfo>(
                nameof(RegisterProspectActivity),
                application.ApplicantName);
        }

        // Determine the Risk profile of the loan
        // TODO

        // Assess the application


        return new ApplicationResult(true);
    }
}