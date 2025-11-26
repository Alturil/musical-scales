using FluentAssertions;
using Moq;
using MusicalScales.Api.Models;
using MusicalScales.Api.Models.Enums;
using MusicalScales.Api.Repositories;
using MusicalScales.Api.Services;

namespace MusicalScales.Tests.Services;

public class ScaleServiceTests
{
    private readonly Mock<IScaleRepository> _mockScaleRepository;
    private readonly Mock<IPitchService> _mockPitchService;
    private readonly Mock<IIntervalService> _mockIntervalService;
    private readonly ScaleService _scaleService;

    public ScaleServiceTests()
    {
        _mockScaleRepository = new Mock<IScaleRepository>();
        _mockPitchService = new Mock<IPitchService>();
        _mockIntervalService = new Mock<IIntervalService>();
        _scaleService = new ScaleService(_mockScaleRepository.Object, _mockPitchService.Object, _mockIntervalService.Object);
    }

    [Fact]
    public async Task GetScalePitchesAsync_WithValidInputs_ReturnsCorrectPitches()
    {
        // Arrange
        var rootPitch = new Pitch
        {
            Name = DiatonicPitchName.C,
            Accidental = AccidentalName.Natural,
            PitchOffset = 0,
            SemitoneOffset = 0
        };

        var intervals = new List<Interval>
        {
            new Interval
            {
                Name = IntervalSizeName.Second,
                Quality = IntervalQualityName.Major,
                PitchOffset = 1,
                SemitoneOffset = 2
            },
            new Interval
            {
                Name = IntervalSizeName.Third,
                Quality = IntervalQualityName.Major,
                PitchOffset = 2,
                SemitoneOffset = 4
            }
        };

        var expectedSecondPitch = new Pitch
        {
            Name = DiatonicPitchName.D,
            Accidental = AccidentalName.Natural,
            PitchOffset = 1,
            SemitoneOffset = 2
        };

        var expectedThirdPitch = new Pitch
        {
            Name = DiatonicPitchName.E,
            Accidental = AccidentalName.Natural,
            PitchOffset = 2,
            SemitoneOffset = 4
        };

        _mockPitchService
            .SetupSequence(x => x.GetPitch(It.IsAny<Pitch>(), It.IsAny<Interval>()))
            .Returns(expectedSecondPitch)
            .Returns(expectedThirdPitch);

        // Act
        var result = await _scaleService.GetScalePitchesAsync(rootPitch, intervals);

        // Assert
        result.Should().HaveCount(3); // Root + 2 intervals
        result[0].Should().BeEquivalentTo(rootPitch);
        result[1].Should().BeEquivalentTo(expectedSecondPitch);
        result[2].Should().BeEquivalentTo(expectedThirdPitch);

        // Verify first interval is calculated from root pitch
        _mockPitchService.Verify(x => x.GetPitch(rootPitch, intervals[0]), Times.Once);
        // Verify second interval is calculated from the previous pitch (D), not root
        _mockPitchService.Verify(x => x.GetPitch(expectedSecondPitch, intervals[1]), Times.Once);
    }

    [Fact]
    public async Task GetScalePitchesAsync_WithEmptyIntervals_ReturnsOnlyRootPitch()
    {
        // Arrange
        var rootPitch = new Pitch
        {
            Name = DiatonicPitchName.C,
            Accidental = AccidentalName.Natural,
            PitchOffset = 0,
            SemitoneOffset = 0
        };

        var intervals = new List<Interval>();

        // Act
        var result = await _scaleService.GetScalePitchesAsync(rootPitch, intervals);

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(rootPitch);
        _mockPitchService.Verify(x => x.GetPitch(It.IsAny<Pitch>(), It.IsAny<Interval>()), Times.Never);
    }

    [Fact]
    public async Task GetAllScalesAsync_CallsRepository()
    {
        // Arrange
        var expectedScales = new List<Scale>
        {
            new Scale
            {
                Id = Guid.NewGuid(),
                Metadata = new ScaleMetadata { Names = ["Major"] },
                Intervals = []
            }
        };

        _mockScaleRepository
            .Setup(x => x.GetAllScalesAsync())
            .ReturnsAsync(expectedScales);

        // Act
        var result = await _scaleService.GetAllScalesAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedScales);
        _mockScaleRepository.Verify(x => x.GetAllScalesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetScaleByIdAsync_CallsRepository()
    {
        // Arrange
        var scaleId = Guid.NewGuid();
        var expectedScale = new Scale
        {
            Id = scaleId,
            Metadata = new ScaleMetadata { Names = ["Major"] },
            Intervals = []
        };

        _mockScaleRepository
            .Setup(x => x.GetScaleByIdAsync(scaleId))
            .ReturnsAsync(expectedScale);

        // Act
        var result = await _scaleService.GetScaleByIdAsync(scaleId);

        // Assert
        result.Should().BeEquivalentTo(expectedScale);
        _mockScaleRepository.Verify(x => x.GetScaleByIdAsync(scaleId), Times.Once);
    }

    [Fact]
    public async Task GetScalesByNameAsync_CallsRepository()
    {
        // Arrange
        var scaleName = "Major";
        var expectedScales = new List<Scale>
        {
            new Scale
            {
                Id = Guid.NewGuid(),
                Metadata = new ScaleMetadata { Names = [scaleName] },
                Intervals = []
            }
        };

        _mockScaleRepository
            .Setup(x => x.GetScalesByNameAsync(scaleName))
            .ReturnsAsync(expectedScales);

        // Act
        var result = await _scaleService.GetScalesByNameAsync(scaleName);

        // Assert
        result.Should().BeEquivalentTo(expectedScales);
        _mockScaleRepository.Verify(x => x.GetScalesByNameAsync(scaleName), Times.Once);
    }

    [Fact]
    public async Task GetScaleByIntervalsAsync_CallsRepository()
    {
        // Arrange
        var intervals = new List<Interval>
        {
            new Interval
            {
                Name = IntervalSizeName.Second,
                Quality = IntervalQualityName.Major,
                PitchOffset = 1,
                SemitoneOffset = 2
            }
        };

        var expectedScale = new Scale
        {
            Id = Guid.NewGuid(),
            Metadata = new ScaleMetadata { Names = ["Major"] },
            Intervals = intervals
        };

        _mockScaleRepository
            .Setup(x => x.GetScaleByIntervalsAsync(intervals))
            .ReturnsAsync(expectedScale);

        // Act
        var result = await _scaleService.GetScaleByIntervalsAsync(intervals);

        // Assert
        result.Should().BeEquivalentTo(expectedScale);
        _mockScaleRepository.Verify(x => x.GetScaleByIntervalsAsync(intervals), Times.Once);
    }

    [Fact]
    public async Task CreateScaleAsync_WithValidScale_CallsRepository()
    {
        // Arrange
        var scale = new Scale
        {
            Id = Guid.NewGuid(),
            Metadata = new ScaleMetadata { Names = ["Test Scale"] },
            Intervals =
            [
                new Interval
                {
                    Name = IntervalSizeName.Second,
                    Quality = IntervalQualityName.Major,
                    PitchOffset = 1,
                    SemitoneOffset = 2
                }
            ]
        };

        _mockScaleRepository
            .Setup(x => x.CreateScaleAsync(scale))
            .ReturnsAsync(scale);

        // Act
        var result = await _scaleService.CreateScaleAsync(scale);

        // Assert
        result.Should().BeEquivalentTo(scale);
        _mockScaleRepository.Verify(x => x.CreateScaleAsync(scale), Times.Once);
    }

    [Fact]
    public async Task CreateScaleAsync_WithNoNames_ThrowsArgumentException()
    {
        // Arrange
        var scale = new Scale
        {
            Id = Guid.NewGuid(),
            Metadata = new ScaleMetadata { Names = [] }, // Empty names
            Intervals =
            [
                new Interval
                {
                    Name = IntervalSizeName.Second,
                    Quality = IntervalQualityName.Major,
                    PitchOffset = 1,
                    SemitoneOffset = 2
                }
            ]
        };

        // Act & Assert
        await _scaleService.Invoking(s => s.CreateScaleAsync(scale))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Scale must have at least one name");

        _mockScaleRepository.Verify(x => x.CreateScaleAsync(It.IsAny<Scale>()), Times.Never);
    }

    [Fact]
    public async Task CreateScaleAsync_WithNullNames_ThrowsArgumentException()
    {
        // Arrange
        var scale = new Scale
        {
            Id = Guid.NewGuid(),
            Metadata = new ScaleMetadata { Names = null! }, // Null names
            Intervals =
            [
                new Interval
                {
                    Name = IntervalSizeName.Second,
                    Quality = IntervalQualityName.Major,
                    PitchOffset = 1,
                    SemitoneOffset = 2
                }
            ]
        };

        // Act & Assert
        await _scaleService.Invoking(s => s.CreateScaleAsync(scale))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Scale must have at least one name");

        _mockScaleRepository.Verify(x => x.CreateScaleAsync(It.IsAny<Scale>()), Times.Never);
    }

    [Fact]
    public async Task CreateScaleAsync_WithNullMetadata_ThrowsArgumentException()
    {
        // Arrange
        var scale = new Scale
        {
            Id = Guid.NewGuid(),
            Metadata = null!, // Null metadata
            Intervals =
            [
                new Interval
                {
                    Name = IntervalSizeName.Second,
                    Quality = IntervalQualityName.Major,
                    PitchOffset = 1,
                    SemitoneOffset = 2
                }
            ]
        };

        // Act & Assert
        await _scaleService.Invoking(s => s.CreateScaleAsync(scale))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Scale must have at least one name");

        _mockScaleRepository.Verify(x => x.CreateScaleAsync(It.IsAny<Scale>()), Times.Never);
    }

    [Fact]
    public async Task CreateScaleAsync_WithNoIntervals_ThrowsArgumentException()
    {
        // Arrange
        var scale = new Scale
        {
            Id = Guid.NewGuid(),
            Metadata = new ScaleMetadata { Names = ["Test Scale"] },
            Intervals = [] // Empty intervals
        };

        // Act & Assert
        await _scaleService.Invoking(s => s.CreateScaleAsync(scale))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Scale must have at least one interval");

        _mockScaleRepository.Verify(x => x.CreateScaleAsync(It.IsAny<Scale>()), Times.Never);
    }

    [Fact]
    public async Task CreateScaleAsync_WithNullIntervals_ThrowsArgumentException()
    {
        // Arrange
        var scale = new Scale
        {
            Id = Guid.NewGuid(),
            Metadata = new ScaleMetadata { Names = ["Test Scale"] },
            Intervals = null! // Null intervals
        };

        // Act & Assert
        await _scaleService.Invoking(s => s.CreateScaleAsync(scale))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Scale must have at least one interval");

        _mockScaleRepository.Verify(x => x.CreateScaleAsync(It.IsAny<Scale>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public async Task CreateScaleAsync_WithEmptyOrWhitespaceNames_ThrowsArgumentException(string invalidName)
    {
        // Arrange
        var scale = new Scale
        {
            Id = Guid.NewGuid(),
            Metadata = new ScaleMetadata { Names = ["Valid Name", invalidName] },
            Intervals =
            [
                new Interval
                {
                    Name = IntervalSizeName.Second,
                    Quality = IntervalQualityName.Major,
                    PitchOffset = 1,
                    SemitoneOffset = 2
                }
            ]
        };

        // Act & Assert
        await _scaleService.Invoking(s => s.CreateScaleAsync(scale))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Scale names cannot be empty or whitespace");

        _mockScaleRepository.Verify(x => x.CreateScaleAsync(It.IsAny<Scale>()), Times.Never);
    }

    [Fact]
    public async Task UpdateScaleAsync_WithValidScale_CallsRepository()
    {
        // Arrange
        var scaleId = Guid.NewGuid();
        var scale = new Scale
        {
            Id = scaleId,
            Metadata = new ScaleMetadata { Names = ["Updated Scale"] },
            Intervals =
            [
                new Interval
                {
                    Name = IntervalSizeName.Second,
                    Quality = IntervalQualityName.Major,
                    PitchOffset = 1,
                    SemitoneOffset = 2
                }
            ]
        };

        _mockScaleRepository
            .Setup(x => x.UpdateScaleAsync(scaleId, scale))
            .ReturnsAsync(scale);

        // Act
        var result = await _scaleService.UpdateScaleAsync(scaleId, scale);

        // Assert
        result.Should().BeEquivalentTo(scale);
        _mockScaleRepository.Verify(x => x.UpdateScaleAsync(scaleId, scale), Times.Once);
    }

    [Fact]
    public async Task UpdateScaleAsync_WithInvalidScale_ThrowsArgumentException()
    {
        // Arrange
        var scaleId = Guid.NewGuid();
        var scale = new Scale
        {
            Id = scaleId,
            Metadata = new ScaleMetadata { Names = [] }, // Invalid: no names
            Intervals =
            [
                new Interval
                {
                    Name = IntervalSizeName.Second,
                    Quality = IntervalQualityName.Major,
                    PitchOffset = 1,
                    SemitoneOffset = 2
                }
            ]
        };

        // Act & Assert
        await _scaleService.Invoking(s => s.UpdateScaleAsync(scaleId, scale))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Scale must have at least one name");

        _mockScaleRepository.Verify(x => x.UpdateScaleAsync(It.IsAny<Guid>(), It.IsAny<Scale>()), Times.Never);
    }

    [Fact]
    public async Task DeleteScaleAsync_CallsRepository()
    {
        // Arrange
        var scaleId = Guid.NewGuid();

        _mockScaleRepository
            .Setup(x => x.DeleteScaleAsync(scaleId))
            .ReturnsAsync(true);

        // Act
        var result = await _scaleService.DeleteScaleAsync(scaleId);

        // Assert
        result.Should().BeTrue();
        _mockScaleRepository.Verify(x => x.DeleteScaleAsync(scaleId), Times.Once);
    }
}