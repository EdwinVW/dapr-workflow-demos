using Dapr.Workflow;
using LoanApplicationService.Models;
using LoanApplicationService.Workflows;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplicationService.Controllers;

[ApiController]
[Route("[controller]")]
public class LoanApplicationController : ControllerBase
{
    private readonly ILogger<LoanApplicationController> _logger;
    private readonly DaprWorkflowClient _workflowClient;

    public LoanApplicationController(
        ILogger<LoanApplicationController> logger, 
        DaprWorkflowClient workflowClient)
    {
        _logger = logger;
        _workflowClient = workflowClient;
    }

    [HttpPost("")]
    public async Task<LoanApplicationStarted> StartLoanApplication(
        LoanApplication loanApplication)
    {
        var instanceId = GenerateId();

        await _workflowClient.ScheduleNewWorkflowAsync(
            name: nameof(LoanApplicationWorkflow),
            instanceId: instanceId,
            input: loanApplication);

        return new LoanApplicationStarted(instanceId);
    }

    [HttpPost("{instanceId}/RaiseLoanAssessmentCompleted")]
    public async Task<ActionResult> RaiseLoanAssessmentCompleted(
        string instanceId,
        [FromBody] LoanAssessmentCompleted loanAssessmentCompleted)
    {
        var assessmentResult = loanAssessmentCompleted.Approved ? "Approved" : "Rejected";
        _logger.LogInformation($"Raising LoanAssessmentCompleted for {instanceId} with result {assessmentResult}...");
        
        await _workflowClient.RaiseEventAsync(
            instanceId: instanceId,
            eventName: "LoanAssessmentCompleted",
            eventPayload: loanAssessmentCompleted);

        return Ok();
    }    

    [HttpPost("{instanceId}/RaiseCustomerProposalDecisionReceived")]
    public async Task<ActionResult> RaiseCustomerProposalDecisionReceived(
        string instanceId,
        [FromBody] CustomerProposalDecisionReceived customerProposalDecisionReceived)
    {
        var decisionResult = customerProposalDecisionReceived.Accepted ? "Accepted" : "Declined";
        _logger.LogInformation($"Raising CustomerProposalDecisionReceived for {instanceId} with result {decisionResult}...");
        
        await _workflowClient.RaiseEventAsync(
            instanceId: instanceId,
            eventName: "CustomerProposalDecisionReceived",
            eventPayload: customerProposalDecisionReceived);

        return Ok();
    }      

    [HttpPost("{instanceId}/RaiseCustomerContactedForProposal")]
    public async Task<ActionResult> RaiseCustomerContactedForProposal(
        string instanceId,
        [FromBody] CustomerContactedForProposal customerContactedForProposal)
    {
        var decisionResult = customerContactedForProposal.Accepted ? "Accepted" : "Declined";
        _logger.LogInformation($"Raising CustomerContactedForProposal for {instanceId} with result {decisionResult}...");
        
        await _workflowClient.RaiseEventAsync(
            instanceId: instanceId,
            eventName: "CustomerContactedForProposal",
            eventPayload: customerContactedForProposal);

        return Ok();
    }     

    private string GenerateId()
    {
        var ticks = new DateTime(2016, 1, 1).Ticks;
        var diff = DateTime.Now.Ticks - ticks;
        return diff.ToString("x");
    }
}
