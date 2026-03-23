using Ipfs.Http;
using Ipfs.CoreApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.Exceptions;
using proiect_licenta.Models;

namespace proiect_licenta.Services;

public class FileService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IpfsClient _ipfsClient;


    public FileService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IpfsClient ipfsClient)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _ipfsClient = ipfsClient;
    }
    
    public async Task<AppFile> UploadFileToIpfs(IFormFile file, int appId)
    {
        var stream = file.OpenReadStream();
        var node = await _ipfsClient.FileSystem.AddAsync(stream, file.FileName);
        var appFile = new AppFile
        {
            Cid = node.Id.Hash.ToString(),
            FileName = file.FileName,
            AppId = appId
        };

        _context.AppFiles.Add(appFile);
        
        var app = await _context.Apps.FindAsync(appId);
        app.AppFileCid = appFile.Cid;
        
        await _context.SaveChangesAsync();

        return appFile;
    }

    public async Task<(Stream stream, string fileName)> DownloadFileFromIpfs(string cid)
    {
        var appFile = await _context.AppFiles.FindAsync(cid);
        if (appFile == null)
            throw new FileNotFoundException();
        
        // library is outdated on read side, requests use POST instead of GET
        var http = new HttpClient();
        var url = $"http://host.docker.internal:5001/api/v0/cat?arg={cid}";
        var response = await http.PostAsync(url, null); // must be POST
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        
        //var stream = await _ipfsClient.FileSystem.ReadFileAsync($"/ipfs/{cid}");
        return (stream, appFile.FileName);
    }

    public async Task<ActionResult<string>> UploadImagesToFilesystem(List<IFormFile> files, int appId, string type)
    {
        switch (type)
        {
            case "icon" when files.Count != 1:
                throw new Exception("Cannot upload more than one icon");
            case "icon":
            {
                Console.WriteLine("icon");
                var file = files[0];
                if (file.Length == 0 || file == null)
                    throw new FileNotFoundException("File not found");
        
                if (file.Length > 1024 * 1024 * 8)
                    throw new Exception("File is too large");
            
                var extension = Path.GetExtension(file.FileName);
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowedExtensions.Contains(extension))
                    throw new Exception("Incorrect file type");

                string fileName = $"{Guid.NewGuid()}{extension}";
                string basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "apps", appId.ToString());
                Console.WriteLine("Writing to: " + basePath);

                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);

                string fullPath = Path.Combine(basePath, fileName);
        
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string urlPath = $"/apps/{appId}/{fileName}";

                var appImage = new AppImage
                {
                    ImageType = "icon",
                    AppId = appId,
                    Path = urlPath
                };

                _context.AppImages.Add(appImage);
                await _context.SaveChangesAsync();
                break;
            }
            case "screenshot":
            {
                Console.WriteLine("screenshot");
                foreach (var file in files)
                {
                    if (file.Length == 0)
                        continue;
        
                    if (file.Length > 1024 * 1024 * 8)
                        throw new Exception("File is too large");
        
                    var extension = Path.GetExtension(file.FileName);
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    if (!allowedExtensions.Contains(extension))
                        throw new Exception("Incorrect file type");

                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var basePath = Path.Combine("wwwroot", "apps", appId.ToString());

                    if (!Directory.Exists(basePath))
                        Directory.CreateDirectory(basePath);

                    var fullPath = Path.Combine(basePath, fileName);
        
                    await using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var urlPath = $"/apps/{appId}/{fileName}";

                    var appImage = new AppImage
                    {
                        ImageType = "screenshot",
                        AppId = appId,
                        Path = urlPath
                    };

                    _context.AppImages.Add(appImage);
                    await _context.SaveChangesAsync();
                }

                break;
            }
            
            default:
                throw new FileNotFoundException("Invalid type");
        }
        
        return "File(s) uploaded successfully";
    }

    public async Task<string[]> GetImagesFromFilesystem(int appId, string type)
    {
        switch (type)
        {
            case "icon":
            {
                var file = await _context.AppImages
                    .Where(f => f.AppId == appId && f.ImageType == "icon")
                    .FirstOrDefaultAsync();
                if (file == null)
                    throw new FileNotFoundException("File not found");
                return [file.Path];
            }
            case "screenshot":
            {
                var files = await _context.AppImages
                    .Where(f => f.AppId == appId && f.ImageType == "screenshot")
                    .ToListAsync();
                if (files.Count == 0)
                    throw new FileNotFoundException("Files not found");
                return files.Select(f => f.Path).ToArray();
            }
            default:
                throw new FileNotFoundException("Invalid type");
        }
    }
}