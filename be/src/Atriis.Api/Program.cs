using System.Net.Mime;
using Atriis.Api.Products.Services;
using Atriis.Api.Products.Services.HostedServices;
using Atriis.Api.Products.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services
    .AddCors(opts => opts.AddDefaultPolicy(
            policy => policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
        )
    )
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddMemoryCache()
    .AddHostedService<BbProductsSynchronizer>()
    .AddHttpClient("best-buy-products", cfg =>
    {
        cfg.BaseAddress = new Uri("https://api.bestbuy.com/");
        cfg.DefaultRequestHeaders.Add("Accept", MediaTypeNames.Application.Json);
    });

builder.Services.AddTransient<IBbProductsClient, BbProductsClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app
        .UseSwagger()
        .UseSwaggerUI()
        .UseCors();
}

app
    .UseHttpsRedirection()
    .UseAuthorization();

app.MapControllers();

app.Run();