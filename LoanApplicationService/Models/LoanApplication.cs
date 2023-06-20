namespace LoanApplicationService.Models;

public record LoanApplication(
    string ApplicantName, 
    decimal LoanAmount, 
    decimal YearlyGrossSalary
);
