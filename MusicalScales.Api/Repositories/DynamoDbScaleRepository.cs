using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using MusicalScales.Api.Models;

namespace MusicalScales.Api.Repositories;

/// <summary>
/// DynamoDB implementation of IScaleRepository
/// </summary>
public class DynamoDbScaleRepository : IScaleRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;
    private readonly JsonSerializerOptions _jsonOptions;

    public DynamoDbScaleRepository(IAmazonDynamoDB dynamoDb, IConfiguration configuration)
    {
        _dynamoDb = dynamoDb;
        _tableName = configuration["DynamoDB:TableName"] ?? "musical-scales";

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<IEnumerable<Scale>> GetAllScalesAsync()
    {
        var request = new ScanRequest
        {
            TableName = _tableName
        };

        var response = await _dynamoDb.ScanAsync(request);
        return response.Items.Select(DeserializeScale).ToList();
    }

    public async Task<Scale?> GetScaleByIdAsync(Guid id)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue { S = id.ToString() } }
            }
        };

        var response = await _dynamoDb.GetItemAsync(request);

        if (!response.IsItemSet || response.Item.Count == 0)
        {
            return null;
        }

        return DeserializeScale(response.Item);
    }

    public async Task<IEnumerable<Scale>> GetScalesByNameAsync(string name)
    {
        // Scan with filter expression for name search
        var request = new ScanRequest
        {
            TableName = _tableName,
            FilterExpression = "contains(#names, :name)",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#names", "NamesSearchable" }
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":name", new AttributeValue { S = name.ToLower() } }
            }
        };

        var response = await _dynamoDb.ScanAsync(request);
        return response.Items.Select(DeserializeScale).ToList();
    }

    public async Task<Scale?> GetScaleByIntervalsAsync(IList<Interval> intervals)
    {
        var intervalsHash = ComputeIntervalsHash(intervals);

        var request = new ScanRequest
        {
            TableName = _tableName,
            FilterExpression = "IntervalsHash = :hash",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":hash", new AttributeValue { S = intervalsHash } }
            }
        };

        var response = await _dynamoDb.ScanAsync(request);

        if (response.Items.Count == 0)
        {
            return null;
        }

        return DeserializeScale(response.Items[0]);
    }

    public async Task<Scale> CreateScaleAsync(Scale scale)
    {
        scale.Id = Guid.NewGuid();
        scale.CreatedAt = DateTime.UtcNow;
        scale.UpdatedAt = DateTime.UtcNow;

        var item = SerializeScale(scale);

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDb.PutItemAsync(request);
        return scale;
    }

    public async Task<Scale?> UpdateScaleAsync(Guid id, Scale scale)
    {
        // Check if scale exists
        var existing = await GetScaleByIdAsync(id);
        if (existing == null)
        {
            return null;
        }

        scale.Id = id;
        scale.CreatedAt = existing.CreatedAt;
        scale.UpdatedAt = DateTime.UtcNow;

        var item = SerializeScale(scale);

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDb.PutItemAsync(request);
        return scale;
    }

    public async Task<bool> DeleteScaleAsync(Guid id)
    {
        var request = new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue { S = id.ToString() } }
            },
            ReturnValues = ReturnValue.ALL_OLD
        };

        var response = await _dynamoDb.DeleteItemAsync(request);
        return response.Attributes.Count > 0;
    }

    public async Task<bool> ScaleExistsAsync(Guid id)
    {
        var scale = await GetScaleByIdAsync(id);
        return scale != null;
    }

    private Dictionary<string, AttributeValue> SerializeScale(Scale scale)
    {
        var intervalsHash = ComputeIntervalsHash(scale.Intervals);
        var namesSearchable = string.Join(" ", scale.Metadata.Names.Select(n => n.ToLower()));

        return new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = scale.Id.ToString() } },
            { "Metadata", new AttributeValue { S = JsonSerializer.Serialize(scale.Metadata, _jsonOptions) } },
            { "Intervals", new AttributeValue { S = JsonSerializer.Serialize(scale.Intervals, _jsonOptions) } },
            { "CreatedAt", new AttributeValue { S = scale.CreatedAt.ToString("o") } },
            { "UpdatedAt", new AttributeValue { S = scale.UpdatedAt.ToString("o") } },
            { "IntervalsHash", new AttributeValue { S = intervalsHash } },
            { "NamesSearchable", new AttributeValue { S = namesSearchable } }
        };
    }

    private Scale DeserializeScale(Dictionary<string, AttributeValue> item)
    {
        return new Scale
        {
            Id = Guid.Parse(item["Id"].S),
            Metadata = JsonSerializer.Deserialize<ScaleMetadata>(item["Metadata"].S, _jsonOptions) ?? new ScaleMetadata(),
            Intervals = JsonSerializer.Deserialize<IList<Interval>>(item["Intervals"].S, _jsonOptions) ?? new List<Interval>(),
            CreatedAt = DateTime.Parse(item["CreatedAt"].S),
            UpdatedAt = DateTime.Parse(item["UpdatedAt"].S)
        };
    }

    private string ComputeIntervalsHash(IList<Interval> intervals)
    {
        // Create a deterministic hash of the interval structure
        var intervalStrings = intervals.Select(i => $"{i.Name}:{i.Quality}:{i.PitchOffset}:{i.SemitoneOffset}");
        return string.Join("|", intervalStrings);
    }
}
