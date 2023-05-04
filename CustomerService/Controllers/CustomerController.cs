using CustomerService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Controllers;

[ApiController]
[Route("[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ILogger<CustomerController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{customerName}")]
    public ActionResult<Customer> Get(string customerName)
    {
        if (customerName.ToLowerInvariant() == "john doe")
        {
            return NotFound();
        }
        return new OkObjectResult(new Customer(customerName));
    }
}
