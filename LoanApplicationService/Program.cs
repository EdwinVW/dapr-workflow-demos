using Dapr.Client;
using Dapr.Workflow;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using LoanApplicationService.Workflows;
using LoanApplicationService.Activities;
using LoanApplicationService.Models;

// The workflow host is a background service that connects to the sidecar over gRPC
var builder = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
{
    services.AddDaprWorkflow(options =>
    {
        // Note that it's also possible to register a lambda function as the workflow
        // or activity implementation instead of a class.
        options.RegisterWorkflow<LoanApplicationWorkflow>();

        // These are the activities that get invoked by the workflow(s).
        options.RegisterActivity<DetermineExistingCustomerActivity>();
    });
});

// Dapr uses a random ports by default. If we don't know what that port
// is (because this app was started separate from dapr), then assume 3500 and 4001.
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_HTTP_PORT")))
{
    Environment.SetEnvironmentVariable("DAPR_HTTP_PORT", "3500");
}
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_GRPC_PORT")))
{
    Environment.SetEnvironmentVariable("DAPR_GRPC_PORT", "4001");
}

using var host = builder.Build();
host.Start();

using var daprClient = new DaprClientBuilder().Build();

// Wait for the sidecar to become available
while (!await daprClient.CheckHealthAsync())
{
    Thread.Sleep(TimeSpan.FromSeconds(5));
}

// Wait one more second for the workflow engine to finish initializing.
// This is just to make the log output look a little nicer.
Thread.Sleep(TimeSpan.FromSeconds(1));

Console.WriteLine("Workflow engine initialized.");

var workflowClient = host.Services.GetRequiredService<WorkflowEngineClient>();

// Start the workflow
var instanceId = Guid.NewGuid().ToString("D");
Console.WriteLine($"Starting Loan Application workflow with instanceId '{instanceId}'");
var loanApplication = new LoanApplication("John Doe", 50000, 100000);
await workflowClient.ScheduleNewWorkflowAsync(
    name: nameof(LoanApplicationWorkflow),
    instanceId: instanceId,
    input: loanApplication);

// Wait for the workflow to complete
WorkflowState state;
do
{
    Thread.Sleep(1000);
    state = await workflowClient.GetWorkflowStateAsync(instanceId: instanceId, getInputsAndOutputs: true);
} while (!state.IsWorkflowCompleted);

if (state.RuntimeStatus == WorkflowRuntimeStatus.Completed)
{
    var result = state.ReadOutputAs<ApplicationResult>();
    if (result != null && result.Approved)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Loan Application workflow is {state.RuntimeStatus} and the application was approved.");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"Loan Appliction workflow is {state.RuntimeStatus} and the application was declined.");
        Console.ResetColor();
    }
}
else if (state.RuntimeStatus == WorkflowRuntimeStatus.Failed)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"The Loan Application workflow has failed.");
    Console.ResetColor();
}

Console.WriteLine();
