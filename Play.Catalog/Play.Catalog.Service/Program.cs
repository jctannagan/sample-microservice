using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Play.Catalog.Service;
using Play.Catalog.Service.Dto.Dtos;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Repositories;
using Play.Catalog.Service.Settings;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddMongo();
builder.Services.AddMongoRepository<Item>("items");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapScalarApiReference();

app.MapGet("items", async (IRepository<Item> itemsRepository) =>
{
    var items = (await itemsRepository.GetAllAsync()).Select(item => item.AsDto());
    return items;
})
.WithName("GetItems");

app.MapGet("items/{id}", async (Guid id, IRepository<Item> itemsRepository) =>
{
    var item = await itemsRepository.GetAsync(id);
    if (item is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(item.AsDto());
})
.WithName("GetItemById");

app.MapPost("items", async (CreateItemDto createItem, IRepository<Item> itemsRepository) =>
{
    var item = new Item {
        Name = createItem.Name,
        Description = createItem.Description,
        Price = createItem.Price,
        CreatedDate = DateTimeOffset.UtcNow
    };

    await itemsRepository.CreateAsync(item);

    return Results.CreatedAtRoute(routeName: "GetItemById",
                                  routeValues: new { Id = item.Id },
                                  value: item);
})
.WithName("CreateItem");;

app.MapPut("items/{id}", async (Guid id, UpdateItemDto updateItem, IRepository<Item> itemsRepository) =>
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

    return Results.NoContent();
})
.WithName("UpdateItem");;

app.MapDelete("items/{id}", async (Guid id, IRepository<Item> itemsRepository) =>
{
    var item = await itemsRepository.GetAsync(id);
    if (item is null)
    {
        return Results.NotFound();
    }

    await itemsRepository.DeleteAsync(id);

    return Results.NoContent();
})
.WithName("DeleteItem");;

app.Run();