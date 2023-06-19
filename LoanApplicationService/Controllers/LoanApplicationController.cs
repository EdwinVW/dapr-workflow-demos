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

    private string GenerateId()
    {
        var ticks = new DateTime(2016, 1, 1).Ticks;
        var diff = DateTime.Now.Ticks - ticks;
        return diff.ToString("x");
    }
}
