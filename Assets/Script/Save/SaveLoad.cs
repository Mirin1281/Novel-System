using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Cysharp.Threading.Tasks;

public class SaveLoad
{
    readonly static int encryptKey = 0;

    const string GameDataName = "GameData";

    public static async UniTask SaveDataAsync(GameData saveData, int index)
    {
        var filePath = GetDataPath(index);
        var writer = new StreamWriter(filePath, false);
        var serializedJson = JsonConvert.SerializeObject(saveData);
        var encryptedJson = CaesarCipher.Encrypt(serializedJson, encryptKey);
        await writer.WriteAsync(encryptedJson);
        writer.Flush();
        writer.Close();
    }

    public static void SaveDataImmediate(GameData saveData, int index)
    {
        var filePath = GetDataPath(index);
        var writer = new StreamWriter(filePath, false);
        var serializedJson = JsonConvert.SerializeObject(saveData);
        var encryptedJson = CaesarCipher.Encrypt(serializedJson, encryptKey);
        writer.Write(encryptedJson);
        writer.Flush();
        writer.Close();
    }

    public static async UniTask<GameData> LoadDataAsync(int index)
    {
        var filePath = GetDataPath(index);
        if (File.Exists(filePath) == false)
        {
            Debug.LogWarning("ロードするデータが見つかりませんでした");
            return new GameData();
        }
        var reader = new StreamReader(filePath);
        var readString = reader.ReadToEnd();
        reader.Close();
        var decryptedString = CaesarCipher.Decrypt(readString, encryptKey);
        return await UniTask.RunOnThreadPool(() =>
            JsonConvert.DeserializeObject<GameData>(decryptedString));
    }

    public static GameData LoadDataImmediate(int index)
    {
        var filePath = GetDataPath(index);
        if (File.Exists(filePath) == false)
        {
            Debug.LogWarning("ロードするデータが見つかりませんでした");
            return new GameData();
        }
        var reader = new StreamReader(filePath);
        var readString = reader.ReadToEnd();
        reader.Close();
        var decryptedString = CaesarCipher.Decrypt(readString, encryptKey);
        return JsonConvert.DeserializeObject<GameData>(decryptedString);
    }

    static string GetDataPath(int index)
        => $"{Application.streamingAssetsPath}/{GameDataName}{index}.json";
}