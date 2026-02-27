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
using b1;
using b1.Respositories;
using System.Windows.Input;
using b1.Repositoris;
using b1.Infrastructure;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Diagnostics;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


//===================== CONFIGURATIONS =================================
JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddAuthorization();
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestBody;
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
//****************************************************************
//===================== FILTERS  =================================

//==================== END FILTERS ===============================


//===================== SERVICES =================================
//added the MongoDB instance to inject
builder.Services.AddSingleton<IPositionVerificationRepository>(sp =>
{
    return InMemoryPositionVerificationRepo.GetInstance();
});
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);

});
//builder.Services.AddSingleton<InMemoryCache>();
builder.Services.AddSingleton<IUserRepository>(sp =>
{
    var db = sp.GetRequiredService<IMongoDatabase>();
    return new MongoUserRepository(db);
});
builder.Services.AddSingleton<IChartDataRepository, MongoChartsDataRepo>();
builder.Services.AddSingleton<IPositionRepository, MongoPositionRepo>();
builder.Services.AddSingleton<ITickerRepository, MongoTickerRepo>();
builder.Services.AddSingleton<ICommandRepository, MongoCommandRepo>();
builder.Services.AddSingleton<IPortfolioRepository, MongoPortfolioRepo>();

builder.Services.AddSingleton<IMessageChannel>(sp =>
{
    return DefaultMessageChannel.GetInstance();
});
builder.Services.AddSingleton<ICommandChannel, DefaultCommandChannel>();
builder.Services.AddSingleton<IReadOnlyNewsRepository, MongoReadonlyNewsRepo>();

builder.Services.AddSingleton<RegularPositionCommandProvider>();
builder.Services.AddSingleton<AdvancedPositionCommandProvider>();
builder.Services.AddSingleton<AssetService>();
builder.Services.AddSingleton<PriceContext>(sp =>
{
    var channel = sp.GetRequiredService<IMessageChannel>();
    return new PriceContext(channel);
});
builder.Services.AddScoped<TokenProvider>();
builder.Services.AddScoped<UserAuthenticator>();
builder.Services.AddScoped<PreProcessor>();
builder.Services.AddScoped<NewsService>();
// services responsible for putting the prices in their respective historical location
builder.Services.AddSingleton<PriceHistoryManager>(sp =>
{
    var db = sp.GetRequiredService<IMongoDatabase>(); //singleton
    var ctx = sp.GetRequiredService<PriceContext>(); //singleton

    return new PriceHistoryManager(ctx, db);
});
builder.Services.AddScoped<RegularCommandCreator>();
builder.Services.AddScoped<AdvancedCommandCreator>();

builder.Services.AddScoped<PositionService>(sp =>
{
    var posVerRepo = sp.GetRequiredService<IPositionVerificationRepository>();
    var ptfRepo = sp.GetRequiredService<IPortfolioRepository>();
    var posRepo = sp.GetRequiredService<IPositionRepository>();
    var usrRepo = sp.GetRequiredService<IUserRepository>();
    var pc = sp.GetRequiredService<PriceContext>();
    var cmdRepo = sp.GetRequiredService<ICommandRepository>();
    var cmdChan = sp.GetRequiredService<ICommandChannel>();
    return new PositionService(posRepo, ptfRepo, usrRepo, pc, sp, cmdRepo, posVerRepo, cmdChan);
});
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddHostedService<StockPriceBackgroundService>();
builder.Services.AddHostedService<EtfPriceBackgroundService>();
builder.Services.AddHostedService<FxPriceBackgroundService>();

builder.Services.AddHostedService<PriceHistoryManager>();

builder.Services.AddScoped<PortfolioService>();
//===================== BUILDING =================================
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    //app.UseHttpLogging(); //this is global logging, it logs too much, sometimes I want my own logger
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

//pre processing steps or setup for buses etc.
using (var scp = app.Services.CreateScope())
{
    var newsCommandExecutror = new Process()
    {
        StartInfo ={
            FileName = "/usr/bin/python3",
            Arguments = "/Users/korsunskyroma/b1/news.py"
        }
    };
    newsCommandExecutror.Start();
    var regularProvider = scp.ServiceProvider.GetRequiredService<RegularPositionCommandProvider>();
    var advancedProvider = scp.ServiceProvider.GetRequiredService<AdvancedPositionCommandProvider>();
    var preProcessor = scp.ServiceProvider.GetService<PreProcessor>();
    if (preProcessor != null)
        await preProcessor.Run();
}
app.Run();

