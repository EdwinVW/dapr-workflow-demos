namespace LoanApplicationService.Workflows;

using System.Threading.Tasks;
using Dapr.Workflow;
using LoanApplicationService.Activities;
using LoanApplicationService.Models;
using Microsoft.DurableTask;

public class LoanApplicationWorkflow : Workflow<LoanApplication, ApplicationResult>
{
    public override async Task<ApplicationResult> RunAsync(WorkflowContext context, LoanApplication application)
    {
        try
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
            var applicationInfo = new ApplicationInfo(
                LoanAmount: application.LoanAmount,
                YearlyGrossSalary: application.YearlyGrossSalary,
                ExistingCustomer: existingCustomer,
                OutstandingAmount: existingCustomer ? customerInfo!.OutstandingAmount : null,
                HasDefaulted: existingCustomer ? customerInfo!.HasDefaulted : null
            );
            var riskProfile = await context.CallActivityAsync<ProspectInfo>(
                nameof(DetermineRiskProfileActivity),
                applicationInfo);

            // Contact the customer
            var customerContacted = await context.WaitForExternalEventAsync<CustomerContacted>("CustomerContacted", TimeSpan.FromSeconds(120));

            return new ApplicationResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
}