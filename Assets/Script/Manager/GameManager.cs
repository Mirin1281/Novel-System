using UnityEngine;
using System;
using Random = UnityEngine.Random;
using Novel;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
        MyStatic.Init();
        Random.InitState(DateTime.Now.Millisecond);
#if UNITY_EDITOR
        Application.targetFrameRate = 60;
#endif
        LoadGameData();
    }

    public float DefaultWriteSpeed { get; private set; } = 2;

    void OnDestroy()
    {
        MyStatic.ResetToken();
    }

    void OnApplicationQuit()
    {
        SaveGameData();
    }

    bool isResetDataOnStart = false;
    bool isLoadDataOnStart = false;
    void LoadGameData()
    {
        if (isLoadDataOnStart == false) return;
        var gameData = isResetDataOnStart ? new GameData() : SaveLoad.LoadDataImmediate(0);
        FlagManager.SetFlagDictionary(gameData.FlagDictionary);
        SaveLoad.SaveDataImmediate(gameData, 0);
        SayLogger.SetLog(gameData.LogList);
    }

    bool isSaveDataOnQuit = false;
    void SaveGameData()
    {
        if (isSaveDataOnQuit == false) return;
        var gameData = SaveLoad.LoadDataImmediate(0);
        gameData.FlagDictionary = FlagManager.GetFlagDictionary();
        gameData.LogList = SayLogger.GetLog();
        SaveLoad.SaveDataImmediate(gameData, 0);
    }
}
