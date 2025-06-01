using System.Text.Json;
using System.Text.Json.Serialization;
using PicView.Core.DebugTools;

namespace PicView.Core.FileHandling;

public static class JsonFileHelper
{
    public static async Task WriteJsonAsync(string path, object? value, Type inputType, JsonSerializerContext context)
    {
        if (value is null || inputType is null || context is null)
        {
            DebugHelper.LogDebug(nameof(JsonFileHelper), nameof(WriteJsonAsync), "Types are null");
            return;
        }

        var contents = JsonSerializer.Serialize(value, inputType, context);
        await File.WriteAllTextAsync(path, contents).ConfigureAwait(false);
    }
}