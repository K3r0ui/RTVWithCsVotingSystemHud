using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace RtvWithCsVotingSystem;

public class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("PluginMode")] public int PluginMode { get; set; } = 0;
}