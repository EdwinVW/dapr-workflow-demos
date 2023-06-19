namespace LoanApplicationService.Models;

public record LoanApplication(string ApplicantName, decimal LoanAmount, decimal YearlyGrossSalary);
public record LoanApplicationStarted(string InstanceId);
public record CustomerInfo(string Id, string Name,decimal OutstandingAmount, bool HasDefaulted);
public record ProspectInfo(string Id, string Name);
public record ApplicationInfo(decimal LoanAmount, decimal YearlyGrossSalary, bool ExistingCustomer, decimal? OutstandingAmount, bool? HasDefaulted);
public record RiskProfile(int RiskClass, RiskClause[] RiskClauses);
public record LoanAssessmentCompleted(bool Approved);
public record LoanInfo(ApplicationInfo ApplicationInfo, RiskProfile RiskProfile);
public record CustomerProposalDecisionReceived(bool Accepted);
public record CustomerContactedForProposal(bool Accepted);
public record ApplicationResult(bool Approved);

public enum RiskClause
{
    A,
    B,
    C
}