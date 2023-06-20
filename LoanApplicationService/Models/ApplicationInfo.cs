namespace LoanApplicationService.Models;

public record ApplicationInfo(
    decimal LoanAmount, 
    decimal YearlyGrossSalary, 
    bool ExistingCustomer, 
    decimal? OutstandingAmount, 
    bool? HasDefaulted
);
