namespace LoanApplicationService.Models;

public record LoanInfo(
    ApplicationInfo ApplicationInfo, 
    RiskProfile RiskProfile
);
