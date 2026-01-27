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
using b1.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


//===================== CONFIGURATIONS =================================
JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddAuthorization();
builder.Host.UseDefaultServiceProvider(opts =>
{
    opts.ValidateScopes = true;
});
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
    jwtOptions.Events = new JwtBearerEvents()
    {
        OnTokenValidated = ctx =>
        {
            Console.WriteLine(ctx.SecurityToken.ToString());
            return Task.CompletedTask;
        }
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
//===================== SERVICES =================================

//added the MongoDB instance to inject
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);

});
builder.Services.AddSingleton<MongoChartsDataRepo>();
//builder.Services.AddSingleton<InMemoryCache>();
builder.Services.AddSingleton<IUserRepository>(sp =>
{
    var db = sp.GetRequiredService<IMongoDatabase>();
    return new MongoUserRepository(db);
});
builder.Services.AddSingleton<IMessageChannel>(sp =>
{
    return new DefaultMessageChannel();
});
builder.Services.AddSingleton<AssetService>();
builder.Services.AddSingleton<PriceContext>(sp =>
{
    var channel = sp.GetRequiredService<IMessageChannel>();
    return new PriceContext(channel);
});
builder.Services.AddSingleton<ITickerRepository,MongoTickerRepo>();
builder.Services.AddScoped<TokenProvider>();
builder.Services.AddScoped<UserAuthenticator>();
builder.Services.AddScoped<PreProcessor>();

// services responsible for putting the prices in their respective historical location
builder.Services.AddSingleton<PriceHistoryManager>(sp =>
{
    var db = sp.GetRequiredService<IMongoDatabase>(); //singleton
    var ctx = sp.GetRequiredService<PriceContext>(); //singleton
    
    return new PriceHistoryManager(ctx, db);
});

builder.Services.AddScoped<UserService>();

builder.Services.AddHostedService<StockPriceBackgroundService>();
builder.Services.AddHostedService<EtfPriceBackgroundService>();
builder.Services.AddHostedService<FxPriceBackgroundService>();

builder.Services.AddHostedService<PriceHistoryManager>();

builder.Services.AddScoped<PortfolioService>();
//===================== BUILDING =================================
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
//properly start the PreProcessor
using (var scp = app.Services.CreateScope())
{
    var preProcessor = scp.ServiceProvider.GetService<PreProcessor>();
    if (preProcessor != null)
        await preProcessor.Run();
}
app.Run();

