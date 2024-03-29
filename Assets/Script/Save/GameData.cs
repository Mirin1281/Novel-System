using Newtonsoft.Json;
using System.Collections.Generic;

[JsonObject]
public class GameData
{
    [JsonProperty("設定: BGM音量")]
    public float BGMVolume { get; set; } = 0.8f;

    [JsonProperty("設定: SE音量")]
    public float SEVolume { get; set; } = 0.8f;

    [JsonProperty("フラグリスト")]
    public Dictionary<string, object> FlagDictionary { get; set; } = new();
}