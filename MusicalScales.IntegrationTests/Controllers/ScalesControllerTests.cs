using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MusicalScales.Api.Data;
using MusicalScales.Api.Models;
using MusicalScales.Api.Models.Enums;
using MusicalScales.IntegrationTests.Fixtures;

namespace MusicalScales.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for the ScalesController endpoints
/// </summary>
public class ScalesControllerTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly MusicalScalesWebApplicationFactory _factory;

    public ScalesControllerTests()
    {
        _factory = new MusicalScalesWebApplicationFactory();
        _client = _factory.CreateClient();
        _jsonOptions = _factory.GetJsonSerializerOptions();
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    #region Helper Methods

    private async Task<StringContent> LoadJsonPayload(string fileName)
    {
        var filePath = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!,
            "TestData",
            "Scales",
            fileName
        );

        var json = await File.ReadAllTextAsync(filePath);
        return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    }

    #endregion

    #region GET /api/scales

    [Fact]
    public async Task GetAllScales_ReturnsSeededScales()
    {
        // Act
        var response = await _client.GetAsync("/api/scales");
        var scales = await response.Content.ReadFromJsonAsync<List<Scale>>(_jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        scales.Should().NotBeNull();
        scales!.Should().HaveCount(11); // DatabaseSeeder seeds 11 scales
        scales.Should().Contain(s => s.Metadata.Names.Any(name => name.Contains("Major")));
    }

    #endregion

    #region GET /api/scales/{id}

    [Fact]
    public async Task GetScaleById_WithValidId_ReturnsScale()
    {
        // Arrange - Get ID from seeded data
        var allScalesResponse = await _client.GetAsync("/api/scales");
        var allScales = await allScalesResponse.Content.ReadFromJsonAsync<List<Scale>>(_jsonOptions);
        var validId = allScales!.First().Id;

        // Act
        var response = await _client.GetAsync($"/api/scales/{validId}");
        var scale = await response.Content.ReadFromJsonAsync<Scale>(_jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        scale.Should().NotBeNull();
        scale!.Id.Should().Be(validId);
    }

    [Fact]
    public async Task GetScaleById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/scales/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/scales/search

    [Fact]
    public async Task SearchScalesByName_WithValidName_ReturnsMatchingScales()
    {
        // Act
        var response = await _client.GetAsync("/api/scales/search?name=Major");
        var scales = await response.Content.ReadFromJsonAsync<List<Scale>>(_jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        scales.Should().NotBeNull();
        scales!.Should().NotBeEmpty();
        scales.Should().OnlyContain(s => s.Metadata.Names.Any(name => name.Contains("Major", StringComparison.OrdinalIgnoreCase)));
    }

    #endregion

    #region POST /api/scales/{id}/pitches

    [Fact]
    public async Task GetScalePitches_WithValidScale_ReturnsCorrectPitches()
    {
        // Arrange - Get ID from seeded data
        var allScalesResponse = await _client.GetAsync("/api/scales");
        var allScales = await allScalesResponse.Content.ReadFromJsonAsync<List<Scale>>(_jsonOptions);
        var scaleId = allScales!.First().Id;
        var rootPitch = new Pitch { Name = DiatonicPitchName.C, Accidental = AccidentalName.Natural };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/scales/{scaleId}/pitches", rootPitch, _jsonOptions);
        var pitches = await response.Content.ReadFromJsonAsync<List<Pitch>>(_jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        pitches.Should().NotBeNull();
        pitches!.Should().NotBeEmpty();
        pitches.Should().Contain(p => p.Name == DiatonicPitchName.C);
    }

    #endregion

    #region POST /api/scales

    [Fact]
    public async Task CreateScale_WithValidJsonPayload_ReturnsCreatedScale()
    {
        // Arrange
        var jsonPayload = await LoadJsonPayload("PentatonicMajor.json");

        // Act
        var response = await _client.PostAsync("/api/scales", jsonPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    #endregion

    #region PUT /api/scales/{id}

    [Fact]
    public async Task UpdateScale_WithValidData_ReturnsUpdatedScale()
    {
        // Arrange - First create a scale to update
        var createPayload = await LoadJsonPayload("Major.json");
        var createResponse = await _client.PostAsync("/api/scales", createPayload);
        var createdScale = await createResponse.Content.ReadFromJsonAsync<Scale>(_jsonOptions);

        // Update the scale
        var updatePayload = await LoadJsonPayload("PentatonicMinor.json");

        // Act
        var response = await _client.PutAsync($"/api/scales/{createdScale!.Id}", updatePayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region DELETE /api/scales/{id}

    [Fact]
    public async Task DeleteScale_WithValidId_ReturnsNoContent()
    {
        // Arrange - Create a scale first
        var createPayload = await LoadJsonPayload("PentatonicMajor.json");
        var createResponse = await _client.PostAsync("/api/scales", createPayload);
        var createdScale = await createResponse.Content.ReadFromJsonAsync<Scale>(_jsonOptions);

        // Act
        var response = await _client.DeleteAsync($"/api/scales/{createdScale!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's deleted
        var getResponse = await _client.GetAsync($"/api/scales/{createdScale.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}