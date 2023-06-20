namespace LoanApplicationService.Models;

public record CustomerInfo(
    string Id, 
    string Name,
    decimal OutstandingAmount, 
    bool HasDefaulted
);
