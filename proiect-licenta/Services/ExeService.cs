using Ipfs.Http;
using Ipfs.CoreApi;
using proiect_licenta.Contexts;
using proiect_licenta.Models;

namespace proiect_licenta.Services;

public class ExeService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IpfsClient _ipfsClient;


    public ExeService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IpfsClient ipfsClient)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _ipfsClient = ipfsClient;
    }
    
    public async Task<AppFile> UploadFile(IFormFile file, int appId)
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

    public async Task<(Stream stream, string fileName)> DownloadFile(string cid)
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
}