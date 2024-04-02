using UnityEngine;
using System;

namespace Novel
{
    public class NovelManager : SingletonMonoBehaviour<NovelManager>
    {
        protected override void Awake()
        {
            base.Awake();
            MyStatic.Init();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MyStatic.ResetToken();
        }

        public float DefaultWriteSpeed { get; private set; } = 2;

        /*void OnApplicationQuit()
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
        }*/
    }
}