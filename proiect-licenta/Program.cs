using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using proiect_licenta.Services;

Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

if (environment == "Development")
{
    DotNetEnv.Env.Load(); // Only loads .env locally
}
var key = Environment.GetEnvironmentVariable("JWT__KEY");
var issuer = Environment.GetEnvironmentVariable("JWT__ISSUER");
var audience = Environment.GetEnvironmentVariable("JWT__AUDIENCE");

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(8080);
});

// for on-chain testing:
//var web3 = new Web3($"https://mainnet.infura.io/v3/{Environment.GetEnvironmentVariable("INFURA__API")}");
//for local testing:
var localKey = Environment.GetEnvironmentVariable("OWNER__ACCOUNT__KEY__LOCAL");
var web3 = new Web3(new Account(localKey), "http://host.docker.internal:8545");

// check
//var balance = await web3.Eth.GetBalance.SendRequestAsync(Environment.GetEnvironmentVariable("0xBa7661DC6603A6D2c1b4fF58d2F5675211C1A7D6"));
//var etherAmount = Web3.Convert.FromWei(balance.Value);
//Console.WriteLine($"Balance in Ether: {etherAmount}");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddIdentity<MyUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders().AddApiEndpoints();

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
builder.Services.AddScoped<AppService>();
builder.Services.AddScoped<AppstoreService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CardService>();
builder.Services.AddScoped<InstallService>();
builder.Services.AddScoped<LicenseGenerationService>();
builder.Services.AddScoped<MetaAuthService>();
builder.Services.AddScoped<PayRecordService>();
builder.Services.AddScoped<RefundService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<UserAccService>();
builder.Services.AddScoped<VoucherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();