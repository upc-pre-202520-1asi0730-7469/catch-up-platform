using System.Net.Mime;
using CatchUpPlatform.API.News.Domain.Model.Queries;
using CatchUpPlatform.API.News.Domain.Services;
using CatchUpPlatform.API.News.Interfaces.REST.Resources;
using CatchUpPlatform.API.News.Interfaces.REST.Transform;
using CatchUpPlatform.API.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace CatchUpPlatform.API.News.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Favorite Sources")]
public class FavoriteSourcesController(
    IFavoriteSourceCommandService favoriteSourceCommandService,
    IFavoriteSourceQueryService favoriteSourceQueryService,
    IStringLocalizer<SharedResource> localizer) : ControllerBase
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
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var createFavoriteSourceCommand =
            CreateFavoriteSourceCommandFromResourceAssembler.ToCommandFromResource(resource);
        try
        {
            var result = await favoriteSourceCommandService.Handle(createFavoriteSourceCommand);
            if (result is null) return Conflict(localizer["NewsFavoriteSourceDuplicated"].Value);
            var createdFavoriteSourceResource = FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity(result);
            return CreatedAtAction(nameof(GetFavoriteSourceById), new { id = result.Id },
                createdFavoriteSourceResource);
        }
        catch (Exception ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(localizer["NewsFavoriteSourceDuplicated"].Value);
        }
        catch
        {
            return BadRequest();
        }
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

    private async Task<IActionResult> GetAllFavoriteSourcesByNewsApiKey(string newsApiKey)
    {
        var getAllFavoriteSourcesByNewsApiKeyQuery = new GetAllFavoriteSourcesByNewsApiKeyQuery(newsApiKey);
        var result = await favoriteSourceQueryService.Handle(getAllFavoriteSourcesByNewsApiKeyQuery);
        var resources = result.Select(FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    private async Task<IActionResult> GetFavoriteSourceByNewsApiKeyAndSourceId(string newsApiKey, string sourceId)
    {
        var getFavoriteSourceByNewsApiKeyAndSourceIdQuery =
            new GetFavoriteSourceByNewsApiKeyAndSourceIdQuery(newsApiKey, sourceId);
        var result = await favoriteSourceQueryService
            .Handle(getFavoriteSourceByNewsApiKeyAndSourceIdQuery);
        if (result is null) return NotFound();
        var resource = FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity(result);
        return Ok(resource);
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get favorite sources",
        Description = "Retrieves favorite sources based on the provided query parameters.",
        OperationId = "GetFavoriteSourcesFromQuery")]
    [SwaggerResponse(200, "Favorite sources retrieved successfully", typeof(FavoriteSourceResource))]
    public async Task<IActionResult> GetFavoriteSourcesFromQuery(
        [FromQuery] string newsApiKey,
        [FromQuery] string sourceId = "")
    {
        return string.IsNullOrEmpty(sourceId)
            ? await GetAllFavoriteSourcesByNewsApiKey(newsApiKey)
            : await GetFavoriteSourceByNewsApiKeyAndSourceId(newsApiKey, sourceId);
    }

}