using Dapr.Workflow;
using FrontEnd.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddDaprWorkflow(options => {});

// Dapr uses a random ports by default. If we don't know what that port
// is (because this app was started separate from dapr), then assume 3500 and 4001.
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_HTTP_PORT")))
{
    Environment.SetEnvironmentVariable("DAPR_HTTP_PORT", "3700");
}
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_GRPC_PORT")))
{
    Environment.SetEnvironmentVariable("DAPR_GRPC_PORT", "4002");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
