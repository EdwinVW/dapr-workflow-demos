namespace LoanApplicationService.Models;

public record LoanApplication(string ApplicantName, decimal LoanAmount, decimal YearlyGrossSalary);
public record CustomerInfo(string Id, string Name,decimal OutstandingAmount, bool HasDefaulted);
public record ProspectInfo(string Id, string Name);
public record ApplicationInfo(decimal LoanAmount, decimal YearlyGrossSalary, bool ExistingCustomer, decimal? OutstandingAmount, bool? HasDefaulted);
public record RiskProfile(int RiskClass, string[] RiskClauses);
public record CustomerContacted(bool Accepted);
public record ApplicationResult(bool Approved);