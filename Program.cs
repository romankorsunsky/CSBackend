using b1.Configs;
using b1.Main;
using b1.Services;
using b1.Srevices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using sadna.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoSettings"));

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

builder.Services.AddScoped<UserRegService>(sp =>
{
    var db = sp.GetRequiredService<IMongoDatabase>();
    return new UserRegService(db);
});


builder.Services.AddHostedService<StockPriceService>();
builder.Services.AddHostedService<EtfPriceService>();
builder.Services.AddHostedService<FxPriceService>();

builder.Services.AddHostedService<PriceHistoryManager>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();
app.MapControllers();

using (var scp = app.Services.CreateScope())
{
    var preProcessor = scp.ServiceProvider.GetService<PreProcessor>();
    if(preProcessor != null)
        await preProcessor.Run(); //noder neder nedarim ze lo null
}
app.Run();

