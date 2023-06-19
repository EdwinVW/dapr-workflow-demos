using LoanApplicationService.Models;

public static class RiskProfileExtensions
{
    public static string Print(this RiskProfile riskProfile)
    {
        return $"Class {riskProfile.RiskClass}, Clauses: {string.Join(",", riskProfile.RiskClauses.Select(s => s.ToString()).ToArray())}";        
    }
}