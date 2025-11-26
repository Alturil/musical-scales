using Microsoft.AspNetCore.Mvc;
using MusicalScales.Api.Models;
using MusicalScales.Api.Services;
using System.ComponentModel.DataAnnotations;

namespace MusicalScales.Api.Controllers;

/// <summary>
/// Controller for managing musical scales
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ScalesController : ControllerBase
{
    private readonly IScaleService _scaleService;
    private readonly ILogger<ScalesController> _logger;

    public ScalesController(IScaleService scaleService, ILogger<ScalesController> logger)
    {
        _scaleService = scaleService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all musical scales
    /// </summary>
    /// <returns>A collection of all scales</returns>
    /// <response code="200">Returns the list of scales</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Scale>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Scale>>> GetAllScales()
    {
        try
        {
            var scales = await _scaleService.GetAllScalesAsync();
            return Ok(scales);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all scales");
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An error occurred while retrieving scales");
        }
    }

    /// <summary>
    /// Gets a specific scale by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the scale</param>
    /// <returns>The requested scale</returns>
    /// <response code="200">Returns the requested scale</response>
    /// <response code="404">If the scale is not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Scale), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Scale>> GetScaleById(Guid id)
    {
        try
        {
            var scale = await _scaleService.GetScaleByIdAsync(id);

            if (scale == null)
            {
                return NotFound($"Scale with ID {id} not found");
            }

            return Ok(scale);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving scale with ID {ScaleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An error occurred while retrieving the scale");
        }
    }

    /// <summary>
    /// Searches for scales by name
    /// </summary>
    /// <param name="name">The name or partial name to search for</param>
    /// <returns>A collection of matching scales</returns>
    /// <response code="200">Returns the matching scales</response>
    /// <response code="400">If the name parameter is invalid</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<Scale>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<Scale>>> SearchScalesByName([FromQuery][Required] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Name parameter cannot be empty or whitespace");
        }

        try
        {
            var scales = await _scaleService.GetScalesByNameAsync(name);
            return Ok(scales);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching scales by name: {Name}", name);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An error occurred while searching for scales");
        }
    }

    /// <summary>
    /// Gets scale pitches starting from a root pitch
    /// </summary>
    /// <param name="id">The unique identifier of the scale</param>
    /// <param name="rootPitch">The root pitch to start the scale from</param>
    /// <returns>The pitches of the scale</returns>
    /// <response code="200">Returns the scale pitches</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the scale is not found</response>
    [HttpPost("{id:guid}/pitches")]
    [ProducesResponseType(typeof(IList<Pitch>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IList<Pitch>>> GetScalePitches(Guid id, [FromBody] Pitch rootPitch)
    {
        if (rootPitch == null)
        {
            return BadRequest("Root pitch is required");
        }

        try
        {
            var scale = await _scaleService.GetScaleByIdAsync(id);

            if (scale == null)
            {
                return NotFound($"Scale with ID {id} not found");
            }

            var pitches = await _scaleService.GetScalePitchesAsync(rootPitch, scale.Intervals);
            return Ok(pitches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting scale pitches for scale {ScaleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An error occurred while generating scale pitches");
        }
    }

    /// <summary>
    /// Creates a new musical scale
    /// </summary>
    /// <param name="scale">The scale to create</param>
    /// <returns>The created scale with its assigned ID</returns>
    /// <response code="201">Returns the newly created scale</response>
    /// <response code="400">If the scale data is invalid</response>
    [HttpPost]
    [ProducesResponseType(typeof(Scale), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Scale>> CreateScale([FromBody] Scale scale)
    {
        if (scale == null)
        {
            return BadRequest("Scale data is required");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdScale = await _scaleService.CreateScaleAsync(scale);
            return CreatedAtAction(nameof(GetScaleById), new { id = createdScale.Id }, createdScale);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new scale");
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An error occurred while creating the scale");
        }
    }

    /// <summary>
    /// Updates an existing musical scale
    /// </summary>
    /// <param name="id">The unique identifier of the scale to update</param>
    /// <param name="scale">The updated scale data</param>
    /// <returns>The updated scale</returns>
    /// <response code="200">Returns the updated scale</response>
    /// <response code="400">If the scale data is invalid</response>
    /// <response code="404">If the scale is not found</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Scale), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Scale>> UpdateScale(Guid id, [FromBody] Scale scale)
    {
        if (scale == null)
        {
            return BadRequest("Scale data is required");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedScale = await _scaleService.UpdateScaleAsync(id, scale);

            if (updatedScale == null)
            {
                return NotFound($"Scale with ID {id} not found");
            }

            return Ok(updatedScale);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating scale with ID {ScaleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An error occurred while updating the scale");
        }
    }

    /// <summary>
    /// Deletes a musical scale
    /// </summary>
    /// <param name="id">The unique identifier of the scale to delete</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">If the scale was successfully deleted</response>
    /// <response code="404">If the scale is not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteScale(Guid id)
    {
        try
        {
            var deleted = await _scaleService.DeleteScaleAsync(id);

            if (!deleted)
            {
                return NotFound($"Scale with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting scale with ID {ScaleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An error occurred while deleting the scale");
        }
    }
}