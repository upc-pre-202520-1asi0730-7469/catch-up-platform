using System.Net.Mime;
using CatchUpPlatform.API.News.Domain.Model.Queries;
using CatchUpPlatform.API.News.Domain.Services;
using CatchUpPlatform.API.News.Interfaces.REST.Resources;
using CatchUpPlatform.API.News.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CatchUpPlatform.API.News.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Favorite Sources")]
public class FavoriteSourcesController(
    IFavoriteSourceCommandService favoriteSourceCommandService,
    IFavoriteSourceQueryService favoriteSourceQueryService
    ) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a favorite source",
        Description = "Creates a new favorite source for the authenticated user.",
        OperationId = "CreateFavoriteSource")]
    [SwaggerResponse(201, "Favorite source created successfully", typeof(FavoriteSourceResource))]
    [SwaggerResponse(400, "Bad request", typeof(BadRequestResult))]
    public async Task<IActionResult> CreateFavoriteSource([FromBody] CreateFavoriteSourceResource resource)
    {
        var createFavoriteSourceCommand =
            CreateFavoriteSourceCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await favoriteSourceCommandService.Handle(createFavoriteSourceCommand);
        if (result is null) return BadRequest();
        var createdFavoriteSourceResource = FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity(result);
        return CreatedAtAction(nameof(GetFavoriteSourceById), new { id = result.Id }, createdFavoriteSourceResource);
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Get favorite source by ID",
        Description = "Retrieves a favorite source by its unique identifier.",
        OperationId = "GetFavoriteSourceById")]
    [SwaggerResponse(200, "Favorite source retrieved successfully", typeof(FavoriteSourceResource))]
    [SwaggerResponse(404, "Favorite source not found", typeof(NotFoundResult))]
    public async Task<IActionResult> GetFavoriteSourceById([FromRoute] int id)
    {
        var getFavoriteSourceByIdQuery = new GetFavoriteSourceByIdQuery(id);
        var result = await favoriteSourceQueryService.Handle(getFavoriteSourceByIdQuery);
        if (result is null) return NotFound();
        var favoriteSourceResource = FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity(result);
        return Ok(favoriteSourceResource);
    }
}