using Newtonsoft.Json;
using System.Collections.Generic;
using Novel;

[JsonObject]
public class GameData
{
    [JsonProperty("�ݒ�: BGM����")]
    public float BGMVolume { get; set; } = 0.8f;

    [JsonProperty("�ݒ�: SE����")]
    public float SEVolume { get; set; } = 0.8f;

    [JsonProperty("�t���O���X�g")]
    public Dictionary<string, object> FlagDictionary { get; set; } = new();

    [JsonProperty("���O")]
    public List<(string, string)> LogList { get; set; } = new();
}