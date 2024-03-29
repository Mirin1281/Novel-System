using Newtonsoft.Json;
using System.Collections.Generic;

[JsonObject]
public class GameData
{
    [JsonProperty("�ݒ�: BGM����")]
    public float BGMVolume { get; set; } = 0.8f;

    [JsonProperty("�ݒ�: SE����")]
    public float SEVolume { get; set; } = 0.8f;

    [JsonProperty("�t���O���X�g")]
    public Dictionary<string, object> FlagDictionary { get; set; } = new();
}