using Ipfs.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using proiect_licenta;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using proiect_licenta.Services;

Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

if (environment == "Development")
{
    DotNetEnv.Env.Load();
}

var ipfs = new IpfsClient("http://host.docker.internal:5001");

var key = Environment.GetEnvironmentVariable("JWT__KEY");
var issuer = Environment.GetEnvironmentVariable("JWT__ISSUER");
var audience = Environment.GetEnvironmentVariable("JWT__AUDIENCE");

// for on chain testing:
var liveAcc = new Account(Environment.GetEnvironmentVariable("OWNER__ACCOUNT__KEY__LIVE"));
var web3 = new Web3(liveAcc, $"https://sepolia.infura.io/v3/{Environment.GetEnvironmentVariable("INFURA__API")}");
//for local testing:
//var localKey = Environment.GetEnvironmentVariable("OWNER__ACCOUNT__KEY__LOCAL");
//var web3 = new Web3(new Account(localKey), "http://host.docker.internal:8545");

// test connection
var balance = await web3.Eth.GetBalance.SendRequestAsync(Environment.GetEnvironmentVariable("OWNER__ACCOUNT__ADDR"));
var etherAmount = Web3.Convert.FromWei(balance);
Console.WriteLine($"Balance in Ether: {etherAmount}");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Content-Disposition");
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddIdentity<MyUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddApiEndpoints();

var signingKey = new SymmetricSecurityKey(Convert.FromBase64String(key));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(jwtOptions =>
{
    jwtOptions.Authority = issuer;
    jwtOptions.Audience = audience;
    jwtOptions.TokenValidationParameters = new TokenValidationParameters{
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = signingKey
    };
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddSingleton(web3);
builder.Services.AddSingleton<IpfsClient>(_ => ipfs);
builder.Services.AddScoped<AppService>();
builder.Services.AddScoped<AppstoreService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<LibraryService>();
builder.Services.AddScoped<LicenseService>();
builder.Services.AddScoped<MetaAuthService>();
builder.Services.AddScoped<PaymentRecordService>();
builder.Services.AddScoped<RefundService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<UserAccService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<CategoryService>();

var app = builder.Build();

var roleManager = app.Services.CreateScope()
    .ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
if (!await roleManager.RoleExistsAsync("USER"))
    await roleManager.CreateAsync(new IdentityRole("USER"));
if (!await roleManager.RoleExistsAsync("DEVELOPER"))
    await roleManager.CreateAsync(new IdentityRole("DEVELOPER"));
if (!await roleManager.RoleExistsAsync("ADMIN"))
    await roleManager.CreateAsync(new IdentityRole("ADMIN"));

var fileProviderUrl = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(fileProviderUrl))
    Directory.CreateDirectory(fileProviderUrl);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(fileProviderUrl),
    RequestPath = ""
});
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();

app.MapControllers();

app.Run();