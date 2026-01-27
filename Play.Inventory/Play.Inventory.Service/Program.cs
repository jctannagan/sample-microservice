using Microsoft.AspNetCore.Server.HttpSys;
using Play.Common;
using Play.Common.MongoDB;
using Play.Inventory.Service;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dto;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<ICatalogClient, CatalogClient>();
builder.Services.AddMongo().AddMongoRepository<InventoryItem>("inventory");

var jitterer = new Random();
builder.Services.AddHttpClient<ICatalogClient, CatalogClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5151");
})
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1))
.AddTransientHttpErrorPolicy(
    builder => builder
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(5,
                    retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(1, 1000)
                )
// onRetry: (outcome, timeSpan, retryAttempt) =>
// {
// This action runs on each retry attempt.
// Good opportunity to log        
// }
))
.AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>()
                                        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(15)));
// .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapScalarApiReference();

var inventoryItemApi = app.MapGroup("/api/items");

inventoryItemApi.MapPost(string.Empty, async (GrantItemDto grantItem, IRepository<InventoryItem> itemsRepository) =>
{
    var userItemExist = await itemsRepository.GetAsync(item => item.UserId == grantItem.UserId
                                                            && item.CatalogItemId == grantItem.CatalogItemId);

    if (userItemExist == null)
    {
        var inventoryItem = new InventoryItem
        {
            CatalogItemId = grantItem.CatalogItemId,
            UserId = grantItem.UserId,
            Quantity = grantItem.Quantity,
            AcquiredDate = DateTimeOffset.UtcNow
        };

        await itemsRepository.CreateAsync(inventoryItem);
    }
    else
    {
        userItemExist.Quantity += grantItem.Quantity;

        await itemsRepository.UpdateAsync(userItemExist);
    }

    return Results.Ok();
})
.WithName("PostAsync");

inventoryItemApi.MapGet("{userId}", async (Guid userId, IRepository<InventoryItem> itemsRepository, ICatalogClient catalogClient) =>
{
    if (userId == Guid.Empty)
    {
        return Results.BadRequest();
    }

    var catalogItems = await catalogClient.GetCatalogItemsAsync();

    var userItems = (await itemsRepository.GetAllAsync(item => item.UserId == userId))
                            .Join(catalogItems,
                                  inventory => inventory.CatalogItemId,
                                  catalog => catalog.Id,
                                  (inventory, catalog) =>
                                  inventory.AsDto(catalog.Name, catalog.Description)
                                 );

    return Results.Ok(userItems);
})
.WithName("GetAsync");

app.UseHttpsRedirection();

app.Run();