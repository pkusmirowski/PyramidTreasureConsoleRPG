using System.Text.Json.Serialization;

namespace PyramidTreasureConsoleRPG.Infrastructure;

/// <summary>Generator źródeł System.Text.Json – bez refleksji, gotowe pod publikację AOT/single-file.</summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SaveData))]
[JsonSerializable(typeof(GameSettings))]
internal sealed partial class GameJsonContext : JsonSerializerContext
{
}
