namespace LoanApplicationService.Models;

public record ProspectInfo(string Name);
public record LoanApplication(string ApplicantName, decimal LoanAmount, decimal YearlyGrossSalary);
public record ApplicationResult(bool Approved);