using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using NexusRpc.Handlers;

namespace NexusRpc.Tests;

public class NexusJsonSerializer : ISerializer
{
    public ConcurrentQueue<object?> SerializeCalls { get; } = new();

    public ConcurrentQueue<(ISerializer.Content Content, Type Type)> DeserializeCalls { get; } = new();

    public async Task<ISerializer.Content> SerializeAsync(object? value)
    {
        SerializeCalls.Enqueue(value);
        // Empty byte array for NoValue
        if (value is NoValue)
        {
            return new([]);
        }
        return new(JsonSerializer.SerializeToUtf8Bytes(value));
    }

    public async Task<object?> DeserializeAsync(ISerializer.Content content, Type type)
    {
        DeserializeCalls.Enqueue((content, type));
        // Short-circuit NoValue
        if (type == typeof(NoValue))
        {
            return default(NoValue);
        }
        return JsonSerializer.Deserialize(Encoding.UTF8.GetString(content.Data), type);
    }
}