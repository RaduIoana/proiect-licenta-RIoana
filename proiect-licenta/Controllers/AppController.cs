using Microsoft.AspNetCore.Mvc;
using proiect_licenta.Models;

namespace proiect_licenta.Controllers;

[ApiController]
[Route("[controller]")]
public class AppController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<AppController> _logger;

    public AppController(ILogger<AppController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetApp")]
    public IEnumerable<App> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new App
            {
                Name = "App" + index,
                Description = Summaries[index],
                Price = index,
                LaunchDate = DateTime.Now.AddDays(index),
                Rating = index,
                Discount = 50
            })
            .ToArray();
    }
}