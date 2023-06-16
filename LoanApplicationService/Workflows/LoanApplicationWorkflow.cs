namespace LoanApplicationService.Workflows;

using System.Threading.Tasks;
using Dapr.Workflow;
using LoanApplicationService.Activities;
using LoanApplicationService.Models;

public class LoanApplicationWorkflow : Workflow<LoanApplication, ApplicationResult>
{
    private string? _applicantName;
    private decimal _loanAmount;
    private decimal _yearlyGrossSalary;
    private bool _existingCustomer = false;
    private decimal? _outstandingAmount;
    private bool? _hasDefaulted;
    private int? _riskClass;
    private string[]? _riskClauses;

    public override async Task<ApplicationResult> RunAsync(WorkflowContext context, LoanApplication application)
    {
        var logger = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        }).CreateLogger<LoanApplicationWorkflow>();

        try
        {
            _applicantName = application.ApplicantName;
            _loanAmount = application.LoanAmount;
            _yearlyGrossSalary = application.YearlyGrossSalary;

            // Determine whether the applicant is already an existing customer with us
            var customerInfo = await context.CallActivityAsync<CustomerInfo>(
                nameof(DetermineExistingCustomerActivity),
                _applicantName);
            if (customerInfo != null)
            {
                _existingCustomer = true;
                _outstandingAmount = customerInfo.OutstandingAmount;
                _hasDefaulted = customerInfo.HasDefaulted;
            }

            // If the customer is new, register a new prospect
            ProspectInfo prospectInfo;
            if (!_existingCustomer)
            {
                prospectInfo = await context.CallActivityAsync<ProspectInfo>(
                    nameof(RegisterProspectActivity),
                    _applicantName);
            }

            // Determine the Risk profile of the loan
            var applicationInfo = new ApplicationInfo(
                LoanAmount: _loanAmount,
                YearlyGrossSalary: _yearlyGrossSalary,
                ExistingCustomer: _existingCustomer,
                OutstandingAmount: _outstandingAmount,
                HasDefaulted: _hasDefaulted
            );
            var riskProfile = await context.CallActivityAsync<RiskProfile>(
                nameof(DetermineRiskProfileActivity),
                applicationInfo);
            _riskClass = riskProfile.RiskClass;
            _riskClauses = riskProfile.RiskClauses;

            // Contact the customer
            logger.LogInformation($"[Workflow {context.InstanceId}] Waiting for external event: CustomerContacted ...");
            var customerContacted = await context.WaitForExternalEventAsync<CustomerContacted>("CustomerContacted", TimeSpan.FromSeconds(120));
            logger.LogInformation($"[Workflow {context.InstanceId}] Received external event: CustomerContacted.");

            var proposalResult = customerContacted == null ? "unknown" : 
                customerContacted.Accepted ? "accepted" : "declined";
            logger.LogInformation($"[Workflow {context.InstanceId}] Proposal for loan application was {proposalResult}.");

            return new ApplicationResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
}