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

    public LoanApplicationController(ILogger<LoanApplicationController> logger, DaprWorkflowClient workflowClient)
    {
        _logger = logger;
        this._workflowClient = workflowClient;
    }

    [HttpPost(Name = "StartLoanApplication")]
    public async Task<LoanApplicationStarted> StartLoanApplication(LoanApplication loanApplication)
    {
        var instanceId = Guid.NewGuid().ToString("D");
        
        _logger.LogInformation($"Starting Loan Application workflow with instanceId '{instanceId}'");

        await _workflowClient.ScheduleNewWorkflowAsync(
            name: nameof(LoanApplicationWorkflow),
            instanceId: instanceId,
            input: loanApplication);

        return new LoanApplicationStarted(instanceId);
    }

    [HttpPost("{instanceId}", Name = "CustomerContacted")]
    public async Task<IActionResult> CustomerContacted(string instanceId, CustomerContacted customerContacted)
    {
        var status = customerContacted.Accepted ? "accepted" : "declined";
        _logger.LogInformation($"Proposal for loan application with instanceId '{instanceId}' was {status}.");

        await _workflowClient.RaiseEventAsync(instanceId, "CustomerContacted", customerContacted);

        return Ok();
    }    
}
