namespace LoanApplicationService.Workflows;

using System.Threading.Tasks;
using Dapr.Workflow;
using LoanApplicationService.Workflows.Activities;
using LoanApplicationService.Models;

public class LoanApplicationWorkflow : Workflow<LoanApplication, ApplicationResult>
{
    public override async Task<ApplicationResult> RunAsync(
        WorkflowContext context, LoanApplication application)
    {
        try
        {
            // Determine whether the applicant is an existing customer with us
            var customerInfo = await context.CallActivityAsync<CustomerInfo>(
                nameof(DetermineExistingCustomerActivity),
                application.ApplicantName);
            bool existingCustomer = customerInfo != null;

            // If the customer is new, register a new customer
            if (!existingCustomer)
            {
                customerInfo = await context.CallActivityAsync<CustomerInfo>(
                    nameof(RegisterCustomerActivity),
                    application.ApplicantName);
            }

            // Determine the Risk profile of the loan
            var applicationInfo = new ApplicationInfo(
                LoanAmount: application.LoanAmount,
                YearlyGrossSalary: application.YearlyGrossSalary,
                ExistingCustomer: existingCustomer,
                OutstandingAmount: customerInfo!.OutstandingAmount,
                HasDefaulted: customerInfo!.HasDefaulted);

            var riskProfile = await context.CallActivityAsync<RiskProfile>(
                nameof(DetermineRiskProfileActivity),
                applicationInfo);

            // Approve the loan
            bool loanApproved = false;
            if (riskProfile.RiskClass < 3)
            {
                loanApproved = true;
                await Log(context, $"Loan was aproved.");
            }
            else
            {
                // Assess the application
                await Log(context, $"Waiting for external event: LoanAssessmentCompleted ...");
                
                var loanAssessmentCompleted = await context.WaitForExternalEventAsync<LoanAssessmentCompleted>(
                    "LoanAssessmentCompleted");

                loanApproved = loanAssessmentCompleted.Approved;
                
                await Log(context, $"Loan was {(loanApproved ? "Approved" : "Rejected")}.");
            }

            // Collect loan info
            var loanInfo = new LoanInfo(
                ApplicationInfo: applicationInfo,
                RiskProfile: riskProfile
            );

            // Handle rejection
            if (!loanApproved)
            {
                // Send rejection letter
                await context.CallActivityAsync<RiskProfile>(
                    nameof(SendRejectionLetterActivity),
                    loanInfo);

                return new ApplicationResult(false);
            }

            // Send proposal
            await context.CallActivityAsync<object?>(
                nameof(SendProposalActivity),
                loanInfo);

            // Wait for the answer from the customer on the proposal
            bool customerDecisionReceivedWithinTimeout = true;
            bool proposalAccepted = false;
            try
            {
                await Log(context, $"Waiting for external event: CustomerProposalDecisionReceived ...");
                
                var customerProposalDecisionReceived =
                    await context.WaitForExternalEventAsync<CustomerProposalDecisionReceived>(
                        "CustomerProposalDecisionReceived", TimeSpan.FromDays(14));

                proposalAccepted = customerProposalDecisionReceived.Accepted;
                
                await Log(context, $"Proposal was {(proposalAccepted ? "Accepted" : "Rejected")}.");
            }
            catch (TaskCanceledException)
            {
                // Customer decision timeout expired
                customerDecisionReceivedWithinTimeout = false;
            }

            // Handle proposal decision timeout
            if (!customerDecisionReceivedWithinTimeout)
            {
                // Contact the customer
                await Log(context, $"Waiting for external event: CustomerContactedForProposal ...");
                
                var customerContactedForProposal =
                    await context.WaitForExternalEventAsync<CustomerContactedForProposal>(
                        "CustomerContactedForProposal");
                
                proposalAccepted = customerContactedForProposal.Accepted;
                
                await Log(context, $"Loan proposal was {(proposalAccepted ? "Accepted" : "Declined")}.");
            }

            // Handle proposal acceptance
            if (!proposalAccepted)
            {
                return new ApplicationResult(false);
            }

            // Register Contract
            await context.CallActivityAsync<object?>(
                nameof(RegisterContractActivity),
                loanInfo);

            // Send contract
            await context.CallActivityAsync<object?>(
                nameof(SendContractActivity),
                loanInfo);

            return new ApplicationResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }

    /// <summary>
    /// Log a message to the console.
    /// </summary>
    /// <param name="context">The workflow context.</param>
    /// <param name="message">The message to log.</param>
    private async Task<object?> Log(WorkflowContext context, string message)
    {
        return await context.CallActivityAsync<string>(nameof(LogActivity), message);
    }
}