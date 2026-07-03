using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nethereum.Web3;
using proiect_licenta.Contexts;
using proiect_licenta.Exceptions;
using proiect_licenta.Models;

namespace proiect_licenta.Services;

public class CategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CategoryService(ApplicationDbContext context, Web3 web3, 
        IHttpContextAccessor httpContextAccessor, UserManager<MyUser> userManager)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<IEnumerable<Category>> GetAllCategories()
    {
        var categories = await _context.Categories.ToListAsync();
        return categories;
    }
    
    public async Task<IEnumerable<Category>> GetAppCategories(int id)
    {
        var app = await _context.Apps.Include(a => a.AppCategories)
            .FirstOrDefaultAsync(a => a.Id == id);
        var categories = new List<Category>();
        foreach (var appCategory in app.AppCategories)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == appCategory.CategoryId);
            if (category != null)
                categories.Add(category);
        }
        return categories;
    }
    
    public async Task<Category> PostCategory(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }
    
    public async Task<Category> PutCategory(Category category)
    {
        var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == category.Id);
        if (existingCategory == null)
            throw new NotFoundException("Category not found");
        
        existingCategory.Name = category.Name;
        _context.Entry(existingCategory).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return existingCategory;
    }

    public async Task DeleteCategory(int id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            throw new NotFoundException("Category not found");
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }

    public async Task AssignAppCategories(int appId, int[] categoryIds)
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found");
        
        var app = await _context.Apps
            .Include(a => a.AppCategories)
            .FirstOrDefaultAsync(a => a.Id == appId);
        if (app == null)
            throw new NotFoundException("App not found");
        
        //remove categories not in the list (de-assignment)
        var removals = app.AppCategories.Where(ac => !categoryIds.Contains(ac.CategoryId));
        foreach (var removal in removals)
        {
            _context.AppCategories.Remove(removal);
        }

        foreach (var categoryId in categoryIds)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            // if null, just skip
            if (category != null)
            {
                if (!app.AppCategories.Any(ac => ac.AppId == appId && ac.CategoryId == categoryId))
                {
                    var appCategory = new AppCategory { CategoryId = categoryId, AppId = appId };
                    Console.WriteLine("adding category: " + appCategory.CategoryId + " " + appCategory.AppId);

                    app.AppCategories.Add(appCategory);
                    category.AppCategories.Add(appCategory);
                }
            }
        }
        
        await _context.SaveChangesAsync();
    }
}