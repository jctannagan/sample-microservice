using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Play.Catalog.Service.Dto.Dtos
{
    public record ItemDto(Guid Id, string Name, string Description, decimal Price, DateTimeOffset CreatedDate);

    public record CreateItemDto([Required] string Name, string Description, [Range(0, 1000)] decimal Price);

    public record UpdateItemDto([Required] string Name, string Description, [Range(0, 1000)] decimal Price);
}