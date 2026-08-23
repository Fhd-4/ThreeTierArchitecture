using Microsoft.AspNetCore.Mvc;
using Project.BLL.Services;

namespace Project.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var result = _service.GetProducts();
        return Ok(result);
    }
}