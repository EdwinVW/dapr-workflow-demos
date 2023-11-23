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
            var customerInfo = await DetermineExistingCustomer(context, application);
            bool applicantIsExistingCustomer = customerInfo != null;

            // If the customer is new, register a new customer
            if (!applicantIsExistingCustomer)
            {
                customerInfo = new CustomerInfo(
                    Id: Guid.NewGuid().ToString("D"),
                    Name: application.ApplicantName,
                    OutstandingAmount: 0,
                    HasDefaulted: false);
                    
                await RegisterCustomer(context, customerInfo);
            }

            // Determine the Risk profile of the loan
            var applicationInfo = new ApplicationInfo(
                LoanAmount: application.LoanAmount,
                YearlyGrossSalary: application.YearlyGrossSalary,
                ExistingCustomer: applicantIsExistingCustomer,
                OutstandingAmount: customerInfo!.OutstandingAmount,
                HasDefaulted: customerInfo!.HasDefaulted);

            RiskProfile riskProfile = await DetermineRiskProfile(context, applicationInfo);

            // Approve the loan
            bool loanWasApproved = false;
            if (riskProfile.RiskClass < 3)
            {
                loanWasApproved = true;
                await Log(context, $"Loan was aproved.");
            }
            else
            {
                // Assess the application
                loanWasApproved = await AssessLoanApplication(context);
            }

            // Collect loan info
            var loanInfo = new LoanInfo(
                ApplicationInfo: applicationInfo,
                RiskProfile: riskProfile
            );

            // Handle rejection
            if (!loanWasApproved)
            {
                // Send rejection letter
                await SendRejectionLetterToCustomer(context, loanInfo);
                return new ApplicationResult(false);
            }

            // Send proposal
            await SendLoanProposalToCustomer(context, loanInfo);

            // Wait for the answer from the customer on the proposal
            CustomerDecisionResult customerDecisionResult =
                await WaitForCustomerDecision(context);

            // Handle customer decision
            if (!customerDecisionResult.CustomerDecisionWasReceivedWithinTimeout)
            {
                // Contact the customer
                bool proposalWasAccepted = await ContactCustomerForDecision(context);

                if (!proposalWasAccepted)
                {
                    return new ApplicationResult(false);
                }
            }
            else if (!customerDecisionResult.ProposalWasAccepted)
            {
                return new ApplicationResult(false);
            }

            // At this point, the proposal was accepted by the customer

            // Register Contract
            await RegisterContract(context, loanInfo);

            // Send contract to customer
            await SendContract(context, loanInfo);

            return new ApplicationResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }

    /// <summary>
    /// Determine whether the customer is an existing customer.
    /// </summary>
    private async Task<CustomerInfo?> DetermineExistingCustomer(
        WorkflowContext context,
        LoanApplication application)
    {
        return await context.CallActivityAsync<CustomerInfo?>(
            nameof(DetermineExistingCustomerActivity),
            application.ApplicantName);
    }

    /// <summary>
    /// Register a new customer.
    /// </summary>
    private static async Task RegisterCustomer(
        WorkflowContext context,
        CustomerInfo customerInfo)
    {
        await context.CallActivityAsync<CustomerInfo>(
            nameof(RegisterCustomerActivity),
            customerInfo);
    }

    /// <summary>
    /// Determine the rsk profile for the loan.
    /// </summary>
    private async Task<RiskProfile> DetermineRiskProfile(
        WorkflowContext context,
        ApplicationInfo applicationInfo)
    {
        return await context.CallActivityAsync<RiskProfile>(
            nameof(DetermineRiskProfileActivity),
            applicationInfo);
    }

    /// <summary>
    /// Assess the LoanApplication.
    /// </summary>
    private async Task<bool> AssessLoanApplication(WorkflowContext context)
    {
        await Log(context, $"Waiting for external event: LoanAssessmentCompleted ...");

        var loanAssessmentCompleted =
            await context.WaitForExternalEventAsync<LoanAssessmentCompleted>(
                "LoanAssessmentCompleted");

        bool loanWasApproved = loanAssessmentCompleted.Approved;

        await Log(context, $"Loan was {(loanWasApproved ? "Approved" : "Rejected")}.");
        
        return loanWasApproved;
    }

    /// <summary>
    /// Send a rejection letter to the customer.
    /// </summary>
    private async Task SendRejectionLetterToCustomer(WorkflowContext context, LoanInfo loanInfo)
    {
        await context.CallActivityAsync<RiskProfile>(
            nameof(SendRejectionLetterActivity),
            loanInfo);
    }

    /// <summary>
    /// Send loan proposal to the customer.
    /// </summary>
    private async Task SendLoanProposalToCustomer(WorkflowContext context, LoanInfo loanInfo)
    {
        await context.CallActivityAsync<object?>(
            nameof(SendProposalActivity),
            loanInfo);
    }

    /// <summary>
    /// Wait for customer decision.
    /// </summary>
    private async Task<CustomerDecisionResult> WaitForCustomerDecision(WorkflowContext context)
    {
        bool proposalWasAccepted = false;
        bool customerDecisionWasReceivedWithinTimeout = true;

        try
        {
            await Log(context, $"Waiting for external event: CustomerProposalDecisionReceived ...");

            var customerProposalDecisionReceived =
                await context.WaitForExternalEventAsync<CustomerProposalDecisionReceived>(
                    "CustomerProposalDecisionReceived", TimeSpan.FromDays(14));

            proposalWasAccepted = customerProposalDecisionReceived.Accepted;

            await Log(context, $"Proposal was {(proposalWasAccepted ? "Accepted" : "Declined")}.");
        }
        catch (TaskCanceledException)
        {
            // Customer decision timeout expired
            customerDecisionWasReceivedWithinTimeout = false;
        }

        return new CustomerDecisionResult( 
            proposalWasAccepted, 
            customerDecisionWasReceivedWithinTimeout);
    }    

    /// <summary>
    /// Contact customer for decision.
    /// </summary>
    private async Task<bool> ContactCustomerForDecision(WorkflowContext context)
    {
        await Log(context, $"Waiting for external event: CustomerContactedForProposal ...");

        var customerContactedForProposal =
            await context.WaitForExternalEventAsync<CustomerContactedForProposal>(
                "CustomerContactedForProposal");

        bool proposalWasAccepted = customerContactedForProposal.Accepted;

        await Log(context, $"Loan proposal was {(proposalWasAccepted ? "Accepted" : "Declined")}.");
        
        return proposalWasAccepted;
    }    

    /// <summary>
    /// Register new contract.
    /// </summary>
    private static async Task RegisterContract(WorkflowContext context, LoanInfo loanInfo)
    {
        await context.CallActivityAsync<object?>(
            nameof(RegisterContractActivity),
            loanInfo);
    }

    /// <summary>
    /// Send contract and accompanying documentation to the customer.
    /// </summary>
    private async Task SendContract(WorkflowContext context, LoanInfo loanInfo)
    {
        await context.CallActivityAsync<object?>(
            nameof(SendContractActivity),
            loanInfo);
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