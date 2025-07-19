using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nethereum.Web3;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using proiect_licenta.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

if (environment == "Development")
{
    DotNetEnv.Env.Load(); // Only loads .env locally
}
//DotNetEnv.Env.Load();
var key = Environment.GetEnvironmentVariable("JWT__KEY");
var issuer = Environment.GetEnvironmentVariable("JWT__ISSUER");
var audience = Environment.GetEnvironmentVariable("JWT__AUDIENCE");

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(8080);
});


// FIX ME - store this nicely and securely
//var web3 = new Web3("https://mainnet.infura.io/v3/d0830a3002cf4e00a65cb05ce6b31cf8");
//var balance = await web3.Eth.GetBalance.SendRequestAsync("0xde0b295669a9fd93d5f28d9ec85e40f4cb697bae");
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

var signingKey = new SymmetricSecurityKey(Convert.FromBase64String(key));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(jwtOptions =>
    {
        jwtOptions.Authority = issuer;
        jwtOptions.Audience = audience;
        jwtOptions.TokenValidationParameters = new TokenValidationParameters{
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = signingKey //placeholder?
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddIdentity<MyUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders().AddApiEndpoints();

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