using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proiect_licenta.Models;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers;

[Route("api/Categories")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoryController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [AllowAnonymous]
    [HttpGet("get_all_categories")]
    public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
    {
        return Ok(await _categoryService.GetAllCategories());
    }
    
    [HttpGet("forApp/{id}")]
    public async Task<ActionResult<IEnumerable<Category>>> GetAppCategories(int id)
    {
        return Ok(await _categoryService.GetAppCategories(id));
    }
    
    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public async Task<ActionResult<Category>> PostCategory(Category category)
    {
        return Ok(await _categoryService.PostCategory(category));
    }
    
    [Authorize(Roles = "ADMIN")]
    [HttpPut]
    public async Task<ActionResult<Category>> PutCategory(Category category)
    {
        return Ok(await _categoryService.PutCategory(category));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCategory(int id)
    {
        await _categoryService.DeleteCategory(id);
        return NoContent();
    }

    [Authorize(Roles = "ADMIN, DEVELOPER")]
    [HttpPut("assign/{appId}")]
    public async Task<ActionResult> AssignAppCategories(int appId, int[] categoryIds)
    {
        await _categoryService.AssignAppCategories(appId, categoryIds);
        return NoContent();
    }
}