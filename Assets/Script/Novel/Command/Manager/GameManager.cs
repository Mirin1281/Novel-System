using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    void Start()
    {
        MyStatic.Init();
        Random.InitState(DateTime.Now.Millisecond);
#if UNITY_EDITOR
        Application.targetFrameRate = 60;
#endif
    }

    public event Action<float> OnBGMVolumeChanged;

    /// <summary>
    /// 0～1で管理
    /// </summary>
    public float BGMVolume
    {
        get => _BGMVolume;
        set
        {
            _BGMVolume = value;
            OnBGMVolumeChanged?.Invoke(value);
        }
    }
    float _BGMVolume = 0.3f;

    /// <summary>
    /// 0～1で管理
    /// </summary>
    public float SEVolume { get; set; } = 0.3f;

    public float DefaultWriteSpeed { get; private set; } = 2;

    void OnDestroy()
    {
        MyStatic.Init();
    }
}
