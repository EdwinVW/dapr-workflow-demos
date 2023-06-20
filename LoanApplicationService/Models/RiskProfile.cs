namespace LoanApplicationService.Models;

public record RiskProfile(
    int RiskClass, 
    RiskClause[] RiskClauses
)
{
    public string Print()
    {
        return $"Class {RiskClass}, Clauses: {string.Join(",", RiskClauses.Select(s => s.ToString()).ToArray())}";
    }    
}
