using System.Text.Json;
using Dapr.Client;
using Dapr.Workflow;
using LoanApplicationService.Workflows;
using LoanApplicationService.Workflows.Activities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDaprClient();

builder.Services.AddDaprWorkflow(options =>
{
    // Register workflows
    options.RegisterWorkflow<LoanApplicationWorkflow>();

    // Register activities
    options.RegisterActivity<LogActivity>();
    options.RegisterActivity<DetermineExistingCustomerActivity>();
    options.RegisterActivity<DetermineRiskProfileActivity>();
    options.RegisterActivity<RegisterCustomerActivity>();
    options.RegisterActivity<SendProposalActivity>();
    options.RegisterActivity<SendRejectionLetterActivity>();
    options.RegisterActivity<RegisterContractActivity>();
    options.RegisterActivity<SendContractActivity>();
});

// Dapr uses a random ports by default. If we don't know what these ports are,
// (because this app was started separate from dapr), then assume 3500 and 4001.
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_HTTP_PORT")))
{
    Environment.SetEnvironmentVariable("DAPR_HTTP_PORT", "3500");
}
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_GRPC_PORT")))
{
    Environment.SetEnvironmentVariable("DAPR_GRPC_PORT", "4001");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
