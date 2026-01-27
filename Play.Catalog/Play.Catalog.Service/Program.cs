using MassTransit;
using Play.Catalog.Contracts;
using Play.Catalog.Service;
using Play.Catalog.Service.Dto;
using Play.Catalog.Service.Entities;
using Play.Common;
using Play.Common.Messaging;
using Play.Common.MongoDB;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddMongo().AddMongoRepository<Item>("items");
builder.Services.AddMessaging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapScalarApiReference();

var itemsApi = app.MapGroup("/api/items");

itemsApi.MapGet(string.Empty, async (IRepository<Item> itemsRepository) =>
{
    var items = (await itemsRepository.GetAllAsync()).Select(item => item.AsDto());
    return Results.Ok(items);
})
.WithName("GetItems");

itemsApi.MapGet("{id}", async (Guid id, IRepository<Item> itemsRepository) =>
{
    var item = await itemsRepository.GetAsync(id);
    if (item is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(item.AsDto());
})
.WithName("GetItemById");

itemsApi.MapPost(string.Empty, async (CreateItemDto createItem, IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint) =>
{
    var item = new Item {
        Name = createItem.Name,
        Description = createItem.Description,
        Price = createItem.Price,
        CreatedDate = DateTimeOffset.UtcNow
    };

    await itemsRepository.CreateAsync(item);
    // This publishes message to MassTransit
    await publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

    return Results.CreatedAtRoute(routeName: "GetItemById",
                                  routeValues: new { Id = item.Id },
                                  value: item);
})
.WithName("CreateItem");;

itemsApi.MapPut("{id}", async (Guid id, UpdateItemDto updateItem, IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint) =>
{
    var itemToUpdate = await itemsRepository.GetAsync(id);
    if (itemToUpdate is null)
    {
        return Results.NotFound();
    }

    itemToUpdate.Name = updateItem.Name;
    itemToUpdate.Description = updateItem.Description;
    itemToUpdate.Price = updateItem.Price;
    
    await itemsRepository.UpdateAsync(itemToUpdate);
    await publishEndpoint.Publish(new CatalogItemUpdated(itemToUpdate.Id, itemToUpdate.Name, itemToUpdate.Description));

    return Results.NoContent();
})
.WithName("UpdateItem");;

itemsApi.MapDelete("{id}", async (Guid id, IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint) =>
{
    var item = await itemsRepository.GetAsync(id);
    if (item is null)
    {
        return Results.NotFound();
    }

    await itemsRepository.DeleteAsync(id);
    await publishEndpoint.Publish(new CatalogItemDeleted(id));
    return Results.NoContent();
})
.WithName("DeleteItem");

app.Run();


public static class MyRequests
{
    public static int Counter { get; set; } = 0;
}