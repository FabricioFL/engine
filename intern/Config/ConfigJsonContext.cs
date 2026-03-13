using System.Text.Json.Serialization;

namespace Engine.Config;

[JsonSerializable(typeof(UiConfig))]
[JsonSerializable(typeof(SceneConfig))]
[JsonSerializable(typeof(SkillsConfig))]
[JsonSerializable(typeof(LogConfig))]
[JsonSerializable(typeof(AssetsConfig))]
internal partial class ConfigJsonContext : JsonSerializerContext
{
}
