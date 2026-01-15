using b1.Authentication;
using b1.Configs;
using b1.Main;
using b1.Repositories;
using b1.Services;
using b1.Srevices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using sadna.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(opts =>
{
    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(jwtOptions =>
{
    jwtOptions.RequireHttpsMetadata = false;
    jwtOptions.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ClockSkew = TimeSpan.Zero,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["JwtSettings:Key"]!)),
    };
});

builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoSettings"));

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));
    
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    var service = new MongoClient(settings.ConnectionString);
    return service;

});
//added the MongoDB instance to inject
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);

});

builder.Services.AddSingleton<IUserRepository>(sp =>
{
    var context = sp.GetRequiredService<IMongoDatabase>();
    return new MongoUserRepository(context);
});
builder.Services.AddSingleton<TokenProvider>();
builder.Services.AddScoped<UserAuthenticator>();
builder.Services.AddScoped<PreProcessor>();

//also created a Context to hold our prices, the project is small so I use a ConcurrentDictionary
//but maybe Redis is worth it, because we can distribute Redis too.
//if we do go for Redis, we could start thinking of what we can cache, maybe price histories for most popular stocks etc.
builder.Services.AddSingleton<PriceContextHolder>(sp =>
{
    return new PriceContextHolder();
});

builder.Services.AddSingleton<StockPriceService>(sp =>
{
    var db = sp.GetService<IMongoDatabase>();
    var ctx = sp.GetService<PriceContextHolder>();
    if (ctx != null && db != null)
        return new StockPriceService(ctx, db);
    else
    {
        throw new ArgumentNullException("PriceCcontextHolder OR Database instance didn't initialize properly");
    }
});
builder.Services.AddSingleton<EtfPriceService>(sp =>
{
    var db = sp.GetService<IMongoDatabase>();
    var ctx = sp.GetService<PriceContextHolder>();
    if (ctx != null && db != null)
        return new EtfPriceService(ctx, db);
    else
    {
        throw new ArgumentNullException("PriceCcontextHolder OR Database instance didn't initialize properly");
    }
});
builder.Services.AddSingleton<FxPriceService>(sp =>
{
    var db = sp.GetService<IMongoDatabase>();
    var ctx = sp.GetService<PriceContextHolder>();
    if (ctx != null && db != null)
        return new FxPriceService(ctx, db);
    else
    {
        throw new ArgumentNullException("PriceCcontextHolder OR Database instance didn't initialize properly");
    }
});

builder.Services.AddSingleton<PriceHistoryManager>(sp =>
{
    var db = sp.GetService<IMongoDatabase>();
    var ctx = sp.GetService<PriceContextHolder>();
    if (ctx != null && db != null)
        return new PriceHistoryManager(ctx, db);
    else
    {
        throw new ArgumentNullException("PriceCcontextHolder OR Database instance didn't initialize properly");
    }
});

builder.Services.AddScoped<UserService>(sp =>
{
    var userRepo = sp.GetRequiredService<IUserRepository>();
    return new UserService(userRepo);
});


builder.Services.AddHostedService<StockPriceService>();
builder.Services.AddHostedService<EtfPriceService>();
builder.Services.AddHostedService<FxPriceService>();

builder.Services.AddHostedService<PriceHistoryManager>();

builder.Services.AddSingleton<IPortfolioRepository>(sp =>
{
    var db = sp.GetRequiredService<IMongoDatabase>();
    return new MongoPortfolioRepo(db);
});
builder.Services.AddScoped<PortfolioService>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
using (var scp = app.Services.CreateScope())
{
    var preProcessor = scp.ServiceProvider.GetService<PreProcessor>();
    if(preProcessor != null)
        await preProcessor.Run();
}
app.Run();

