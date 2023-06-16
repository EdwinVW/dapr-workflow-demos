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
        var instanceId = GenerateId();

        _logger.LogInformation($"Starting Loan Application workflow with instanceId '{instanceId}'");

        await _workflowClient.ScheduleNewWorkflowAsync(
            name: nameof(LoanApplicationWorkflow),
            instanceId: instanceId,
            input: loanApplication);

        return new LoanApplicationStarted(instanceId);
    }

    [HttpPost("{instanceId}/CustomerContacted", Name = "CustomerContacted")]
    public async Task<IActionResult> CustomerContacted(string instanceId, [FromBody] CustomerContacted customerContacted)
    {
        var result = customerContacted.Accepted ? "Accepted" : "Declined";
        _logger.LogInformation($"Raise external event 'CustomerContacted' (result: {result}).");
        await _workflowClient.RaiseEventAsync(instanceId, "CustomerContacted", customerContacted);
        return Ok();
    }

    private string GenerateId()
    {
        var ticks = new DateTime(2016, 1, 1).Ticks;
        var diff = DateTime.Now.Ticks - ticks;
        return diff.ToString("x");
    }
}
