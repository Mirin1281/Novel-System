using UnityEngine;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(AudioSource))]
public class BGMManager : SingletonMonoBehaviour<BGMManager>
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] BGMData bgmData;
    LinkedBGM playingLinkedBGM;
    float volume = 1f;

    public AudioSource AudioSource => audioSource;

    protected override void Awake()
    {
        base.Awake();
        GameManager.Instance.OnBGMVolumeChanged += UpdateVolume;
    }

    public void Play(BGMType type, float vol = 1f, bool isLoop = false)
    {
        LinkedBGM linkedBGM = bgmData.GetLinkedBGM(type);
        volume = vol;
        var shapedVolume = linkedBGM.Volume * vol
            * GameManager.Instance.BGMVolume * MyStatic.BGMMasterVolume;
        audioSource.clip = linkedBGM.Clip;
        audioSource.volume = shapedVolume;
        audioSource.loop = isLoop;
        audioSource.Play();
        playingLinkedBGM = linkedBGM;
    }

    public void Stop()
    {
        audioSource.Stop();
        playingLinkedBGM = null;
    }

    /// <summary>
    /// GameManager.BGMVolumeが変化した際に呼ばれる
    /// それ以外は想定してない
    /// </summary>
    void UpdateVolume(float _)
    {
        if (playingLinkedBGM == null) return;
        ChangeVolume(volume);
    }

    public void ChangeVolume(float vol)
    {
        volume = vol;
        var shapedVolume = playingLinkedBGM.Volume * vol
            * GameManager.Instance.BGMVolume * MyStatic.BGMMasterVolume;
        audioSource.volume = shapedVolume;
    }

    /// <summary>
    /// 音量をフェードさせます
    /// </summary>
    /// <param name="endValue"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public async UniTask FadeVolumeAsync(float endValue, float time)
    {
        var startValue = volume;
        var delta = (endValue - startValue) / time;
        var t = 0f;
        while (t < time)
        {
            ChangeVolume(startValue + t * delta);
            t += Time.deltaTime;
            await MyStatic.Yield();
        }
        ChangeVolume(endValue);
    }

    /// <summary>
    /// フェードアウトしたのちBGMを停止します
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public async UniTask FadeOutAsync(float time)
    {
        await FadeVolumeAsync(0f, time);
        Stop();
    }
}
