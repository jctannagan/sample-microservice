using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dto.Dtos;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddValidation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapScalarApiReference();

List<ItemDto> itemDtos = new()
{
    new ItemDto(Guid.NewGuid(), "Potion", "Restores a small amount of HP", 5, DateTimeOffset.UtcNow),
    new ItemDto(Guid.NewGuid(), "Antidote", "Cures any poison effect", 7, DateTimeOffset.UtcNow),
    new ItemDto(Guid.NewGuid(), "Wooden sword", "Deals a small amount of damage", 15, DateTimeOffset.UtcNow),
};

app.MapGet("items", () =>
{
    return itemDtos;
})
.WithName("GetItems");

app.MapGet("items/{id}", (Guid id) =>
{
    var item = itemDtos.SingleOrDefault(p => p.Id == id);
    if (item is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(item);
})
.WithName("GetItemById");

app.MapPost("items", (CreateItemDto createItem) =>
{
    var item = new ItemDto(Guid.NewGuid(), createItem.Name, createItem.Description, createItem.Price, DateTimeOffset.UtcNow);
    itemDtos.Add(item);

    return Results.CreatedAtRoute(routeName: "GetItemById",
                                  routeValues: new { Id = item.Id },
                                  value: item);
});

app.MapPut("items/{id}", (Guid id, UpdateItemDto updateItem) =>
{
    var itemToUpdate = itemDtos.SingleOrDefault(x => x.Id == id);
    if (itemToUpdate is null)
    {
        return Results.NotFound();
    }

    var updatedItemValues = itemToUpdate with
    {
        Name = updateItem.Name,
        Description = updateItem.Description,
        Price = updateItem.Price
    };

    var index = itemDtos.FindIndex(existingItem => existingItem.Id == id);
    itemDtos[index] = updatedItemValues;

    return Results.NoContent();
});

app.MapDelete("items/{id}", (Guid id) =>
{
    var index = itemDtos.FindIndex(existingItem => existingItem.Id == id);
    if (index == -1)
    {
        return Results.NotFound();
    }

    itemDtos.RemoveAt(index);

    return Results.NoContent();
});

app.Run();