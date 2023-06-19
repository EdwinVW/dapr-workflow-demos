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
        var logger = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        }).CreateLogger<LoanApplicationWorkflow>();

        try
        {
            // Determine whether the applicant is already an existing customer with us
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

            logger.LogInformation($"[Workflow {context.InstanceId}] Risk profile determined: {riskProfile.Print()}.");

            // Approve the loan
            bool loanApproved = false;
            if (riskProfile.RiskClass < 3)
            {
                loanApproved = true;
                logger.LogInformation($"[Workflow {context.InstanceId}] Loan aproved.");
            }
            else
            {
                // Assess the application
                logger.LogInformation($"[Workflow {context.InstanceId}] Waiting for external event: LoanAssessmentCompleted ...");
                var loanAssessmentCompleted = await context.WaitForExternalEventAsync<LoanAssessmentCompleted>(
                    "LoanAssessmentCompleted");

                logger.LogInformation($"[Workflow {context.InstanceId}] Received external event: LoanAssessmentCompleted.");
                loanApproved = loanAssessmentCompleted.Approved;
                logger.LogInformation($"[Workflow {context.InstanceId}] Loan was {(loanApproved ? "Approved" : "Rejected")}.");

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
                logger.LogInformation($"[Workflow {context.InstanceId}] Waiting for external event: CustomerProposalDecisionReceived ...");
                var customerProposalDecisionReceived = 
                    await context.WaitForExternalEventAsync<CustomerProposalDecisionReceived>(
                        "CustomerProposalDecisionReceived", TimeSpan.FromDays(14));

                logger.LogInformation($"[Workflow {context.InstanceId}] Received external event: CustomerProposalDecisionReceived.");
                proposalAccepted = customerProposalDecisionReceived.Accepted;
                logger.LogInformation($"[Workflow {context.InstanceId}] Proposal was {(proposalAccepted ? "Accepted" : "Rejected")}.");
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
                logger.LogInformation($"[Workflow {context.InstanceId}] Waiting for external event: CustomerContactedForProposal ...");
                var customerContactedForProposal = 
                    await context.WaitForExternalEventAsync<CustomerContactedForProposal>(
                        "CustomerContactedForProposal");

                // Customer was contacted
                logger.LogInformation($"[Workflow {context.InstanceId}] Received external event: CustomerContactedForProposal.");
                proposalAccepted = customerContactedForProposal.Accepted;
                logger.LogInformation($"[Workflow {context.InstanceId}] Proposal for loan application was {(proposalAccepted ? "Accepted" : "Declined")}.");
            }

            // Handle proposal acceptance
            if (!proposalAccepted)
            {
                return new ApplicationResult(false);
            }

            // Register Contract
            //TODO

            // Send contract
            // TODO

            return new ApplicationResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }

    private async Task<CustomerInfo?> DetermineExistingCustomer(WorkflowContext context, string applicantName)
    {
        var customerInfo = await context.CallActivityAsync<CustomerInfo>(
            nameof(DetermineExistingCustomerActivity),
            applicantName);
        return customerInfo;
    }
}