using CustomerService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Controllers;

[ApiController]
[Route("[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ILogger<CustomerController> _logger;
    private readonly Random _random = new Random();

    public CustomerController(ILogger<CustomerController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{customerName}")]
    public ActionResult<CustomerInfo> Get(string customerName)
    {
        if (customerName.ToLowerInvariant() == "john doe")
        {
            return NotFound();
        }

        // Generate random demo customer info based on the input
        CustomerInfo info;
        if (customerName.ToLowerInvariant() == "eric white")
        {
            info = new CustomerInfo(
                Id: Guid.NewGuid().ToString("D"),
                Name: customerName,
                OutstandingAmount: 12500,
                HasDefaulted: false);
        }
        else if (customerName.ToLowerInvariant() == "eric grey")
        {
            info = new CustomerInfo(
                Id: Guid.NewGuid().ToString("D"),
                Name: customerName,
                OutstandingAmount: 105000,
                HasDefaulted: false);
        }
        else if (customerName.ToLowerInvariant() == "eric black")
        {
            info = new CustomerInfo(
                Id: Guid.NewGuid().ToString("D"),
                Name: customerName,
                OutstandingAmount: 112000,
                HasDefaulted: true);
        }
        else 
        {
            info = new CustomerInfo(
                Id: Guid.NewGuid().ToString("D"),
                Name: customerName,
                OutstandingAmount: _random.Next(10000, 100000),
                HasDefaulted: _random.Next(10) % 2 == 0);
        }

        return new OkObjectResult(info);
    }
}
