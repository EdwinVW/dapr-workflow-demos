namespace LoanApplicationService.Models;

public record CustomerDecisionResult(
    bool ProposalWasAccepted, 
    bool CustomerDecisionWasReceivedWithinTimeout
);
