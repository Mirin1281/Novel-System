using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class GameManager : SingletonMonoBehaviour<GameManager>, IInitializableManager
{
    void IInitializableManager.Init()
    {
        MyStatic.Init();
        Random.InitState(DateTime.Now.Millisecond);
#if UNITY_EDITOR
        Application.targetFrameRate = 60;
#endif
    }

    public float DefaultWriteSpeed { get; private set; } = 2;

    void OnDestroy()
    {
        MyStatic.ResetToken();
    }
}
